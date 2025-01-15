using RCCommon;
using RCProtocol;
using System.Numerics;

// 实体移动同步组件
namespace ARPGServer
{
    public class MoveComp : EntityComp, IAwake, IUpdate
    {
        public Vector3 entityTargetPos; // 实体目标位置
        public Vector3 entityTargetDir; // 实体目标方向

        public void Awake()
        {
            SendSyncMovePos();
        }

        public void Update()
        {
            if (Vector3.Distance(parent.entityPos, entityTargetPos) <= 0.01f)
                return;

            parent.entityPos = Vector3.Lerp(parent.entityPos, entityTargetPos, 0.1f);
            parent.entityDir = Vector3.Lerp(parent.entityDir, entityTargetDir, 0.05f);
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
                    PosZ = parent.entityPos.Z,
                    dirX = parent.entityDir.X,
                    dirY = parent.entityDir.Y,
                    dirZ = parent.entityDir.Z
                }
            };
            ARPGProcess.Instance.entitySystem.SendToAll(netMsg, parent.gameToken);
        }
    }
}
