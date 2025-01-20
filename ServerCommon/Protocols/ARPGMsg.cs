// 战斗进程网络协议
namespace RCProtocol
{
    public class ReqTokenAccess
    {
        public int uid { get; set; }
        public string token { get; set; }
    }

    public class SyncMovePos
    {
        public int roleID { get; set; }
        public string account { get; set; }
        public float PosX { get; set; }
        public float PosZ { get; set; }

        public float dirX { get; set; }
        public float dirY { get; set; }
        public float dirZ { get; set; }

        public long timestamp { get; set; }
    }

    public class SyncAnimationState
    {
        public int roleID { get; set; }
        public string account { get; set; }

        public AnimationStateEnum animationStateEnum { get; set; }
    }

    public class SwitchWeapon
    {
        public int roleID { get; set; }
        public string account { get; set; }
        public string weaponName { get; set; }
    }
    public class UnEquipWeapon
    {
        public int roleID { get; set; }
        public string account { get; set; }
    }

    public class RemoveEntity
    {
        public int roleID { get; set; }
    }

    public partial class NetMsg
    {
        public ReqTokenAccess reqTokenAccess { get; set; }
        public SyncMovePos syncMovePos { get; set; }
        public SyncAnimationState syncAnimationState { get; set; }
        public SwitchWeapon switchWeapon { get; set; }
        public UnEquipWeapon unEquipWeapon { get; set; }
        public RemoveEntity removeEntity { get; set; }
    }

    public enum AnimationStateEnum
    {
        None,
        Sprint,
        Roll,

        // Combo Attack
        Combo_1,
        Combo_2,
        Combo_3,
        Combo_4,
        Combo_5,
    }
}
