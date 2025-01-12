using PEUtils;

// Process Launcher 进程启动器
namespace RCCommon
{
    public class ProcessLauncher<T> where T : ServerProcess<T>, new()
    {
        // e.g: break_12m 3001 ==> cmdEnum = break_12m, cmdArgs = 3001
        ConsoleCommand cmdEnum;
        string cmdArgs = "";

        public ProcessLauncher(T server, int defaultID, bool ignoreInput, string[] args)
        {
            string argsContent = "Args:";
            if (args.Length == 0)
            {
                while (true)
                {
                    Console.WriteLine($"Input ProcessID:\n(---Press「Enter」use default ID:{defaultID}---)");
                    string? input = Console.ReadLine();
                    if (input != "")
                    {
                        // Parse ID 解析ID
                        if (int.TryParse(input, out defaultID))
                        {
                            break;
                        }
                        else
                        {
                            PELog.Warn($"Input Illegal:{input}");
                        }
                    }
                    else
                    {
                        break; // use default ID
                    }
                }
            }
            else
            {
                for (int i = 0; i < args.Length; i++)
                {
                    argsContent += args[i];
                }
                if (int.TryParse(args[0], out int argsID))
                {
                    defaultID = argsID;
                }
            }

            LogConfig cfg = new()
            {
                loggerEnum = LoggerType.Console,
                saveName = $"{typeof(T).Name}_{defaultID}.txt"
            };

            PELog.InitSettings(cfg);

            this.LogMagenta(argsContent);
            server.processID = defaultID;
            PELog.LogGreen($"Launch {typeof(T).Name} ==> ID: {server.processID}");

            // Run Console Command in another Thread 在另一个线程中运行控制台命令
            Task.Run(HandleConsoleCmd);

            // main logics
            server.Awake();
            while (true)
            {
                server.Update();
                Thread.Sleep(30);

                if (cmdEnum != ConsoleCommand.none)
                {
                    if (cmdEnum == ConsoleCommand.quit)
                    {
                        server.Destroy();
                        break;
                    }
                    else
                    {
                        server.HandleCmd(cmdEnum, cmdArgs);
                        cmdEnum = ConsoleCommand.none;
                        cmdArgs = "";
                    }
                }
            }
        }

        // Handle Console Command 处理控制台输入的命令
        void HandleConsoleCmd()
        {
            while (true)
            {
                if (cmdEnum == ConsoleCommand.quit)
                {
                    // wait for main thread to break
                    Thread.Sleep(1000);
                    continue;
                }
                else
                {
                    string? input = Console.ReadLine();
                    if (!string.IsNullOrEmpty(input))
                    {
                        ParseConsoleCmd(input);
                    }
                }
            }
        }

        void ParseConsoleCmd(string input)
        {
            string[] cmdArr = input.Split(' ');
            try
            {
                // cmdArr[0] ==> ConsoleCommand
                ConsoleCommand cmd = (ConsoleCommand)Enum.Parse(typeof(ConsoleCommand), cmdArr[0]);
                if (cmd < ConsoleCommand.cmd_max)
                {
                    PELog.LogCyan($"input command: [{cmd}]");
                    cmdEnum = cmd;
                    if (cmdArr.Length > 1)
                    {
                        cmdArgs = cmdArr[1];
                    }
                }
            }
            catch (Exception)
            {
                PELog.Warn($"command: [{input}] does not exist.");
                cmdEnum = ConsoleCommand.none;
                cmdArgs = "";
            }
        }
    }
}
