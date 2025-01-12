using System.Collections.Generic;

// IOCP会话连接Token缓存池
// IOCP Token Pool

namespace RC_IOCPNet
{
    public class IOCPTokenPool<T, K>
        where T : IOCPToken<K>, new()
        where K : new()
    {
        readonly Stack<T> stk;
        public int Size => stk.Count;
        public IOCPTokenPool(int capacity)
        {
            stk = new Stack<T>(capacity);
        }

        public T Pop()
        {
            lock (stk)
            {
                return stk.Pop();
            }
        }
        public void Push(T token)
        {
            if (token == null)
            {
                IOCPTool.Error("Token to be pushed cannot be null.");
                return;
            }
            lock (stk)
            {
                stk.Push(token);
            }
        }
    }
}