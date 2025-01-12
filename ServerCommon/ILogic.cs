namespace RCCommon
{
    public interface ILogic
    {
        public void Awake();
        public void Update();
        public void Destroy();
    }

    public interface IAwake
    {
        public void Awake();
    }

    public interface IUpdate
    {
        public void Update();
    }

    public interface IDestroy
    {
        public void Destroy();
    }
}
