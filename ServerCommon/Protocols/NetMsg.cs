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
        public InstantiateRole instantiateRole { get; set; }
        public AffirmEnterStage affirmEnterStage { get; set; }
        public ExitGame exitGame { get; set; }
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
        /*public int prefixID { get; set; }
        public int stageID { get; set; }*/

        public string stageName { get; set; }
    }

    public class InstantiateRole
    {
        public int roleID { get; set; }
        public string roleName { get; set; }

        public string account { get; set; }
        public PlayerStateEnum playerState = PlayerStateEnum.None; // 玩家状态
        public EntityDriverEnum driverEnum = EntityDriverEnum.None; // 实体驱动类型

        public float PosX { get; set; }
        public float PosZ { get; set; }
    }
    public enum PlayerStateEnum
    {
        None = 0,
        Online = 1, // 玩家上线
        Offline = 2, // 玩家离线
        Mandate = 3 // 服务器托管状态
    }
    public enum EntityDriverEnum
    {
        None,
        Client, // 客户端驱动的实体
        Server // 服务器驱动的实体
    }

    public class AffirmEnterStage
    {
        public EnterStageMode mode { get; set; }
        /*public int prefixID { get; set; }
        public int stageID { get; set; }*/
        public string stageName { get; set; }

        // 玩家信息
        public int roleID { get; set; }
        public string roleName { get; set; } // 玩家选中的角色名称
        public string account { get; set; }
        public PlayerStateEnum playerState = PlayerStateEnum.None; // 玩家状态
        public EntityDriverEnum driverEnum = EntityDriverEnum.None; // 实体驱动类型

        public float PosX { get; set; }
        public float PosZ { get; set; }
    }

    public class ExitGame
    {
        public int roleID { get; set; }
        public string account { get; set; }
    }
}