using RCCommon;
using RCProtocol;

namespace ARPGServer
{
    public class EntitySystem : ILogic
    {
        readonly List<GameEntity> currentEntities = new(); // 当前实体列表
        public List<GameEntity> CurrentEntities { get => currentEntities; }
        public int CurrentEntitiesCount { get => currentEntities.Count; }

        readonly List<MonsterEntity> currentMonsters = new(); // 当前怪物列表

        public void Awake() { }

        public void Update()
        {
            foreach (var entity in currentEntities)
            {
                entity.Update();
            }

            foreach (var monster in currentMonsters)
            {
                monster.Update();
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

            if (currentEntities.Count == 0)
            {
                currentMonsters.Clear();
                ARPGProcess.Instance.gameNet.IsMonstersCreated = false;
                this.LogYellow("All Entities in the GameWorld have been removed!");
            }
        }

        /// <summary>
        /// 同步所有实体
        /// </summary>
        public void SendToAll(NetMsg msg, GameToken selfToken, Action onSendComplete = null)
        {
            if (currentEntities.Count == 0) return;

            /* if (msg.cmd == CMD.SyncAnimationState)
                 this.LogYellow($"SendToAll: {msg.syncAnimationState.animationStateEnum}");*/

            foreach (var entity in currentEntities)
            {
                if (entity != null && entity.gameToken != null && entity.gameToken != selfToken)
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
            GameEntity entity = currentEntities.Find(e => e.roleID == id);

            // 如果是第一个玩家退出游戏，则清空所有其创建的怪物实体
            if (entity.isFirstPlayer)
            {
                currentMonsters.Clear();
                ARPGProcess.Instance.gameNet.IsMonstersCreated = false;
                this.LogYellow("All Entities in the GameWorld have been removed!");
            }

            if (entity != null)
            {
                entity.Destroy();
            }
        }

        /// <summary>
        /// 所有实体切换武器
        /// </summary>
        public void AllEntitySwitchWeapon()
        {
            foreach (var entity in currentEntities)
            {
                entity.GetComp<WeaponAndArmorComp>().SendSwitchWeapon();
            }
        }

        /// <summary>
        /// 添加怪物实体
        /// </summary>
        public void AddMonsterEntity(MonsterEntity entity)
        {
            if (!currentMonsters.Contains(entity))
            {
                currentMonsters.Add(entity);
                this.LogYellow($"Number of Current Monsters in the GameWorld: {currentMonsters.Count}");
            }
        }
        /// <summary>
        /// 移除怪物实体
        /// </summary>
        public void RemoveMonsterEntity(MonsterEntity entity)
        {
            if (currentMonsters.Contains(entity))
            {
                currentMonsters.Remove(entity);
                this.LogYellow($"Number of Current Monsters in the GameWorld: {currentMonsters.Count}");
            }
        }
        public MonsterEntity GetMonsterByID(int id)
        {
            return currentMonsters.Find(e => e.monsterID == id);
        }
    }
}
