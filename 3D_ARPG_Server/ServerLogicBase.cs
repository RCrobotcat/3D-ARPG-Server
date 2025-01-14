using RCCommon;

// 基础组件组成的逻辑单位
namespace ARPGServer
{
    public abstract class ServerLogicBase : ILogic
    {
        readonly protected List<IAwake> awakeList = new();
        readonly protected List<IUpdate> updateList = new();
        readonly protected List<IDestroy> destroyList = new();

        public virtual void Awake()
        {
            for (int i = 0; i < awakeList.Count; i++)
            {
                awakeList[i].Awake();
            }
        }

        public virtual void Update()
        {
            for (int i = 0; i < updateList.Count; i++)
            {
                updateList[i].Update();
            }
        }

        public virtual void Destroy()
        {
            for (int i = 0; i < destroyList.Count; i++)
            {
                destroyList[i].Destroy();
            }
            awakeList.Clear();
            updateList.Clear();
            destroyList.Clear();
        }
    }

    /// <summary>
    /// 操控状态
    /// </summary>
    public enum CtrlState
    {
        /// <summary>
        /// 数据未初始化
        /// </summary>
        None = 0,
        /// <summary>
        /// 客户端玩家操控状态
        /// </summary>
        ClientOperate = 1,
        /// <summary>
        /// 服务器AI托管状态
        /// </summary>
        ServerMandate = 2
    }
    /// <summary>
    /// 逻辑驱动状态
    /// </summary>
    public enum TickState
    {
        None,
        /// <summary>
        /// 待机状态（初始化阶段,未进入游戏世界）
        /// </summary>
        Idle,
        /// <summary>
        /// 地图传送状态
        /// </summary>
        Teleport,
        /// <summary>
        /// GameWorld驱动逻辑。
        /// </summary>
        GameWorld
    }
}