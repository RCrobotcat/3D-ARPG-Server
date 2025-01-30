namespace RCProtocol
{
    public enum MonstersEnum
    {
        Golem,
        Skeleton,
        //...
    }

    public enum MonsterAnimationStateEnum
    {
        None,
        Run,
        Attack,
    }

    /// <summary>
    /// 同步怪物动画状态
    /// </summary>
    public class SyncMonsterAnimationState
    {
        public int monsterID { get; set; }
        public MonsterAnimationStateEnum monsterAnimationStateEnum { get; set; }
    }

    /// <summary>
    /// 创建怪物
    /// </summary>
    public class CreateMonsters
    {
        public int monsterID { get; set; }
        public MonstersEnum monsterType { get; set; }
        public float PosX { get; set; }
        public float PosZ { get; set; }
    }

    /// <summary>
    /// 同步怪物移动位置
    /// </summary>
    public class SyncMonsterMovePos
    {
        public int monsterID { get; set; }
        public MonstersEnum monsterType { get; set; }

        public float PosX { get; set; }
        public float PosZ { get; set; }

        public float dirX { get; set; }
        public float dirY { get; set; }
        public float dirZ { get; set; }

        public long timestamp { get; set; }
    }

    public partial class NetMsg
    {
        public CreateMonsters createMonsters { get; set; }
        public SyncMonsterMovePos syncMonsterMovePos { get; set; }
        public SyncMonsterAnimationState syncMonsterAnimationState { get; set; }

        public RemoveMonster removeMonster { get; set; }
    }

    public class RemoveMonster
    {
        public int monsterID { get; set; }
    }
}
