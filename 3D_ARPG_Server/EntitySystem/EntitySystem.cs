using RCCommon;
using RCProtocol;

namespace ARPGServer
{
    public class EntitySystem : ILogic
    {
        readonly List<GameEntity> currentEntities = new(); // 当前实体列表

        public void Awake() { }

        public void Update()
        {
            foreach (var entity in currentEntities)
            {
                entity.Update();
            }
        }

        public void Destroy() { }

        /// <summary>
        /// 添加实体
        /// </summary>
        public void AddEntity(GameEntity entity)
        {
            if (!currentEntities.Contains(entity))
            {
                currentEntities.Add(entity);
                this.LogCyan($"Number of Current Entities in the GameWorld: {currentEntities.Count}");
            }
        }
        /// <summary>
        /// 通过ID获取实体
        /// </summary>
        public GameEntity GetEntityByID(int id)
        {
            return currentEntities.Find(e => e.roleID == id);
        }
        /// <summary>
        /// 移除实体
        /// </summary>
        public void RemoveEntity(GameEntity entity)
        {
            if (currentEntities.Contains(entity))
            {
                currentEntities.Remove(entity);
                SendToAll(new NetMsg // 通知所有客户端移除实体
                {
                    cmd = CMD.RemoveEntity,
                    removeEntity = new RemoveEntity
                    {
                        roleID = entity.roleID
                    }
                });
                this.LogCyan($"Number of Current Entities in the GameWorld: {currentEntities.Count}");
            }
        }

        /// <summary>
        /// 同步所有实体
        /// </summary>
        public void SendToAll(NetMsg msg, GameToken selfToken)
        {
            foreach (var entity in currentEntities)
            {
                if (entity.gameToken != null && entity.gameToken != selfToken)
                    entity.gameToken.SendMsg(msg);
            }
        }
        public void SendToAll(NetMsg msg)
        {
            foreach (var entity in currentEntities)
            {
                if (entity.gameToken != null)
                    entity.gameToken.SendMsg(msg);
            }
        }

        /// <summary>
        /// 实体退出游戏
        /// </summary>
        /// <param name="id">要退出的实体id</param>
        public void ExitEntityByID(int id)
        {
            var entity = currentEntities.Find(e => e.roleID == id);
            if (entity != null)
            {
                entity.Destroy();
            }
        }
    }
}
