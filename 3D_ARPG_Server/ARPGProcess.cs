// 战斗进程 Battle Process
using RCCommon;
using ARPGServer;

// C# 顶级语句 Top-Level Statements
// 相当于Main函数
new ProcessLauncher<ARPGProcess>(ARPGProcess.Instance, DefaultConfig.battleID, DefaultConfig.JumpIDInput, args);

namespace ARPGServer
{
    public class ARPGProcess : ServerProcess<ARPGProcess>
    {
        public LoginNet loginNet;
        public GameNet gameNet;

        public override void Awake()
        {
            loginNet = new LoginNet();
            AddServceOrSystem(loginNet);

            gameNet = new GameNet();
            AddServceOrSystem(gameNet);

            base.Awake();
        }

        public override void HandleCmd(ConsoleCommand cmd, string? args)
        {
            this.LogGreen($"input: {cmd}");
        }
    }
}