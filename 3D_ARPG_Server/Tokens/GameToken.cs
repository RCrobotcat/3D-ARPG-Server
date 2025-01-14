using RC_IOCPNet;
using RCProtocol;

// 游戏内同步网络会话
namespace ARPGServer
{
    public class GameToken : IOCPToken<NetMsg>
    {
        protected override void OnConnected(bool result)
        {
            if (result)
            {
                this.LogGreen($"Token: {tokenID} => UnityClient Gameworld Connection: {result}");
                ARPGProcess.Instance.gameNet.AddServerPackages(new GamePackage(this, new NetMsg(CMD.OnClient2GameConnected)));
            }
            else
            {
                this.LogYellow($"Token: {tokenID} => UnityClient Gameworld Connection: {result}");
            }
        }

        protected override void OnDisConnected()
        {
            this.LogYellow($"Token: {tokenID} => UnityClient Gameworld Disconnected.");
            ARPGProcess.Instance.gameNet.AddServerPackages(new GamePackage(this, new NetMsg(CMD.OnClient2GameDisConnected)));
        }

        protected override void OnReceiveMsg(NetMsg msg)
        {
            // this.Log($"Token: {tokenID} => Receive UnityClient Gameworld Command:{msg.cmd}");
            ARPGProcess.Instance.gameNet.AddServerPackages(new GamePackage(this, msg));
        }
    }
}
