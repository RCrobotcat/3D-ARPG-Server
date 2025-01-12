// 服务进程抽象类 Server Process Abstract Class
using System.Collections.Generic;

namespace RCCommon
{
    public abstract class ServerProcess<T> : ILogic where T : new()
    {
        // Singleton
        private static T? instance;
        public static T Instance
        {
            get
            {
                instance ??= new();
                // same as:
                // if (instance == null)
                // {
                //     instance = new();
                // }

                return instance;
            }
        }

        public int processID;
        readonly List<ILogic> logics = new();

        public virtual void Awake()
        {
            for (int i = 0; i < logics.Count; i++)
            {
                logics[i].Awake();
            }
        }

        public virtual void Update()
        {
            for (int i = 0; i < logics.Count; i++)
            {
                logics[i].Update();
            }
        }

        public virtual void Destroy()
        {
            for (int i = 0; i < logics.Count; i++)
            {
                logics[i].Destroy();
            }
        }

        protected void AddServceOrSystem(ILogic logicModule)
        {
            logics.Add(logicModule);
        }

        public abstract void HandleCmd(ConsoleCommand cmd, string? args);
    }
}
