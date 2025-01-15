using RCCommon;
using RCProtocol;
using System.Numerics;

// 实体移动同步组件
namespace ARPGServer
{
    public class MoveComp : EntityComp, IUpdate
    {
        public Vector3 entityTargetPos; // 实体目标位置
        public Vector3 entityTargetDir; // 实体目标方向

        private System.Timers.Timer syncTimer;
        /// <summary>
        /// 初始化同步定时器
        /// </summary>
        public void InitTimer()
        {
            // 初始化同步定时器，每隔53ms发送一次位置更新
            syncTimer = new System.Timers.Timer(53);
            syncTimer.Elapsed += (sender, e) => SendSyncMovePos();
            syncTimer.Start();
        }

        public void Update()
        {
            parent.entityPos = entityTargetPos;
            parent.entityDir = entityTargetDir;
        }

        /// <summary>
        /// 发送同步移动位置
        /// </summary>
        void SendSyncMovePos()
        {
            if (parent.playerState == PlayerStateEnum.Offline || parent.playerState == PlayerStateEnum.None)
                return;

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
                    dirZ = parent.entityDir.Z,
                    timestamp = DateTime.UtcNow.Ticks
                }
            };
            ARPGProcess.Instance.entitySystem.SendToAll(netMsg, parent.gameToken);
        }
    }
}
