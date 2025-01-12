// 战斗进程网络协议
namespace RCProtocol
{
    public class ReqTokenAccess
    {
        public int uid { get; set; }
        public string token { get; set; }
    }

    public partial class NetMsg
    {
        public ReqTokenAccess reqTokenAccess { get; set; }
    }
}
