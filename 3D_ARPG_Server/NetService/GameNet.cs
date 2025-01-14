using PEUtils;
using RC_IOCPNet;
using RCCommon;
using RCProtocol;
using System.Collections.Concurrent;

// 游戏内网络同步服务
namespace ARPGServer
{
    public class GameNet : ILogic
    {
        public IOCPNet<GameToken, NetMsg> gameNet;

        readonly ConcurrentQueue<GamePackage> serverPackages = new();
        readonly Dictionary<CMD, Action<GamePackage>> serverHandlers = new();

        public void Awake()
        {
            IOCPTool.LogFunc = PELog.Log;
            IOCPTool.WarnFunc = PELog.Warn;
            IOCPTool.ErrorFunc = PELog.Error;
            IOCPTool.ColorLogFunc = (color, msg) => { PELog.ColorLog((LogColor)color, msg); };

            AddServerHandler(CMD.OnClient2GameConnected, OnClient2GameConnected);
            AddServerHandler(CMD.OnClient2GameDisConnected, OnClient2GameDisConnected);

            gameNet = new();
            gameNet.StartAsServer("127.0.0.1", 19000, 5000);
        }

        public void Update()
        {
            while (!serverPackages.IsEmpty)
            {
                if (serverPackages.TryDequeue(out GamePackage package))
                {
                    if (serverHandlers.TryGetValue(package.message.cmd, out Action<GamePackage> handlerCb))
                    {
                        handlerCb.Invoke(package);
                    }
                }
            }
        }

        public void Destroy()
        {
            gameNet.CloseServer();
            gameNet = null;
            serverPackages.Clear();
            serverHandlers.Clear();
        }

        void OnClient2GameConnected(GamePackage pkg)
        {
            this.LogGreen($"{pkg.token.tokenID} new client Gameworld connected!");
        }
        void OnClient2GameDisConnected(GamePackage pkg)
        {
            this.LogRed($"{pkg.token.tokenID} new client Gameworld Disconnected!");
        }

        //-------------Tool Functions-------------//
        public void AddServerPackages(GamePackage package)
        {
            serverPackages.Enqueue(package);
        }
        public void AddServerHandler(CMD cmd, Action<GamePackage> handlerCb)
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
    }

    public class GamePackage
    {
        public GameToken token;
        public NetMsg message;
        public GamePackage(GameToken token, NetMsg message)
        {
            this.token = token;
            this.message = message;
        }
    }
}
