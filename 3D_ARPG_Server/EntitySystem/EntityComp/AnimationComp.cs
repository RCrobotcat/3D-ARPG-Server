using RCCommon;
using RCProtocol;

namespace ARPGServer
{
    public class AnimationComp : EntityComp, IUpdate
    {
        private AnimationStateEnum lastSentAnimationState = AnimationStateEnum.None; // 上次发送的动画状态
        public AnimationStateEnum animationState = AnimationStateEnum.None;

        public void Update()
        {
            // 只有当动画状态发生变化时才发送同步消息
            if (animationState != lastSentAnimationState)
            {
                lastSentAnimationState = animationState;
                SendSyncAnimationState();
            }
        }

        /// <summary>
        /// 发送同步动画状态
        /// </summary>
        void SendSyncAnimationState()
        {
            NetMsg netMsg = new NetMsg
            {
                cmd = CMD.SyncAnimationState,
                syncAnimationState = new SyncAnimationState
                {
                    roleID = parent.roleID,
                    account = parent.account,
                    animationStateEnum = animationState
                }
            };
            ARPGProcess.Instance.entitySystem.SendToAll(netMsg, parent.gameToken);
        }
    }
}
