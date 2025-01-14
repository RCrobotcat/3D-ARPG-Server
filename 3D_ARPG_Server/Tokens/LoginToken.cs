using RC_IOCPNet;
using RCProtocol;

// 登录网络会话
namespace ARPGServer
{
    public class LoginToken : IOCPToken<NetMsg>
    {
        protected override void OnConnected(bool result)
        {
            if (result)
            {
                this.LogGreen($"Token: {tokenID} => UnityClient Login Connection: {result}");
                ARPGProcess.Instance.loginNet.AddServerPackages(new LoginPackage(this, new NetMsg(CMD.OnClient2LoginConnected)));
            }
            else
            {
                this.LogYellow($"Token: {tokenID} => UnityClient Login Connection: {result}");
            }
        }

        protected override void OnDisConnected()
        {
            this.LogYellow($"Token: {tokenID} => UnityClient Login Disconnected.");
            ARPGProcess.Instance.loginNet.AddServerPackages(new LoginPackage(this, new NetMsg(CMD.OnClient2LoginDisConnected)));
        }

        protected override void OnReceiveMsg(NetMsg msg)
        {
            this.Log($"Token: {tokenID} => Receive UnityClient Login Command:{msg.cmd}");
            ARPGProcess.Instance.loginNet.AddServerPackages(new LoginPackage(this, msg));
        }
    }
}
