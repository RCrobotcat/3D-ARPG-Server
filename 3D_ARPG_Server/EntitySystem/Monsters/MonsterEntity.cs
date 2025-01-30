using RCCommon;
using RCProtocol;
using System.Numerics;

// 怪物实体类
namespace ARPGServer
{
    public class MonsterEntity : ServerLogicBase
    {
        public int monsterID; // 怪物ID
        public MonstersEnum monsterEnum; // 怪物类型
        public EntityDriverEnum driverEnum = EntityDriverEnum.None; // 实体驱动类型

        public GameToken createToken; // 创建怪物的玩家会话

        public Vector3 entityPos; // 实体位置
        public Vector3 entityDir; // 实体方向

        readonly protected Dictionary<int, MonsterEntityComp> compDic = new();

        MonsterMoveComp moveComp;
        MonsterAnimationComp animationComp;

        public MonsterEntity(int id, MonstersEnum monsterEnum)
        {
            this.monsterID = id;
            this.monsterEnum = monsterEnum;

            moveComp = AddComp<MonsterMoveComp>();
            GetComp<MonsterMoveComp>().InitTimer();

            animationComp = AddComp<MonsterAnimationComp>();
        }

        public override void Destroy()
        {
            base.Destroy();

            moveComp = null;
            animationComp = null;

            compDic.Clear();

            createToken = null;
        }

        /// <summary>
        /// 添加组件
        /// </summary>
        public T AddComp<T>() where T : MonsterEntityComp, new()
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
        public T GetComp<T>() where T : MonsterEntityComp, new()
        {
            int hashCode = typeof(T).GetHashCode();
            if (compDic.TryGetValue(hashCode, out MonsterEntityComp? comp))
            {
                if (comp != null && comp is T)
                {
                    return comp as T;
                }
            }
            return null;
        }
    }

    public class MonsterEntityComp
    {
        public int MonsterID { get => parent.monsterID; }
        public MonsterEntity parent;

        public T GetComp<T>() where T : MonsterEntityComp, new()
        {
            return parent.GetComp<T>();
        }
    }
}
