using RCCommon;
using RCProtocol;

namespace ARPGServer
{
    public class MonsterAnimationComp : MonsterEntityComp, IUpdate
    {
        private MonsterAnimationStateEnum lastSentAnimationState = MonsterAnimationStateEnum.None; // 上次发送的动画状态
        public MonsterAnimationStateEnum animationState = MonsterAnimationStateEnum.None;

        public void Update()
        {
            // 只有当动画状态发生变化时才发送同步消息
            if (animationState != lastSentAnimationState)
            {
                lastSentAnimationState = animationState;
                SendSyncAnimationState();
            }
        }

        void SendSyncAnimationState()
        {
            NetMsg netMsg = new NetMsg
            {
                cmd = CMD.SyncMonsterAnimationState,
                syncMonsterAnimationState = new SyncMonsterAnimationState
                {
                    monsterID = parent.monsterID,
                    monsterAnimationStateEnum = animationState
                }
            };
            ARPGProcess.Instance.entitySystem.SendToAll(netMsg, parent.createToken);
        }
    }
}
