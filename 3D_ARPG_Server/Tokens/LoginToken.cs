using RC_IOCPNet;
using RCProtocol;

// 登录网络会话Token
// Login Network Session Token

namespace ARPGServer
{
    public class LoginToken : IOCPToken<NetMsg>
    {
        protected override void OnConnected(bool result)
        {
            if (result)
            {
                this.LogGreen($"Token: {tokenID} => UnityClient Connection: {result}");
                ARPGProcess.Instance.loginNet.AddServerPackages(new LoginPackage(this, new NetMsg(CMD.OnClient2LoginConnected)));
            }
            else
            {
                this.LogYellow($"Token: {tokenID} => UnityClient Connection: {result}");
            }
        }

        protected override void OnDisConnected()
        {
            this.LogYellow($"Token: {tokenID} => UnityClient Disconnected.");
            ARPGProcess.Instance.loginNet.AddServerPackages(new LoginPackage(this, new NetMsg(CMD.OnClient2LoginDisConnected)));
        }

        protected override void OnReceiveMsg(NetMsg msg)
        {
            this.Log($"Token: {tokenID} => Receive UnityClient Command:{msg.cmd}");
            ARPGProcess.Instance.loginNet.AddServerPackages(new LoginPackage(this, msg));
        }
    }
}
