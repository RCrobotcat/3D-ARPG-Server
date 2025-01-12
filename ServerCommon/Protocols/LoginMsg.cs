using System.Collections.Generic;

// 登陆相关协议
namespace RCProtocol
{
    public class ReqAccountLogin
    {
        public string account { get; set; } // Current Account.
        public string password { get; set; } // Current Password.

        /// <summary>
        /// 当前区服ID所指向的数据进程ID
        /// </summary>
        public int dataID { get; set; }
    }

    public class NtfRoleData
    {
        public List<RoleData> roleData { get; set; }
    }

    public class RoleData
    {
        public int uid { get; set; }
        public string nickName { get; set; }

        // 记录玩家的位置信息和朝向信息
        public float posX { get; set; }
        public float posZ { get; set; }
        public float dirX { get; set; }
        public float dirZ { get; set; }

        /// <summary>
        /// Rid: 会一直常驻的地图ID, 比如主城等; => 玩家上一次离线时所在的地图ID.
        /// Tid: 临时副本的ID.
        /// </summary>
        public ulong lastRid { get; set; }
        public ulong lastTid { get; set; }

        #region 角色成长数值
        /// <summary>
        /// 当前角色基础单位配置ID号
        /// </summary>
        public int unitID { get; set; }
        /// <summary>
        /// 当前角色等级
        /// </summary>
        public int level { get; set; }
        /// <summary>
        /// 当前经验值
        /// </summary>
        public int exp { get; set; }
        #endregion

        // TODO: Add more role data.(业务数据)

        public override string ToString()
        {
            return $"  uid:{uid}\t NickName:{nickName}\t level:{level}\t exp:{exp}";
        }
    }

    public class ReqRoleToken
    {
        public string account { get; set; }
        public int selectedUid { get; set; }
    }

    public class RespRoleToken
    {
        public string battleIP { get; set; }
        public int battlePort { get; set; }
        public string token { get; set; }
    }

    public partial class NetMsg
    {
        public ReqRoleToken reqRoleToken { get; set; }
        public RespRoleToken respRoleToken { get; set; }
        public ReqAccountLogin reqAccountLogin { get; set; } // Common Response
        public NtfRoleData ntfRoleData { get; set; }
    }
}
