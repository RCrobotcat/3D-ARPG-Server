using RCCommon;
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
        public Vector3 entityDir; // 实体方向

        public int roleID; // 玩家ID
        public string account; // 玩家账号
        public string stageName; // 场景名称

        readonly protected Dictionary<int, EntityComp> compDic = new();

        MoveComp moveComp;
        public MoveComp MoveComp { get => moveComp; }

        AnimationComp animationComp;
        public AnimationComp AnimationComp { get => animationComp; }

        public GameEntity(int id, string account, string stageName)
        {
            this.roleID = id;
            this.account = account;
            this.stageName = stageName;

            playerState = PlayerStateEnum.Online;

            moveComp = AddComp<MoveComp>();
            moveComp.InitTimer();

            animationComp = AddComp<AnimationComp>();
        }

        public override void Destroy()
        {
            base.Destroy();

            gameToken = null;
            playerState = PlayerStateEnum.Offline;
            driverEnum = EntityDriverEnum.None;
            ARPGProcess.Instance.entitySystem.RemoveEntity(this);
        }

        /// <summary>
        /// 添加组件
        /// </summary>
        public T AddComp<T>() where T : EntityComp, new()
        {
            T comp = new() { parent = this };
            if (comp is IAwake awakeComp) { awakeList.Add(awakeComp); }
            if (comp is IUpdate updateComp) { updateList.Add(updateComp); }
            if (comp is IDestroy destroyComp) { destroyList.Add(destroyComp); }

            compDic.Add(typeof(T).GetHashCode(), comp);
            return comp;
        }
        /// <summary>
        /// 获取组件
        /// </summary>
        public T GetComp<T>() where T : EntityComp, new()
        {
            int hashCode = typeof(T).GetHashCode();
            if (compDic.TryGetValue(hashCode, out EntityComp? comp))
            {
                if (comp != null && comp is T)
                {
                    return comp as T;
                }
            }
            return null;
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        public void SenMsg(NetMsg msg)
        {
            gameToken?.SendMsg(msg);
        }

        /// <summary>
        /// 是否是客户端玩家
        /// </summary>
        public bool IsClientPlayer()
        {
            return driverEnum == EntityDriverEnum.Client;
        }
    }

    public class EntityComp
    {
        public int RoleID { get => parent.roleID; }
        public GameEntity parent;

        public T GetComp<T>() where T : EntityComp, new()
        {
            return parent.GetComp<T>();
        }

        public bool IsClientPlayer() { return parent.IsClientPlayer(); }
    }
}
