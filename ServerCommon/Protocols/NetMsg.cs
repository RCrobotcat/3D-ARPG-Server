// 网络通信协议数据类 Network Protocol Data Structure Class
namespace RCProtocol
{
    public partial class NetMsg
    {
        public CMD cmd { get; set; }
        public ErrorCode errorCode { get; set; }

        public NetMsg() { }
        public NetMsg(CMD cmd, ErrorCode errorCode = ErrorCode.None)
        {
            this.cmd = cmd;
            this.errorCode = errorCode;
        }

        public NtfEnterStage ntfEnterStage { get; set; }
        public AffirmEnterStage affirmEnterStage { get; set; }
    }

    public enum EnterStageMode
    {
        Login,
        Teleport,
        Reconnect,
    }

    public class NtfEnterStage
    {
        public EnterStageMode mode { get; set; }
        public int prefixID { get; set; }
        public int stageID { get; set; }

        public string stageName { get; set; }
    }

    public class AffirmEnterStage
    {
        public EnterStageMode mode { get; set; }
        public int prefixID { get; set; }
        public int stageID { get; set; }
    }
}
