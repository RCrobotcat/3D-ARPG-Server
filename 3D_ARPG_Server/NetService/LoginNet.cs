using PEUtils;
using RC_IOCPNet;
using RCCommon;
using RCProtocol;
using System.Collections.Concurrent;

// 登录进程网络服务
namespace ARPGServer
{
    class BattleAccessToken
    {
        public int uid;
        public string accessToken;
        public RoleData roleData;
        public DateTime validationTime; // token有效期, 过期后需要重新登录
    }

    public class LoginNet : ILogic
    {
        public IOCPNet<LoginToken, NetMsg> loginNet;

        readonly Dictionary<string, LoginToken> Account2Tokens = new();
        readonly Dictionary<string, List<RoleData>> Account2Roles = new();
        Dictionary<int, BattleAccessToken> battleAccessTokens = new();

        readonly ConcurrentQueue<LoginPackage> serverPackages = new();
        readonly Dictionary<CMD, Action<LoginPackage>> serverHandlers = new();

        public void Awake()
        {
            IOCPTool.LogFunc = PELog.Log;
            IOCPTool.WarnFunc = PELog.Warn;
            IOCPTool.ErrorFunc = PELog.Error;
            IOCPTool.ColorLogFunc = (color, msg) => { PELog.ColorLog((LogColor)color, msg); };

            AddServerHandler(CMD.OnClient2LoginConnected, OnClient2LoginConnected);
            AddServerHandler(CMD.OnClient2LoginDisConnected, OnClient2LoginDisConnected);
            AddServerHandler(CMD.ReqAccountLogin, ReqAccountLogin);
            AddServerHandler(CMD.ReqRoleToken, ReqRoleToken);

            loginNet = new();
            loginNet.StartAsServer("127.0.0.1", 18000, 5000);
        }
        public void Update()
        {
            while (!serverPackages.IsEmpty)
            {
                if (serverPackages.TryDequeue(out LoginPackage package))
                {
                    if (serverHandlers.TryGetValue(package.message.cmd, out Action<LoginPackage> handlerCb))
                    {
                        handlerCb.Invoke(package);
                    }
                }
            }
        }
        public void Destroy()
        {
            loginNet.CloseServer();
            loginNet = null;
            serverPackages.Clear();
            serverHandlers.Clear();
        }

        //-------------Tool Functions-------------//
        public void AddServerPackages(LoginPackage package)
        {
            serverPackages.Enqueue(package);
        }
        public void AddServerHandler(CMD cmd, Action<LoginPackage> handlerCb)
        {
            if (!serverHandlers.ContainsKey(cmd))
            {
                serverHandlers.Add(cmd, handlerCb);
            }
            else
            {
                this.Error($"Command: {cmd} already exists in server handler!");
            }
        }

        void OnClient2LoginConnected(LoginPackage package)
        {
            this.LogGreen($"{package.token.tokenID} new client connected!");
        }

        void OnClient2LoginDisConnected(LoginPackage package)
        {
            this.LogRed($"{package.token.tokenID} client disconnected!");
        }

        int tempUid = 1001;
        public int TempUid
        {
            get => tempUid++;
            set => tempUid = value;
        }
        void ReqAccountLogin(LoginPackage package)
        {
            ReqAccountLogin reqAccountLogin = package.message.reqAccountLogin;
            NetMsg response = new(CMD.RespAccountLogin);
            if (!Account2Tokens.ContainsKey(reqAccountLogin.account))
            {
                Account2Tokens.Add(reqAccountLogin.account, package.token); // cache the account and token

                // SDK校验, DataBase查询 TODO
            }
            else
            {
                // 已经登录过了 Already logged in
                response.errorCode = ErrorCode.acct_online_login;
            }
            package.token.SendMsg(response);

            if (response.errorCode == ErrorCode.None)
            {
                // 通知跳转场景 Notify Enter Stage
                this.LogGreen($"Login Account: {reqAccountLogin.account}.");

                package.token.SendMsg(new NetMsg
                {
                    cmd = CMD.NtfEnterStage,
                    ntfEnterStage = new NtfEnterStage
                    {
                        // stageID = 2
                        mode = EnterStageMode.Login,
                        stageName = "TestScene"
                    }
                });

                package.token.SendMsg(new NetMsg
                {
                    cmd = CMD.InstantiateRole,
                    instantiateRole = new InstantiateRole
                    {
                        roleID = GetRoleID(),
                        PosX = 1,
                        PosZ = 1
                    }
                });
            }
        }
        int roleId = 101;
        public int GetRoleID()
        {
            return roleId++;
        }

        // 请求角色Token Request Role Token
        void ReqRoleToken(LoginPackage package)
        {
            ReqRoleToken request = package.message.reqRoleToken;
            int uid = request.selectedUid;
            RoleData selectedRoleData = null;
            NetMsg response = new() { cmd = CMD.RespRoleToken };
            if (Account2Roles.TryGetValue(request.account, out List<RoleData> roles))
            {
                selectedRoleData = roles.Find(role => role.uid == uid);
                if (!battleAccessTokens.TryGetValue(selectedRoleData.uid, out BattleAccessToken accessToken))
                {
                    accessToken = new()
                    {
                        uid = selectedRoleData.uid,
                        accessToken = PECalculateTool.GetMD5Hash(uid.ToString() + DateTime.Now),
                        roleData = selectedRoleData,
                        validationTime = DateTime.Now.AddMinutes(5) // 5分钟有效期 5 minutes validity period
                    };
                    battleAccessTokens.Add(uid, accessToken);
                }

                response.respRoleToken = new RespRoleToken
                {
                    battleIP = "127.0.0.1",
                    battlePort = 19000,
                    token = accessToken.accessToken
                };
            }
            else
            {
                response.errorCode = ErrorCode.acct_not_exist;
            }

            // Send response to client.
            package.token.SendMsg(response);
        }

        // Note: 模拟数据
        public Tuple<ErrorCode, RoleData> GetTokenRoleData(ReqTokenAccess request)
        {
            if (battleAccessTokens.TryGetValue(request.uid, out BattleAccessToken token))
            {
                if (token.accessToken == request.token)
                {
                    if ((token.validationTime - DateTime.Now).TotalSeconds > 0)
                    {
                        return new Tuple<ErrorCode, RoleData>(ErrorCode.None, token.roleData);
                    }
                    else
                    {
                        return new Tuple<ErrorCode, RoleData>(ErrorCode.token_expired, null);
                    }
                }
                else
                {
                    return new Tuple<ErrorCode, RoleData>(ErrorCode.token_error, null);
                }
            }
            else
            {
                return new Tuple<ErrorCode, RoleData>(ErrorCode.token_not_exist, null);
            }
        }

        /// <summary>
        /// 下发角色数据
        /// </summary>
        public void RespRoleData(LoginPackage package)
        {
            ReqAccountLogin reqAccountLogin = package.message.reqAccountLogin;

            if (!Account2Roles.TryGetValue(reqAccountLogin.account, out List<RoleData> roles))
            {
                roles = new List<RoleData>()
                    {
                        new ()
                            {
                                uid = TempUid, // 临时模拟uid
                                nickName = $"{reqAccountLogin.account}_{tempUid}",
                                posX = 1,
                                posZ = 1,
                                dirX = 0,
                                dirZ = 0,
                                lastRid = PECalculateTool.GetWorldID(1, 101),
                                lastTid = 0,

                                unitID = 101,
                                level = 66,
                                exp = 777
                            },
                        new ()
                            {
                                uid = TempUid, // 临时模拟uid
                                nickName = $"{reqAccountLogin.account}_{tempUid}",
                                posX = 1,
                                posZ = 1,
                                dirX = 0,
                                dirZ = 0,
                                lastRid = PECalculateTool.GetWorldID(1, 101),
                                lastTid = 0,

                                unitID = 101,
                                level = 77,
                                exp = 666
                            }
                    };
                Account2Roles.Add(reqAccountLogin.account, roles);
            }

            // 下发角色数据 Send Role Data
            package.token.SendMsg(new NetMsg
            {
                cmd = CMD.NtfRoleData,
                errorCode = package.message.errorCode,
                ntfRoleData = new NtfRoleData
                {
                    roleData = roles
                }
            });
        }
    }

    public class LoginPackage
    {
        public LoginToken token;
        public NetMsg message;
        public LoginPackage(LoginToken token, NetMsg message)
        {
            this.token = token;
            this.message = message;
        }
    }
}
