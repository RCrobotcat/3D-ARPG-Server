using RCCommon;
using RCProtocol;
using System.Numerics;

// 实体移动同步组件
namespace ARPGServer
{
    public class MoveComp : EntityComp, IAwake, IUpdate
    {
        public Vector3 entityTargetPos; // 实体目标位置

        public void Awake()
        {
            SendSyncMovePos();
        }

        public void Update()
        {
            if (parent.entityPos == entityTargetPos)
                return;

            parent.entityPos = entityTargetPos;
            SendSyncMovePos();
        }

        /// <summary>
        /// 发送同步移动位置
        /// </summary>
        void SendSyncMovePos()
        {
            NetMsg netMsg = new NetMsg
            {
                cmd = CMD.SyncMovePos,
                syncMovePos = new SyncMovePos
                {
                    roleID = parent.roleID,
                    account = parent.account,
                    PosX = parent.entityPos.X,
                    PosZ = parent.entityPos.Z
                }
            };
            ARPGProcess.Instance.entitySystem.SendToAll(netMsg, parent.gameToken);
        }
    }
}
