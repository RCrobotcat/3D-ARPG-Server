using RCProtocol;
using System.Numerics;

// 游戏中实体系统
namespace ARPGServer
{
    public class GameEntity : ServerLogicBase
    {
        public PlayerStateEnum playerState = PlayerStateEnum.None; // 玩家状态
        public EntityDriverEnum driverEnum = EntityDriverEnum.None; // 实体驱动类型

        public GameToken gameToken; // 游戏网络会话

        public Vector3 entityPos; // 实体位置
        public Vector3 entityTargetPos; // 实体目标位置

        public int roleID; // 玩家ID
        public string account; // 玩家账号
        public string stageName; // 场景名称

        public GameEntity() { }

        public GameEntity(int id, string account, string stageName)
        {
            this.roleID = id;
            this.account = account;
            this.stageName = stageName;
        }

        public override void Update()
        {
            base.Update();
        }

        public override void Destroy()
        {
            base.Destroy();

            gameToken = null;
            playerState = PlayerStateEnum.None;
            driverEnum = EntityDriverEnum.None;
            ARPGProcess.Instance.entitySystem.RemoveEntity(this);
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        public void SenMsg(NetMsg msg)
        {
            gameToken?.SendMsg(msg);
        }
    }
}
