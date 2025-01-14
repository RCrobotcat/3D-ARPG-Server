using RCCommon;

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
        /// 移除实体
        /// </summary>
        public void RemoveEntity(GameEntity entity)
        {
            if (currentEntities.Contains(entity))
            {
                currentEntities.Remove(entity);
                this.LogCyan($"Number of Current Entities in the GameWorld: {currentEntities.Count}");
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
