using RCCommon;
using RCProtocol;
using System.Collections.Concurrent;

namespace ARPGServer
{
    public class EntitySystem : ILogic
    {
        readonly List<GameEntity> currentEntities = new(); // 当前实体列表
        public List<GameEntity> CurrentEntities { get => currentEntities; }
        public int CurrentEntitiesCount { get => currentEntities.Count; }

        private ConcurrentQueue<MonsterEntity> currentMonsters = new(); // 怪物队列
        List<int> removedMonsterIDs = new List<int>(); // 已经被移除的怪物ID列表

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

            // 移除已经被移除的怪物ID
            for (int i = 0; i < removedMonsterIDs.Count; i++)
            {
                SendRemoveMonster(removedMonsterIDs[i]);
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
                // 如果所有实体被移除，清空怪物列表
                foreach (var monster in currentMonsters)
                {
                    SendRemoveMonster(monster.monsterID);
                    removedMonsterIDs.Add(monster.monsterID);
                    monster.Destroy();
                }

                currentMonsters = new ConcurrentQueue<MonsterEntity>(); // 重建一个新的队列
                ARPGProcess.Instance.gameNet.IsMonstersCreated = false;
                this.LogCyan("All Entities in the GameWorld have been removed!");
            }
        }

        /// <summary>
        /// 同步所有实体
        /// </summary>
        public void SendToAll(NetMsg msg, GameToken selfToken, Action onSendComplete = null)
        {
            if (currentEntities.Count == 0) return;

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
            if (entity != null && entity.isFirstPlayer)
            {
                foreach (var monster in currentMonsters)
                {
                    SendRemoveMonster(monster.monsterID);
                    removedMonsterIDs.Add(monster.monsterID);
                    monster.Destroy();
                }

                currentMonsters = new ConcurrentQueue<MonsterEntity>(); // 清空怪物队列
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
            currentMonsters.Enqueue(entity);
            this.LogYellow($"Number of Current Monsters in the GameWorld: {currentMonsters.Count}");
        }

        /// <summary>
        /// 移除怪物实体
        /// </summary>
        public void RemoveMonsterEntity(MonsterEntity entity)
        {
            var tempQueue = new ConcurrentQueue<MonsterEntity>();
            bool found = false;

            // 通过遍历队列寻找并排除指定的怪物
            while (currentMonsters.TryDequeue(out var monster))
            {
                if (!found && monster.Equals(entity))
                {
                    found = true;  // 找到该怪物并跳过
                    continue;
                }
                tempQueue.Enqueue(monster);  // 重新添加其它怪物
            }

            currentMonsters = tempQueue;  // 替换队列

            if (found)
            {
                this.LogYellow($"Number of Current Monsters in the GameWorld: {currentMonsters.Count}");
            }
            else
            {
                this.LogYellow("Monster not found in the queue.");
            }
        }

        /// <summary>
        /// 通过ID获取怪物
        /// </summary>
        public MonsterEntity GetMonsterByID(int id)
        {
            foreach (var monster in currentMonsters)
            {
                if (monster.monsterID == id)
                {
                    return monster;
                }
            }
            return null;
        }

        /// <summary>
        /// 移除怪物消息
        /// </summary>
        public void SendRemoveMonster(int id)
        {
            SendToAll(new NetMsg
            {
                cmd = CMD.RemoveMonster,
                removeMonster = new RemoveMonster
                {
                    monsterID = id
                }
            });
        }
    }
}
