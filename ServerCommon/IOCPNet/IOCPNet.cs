// 基于IOCP封装的异步套接字通信 - 客户端和服务端 
// IOCP Based Asynchronous Socket Communication - Client and Server

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace RC_IOCPNet
{
    public class IOCPNet<T, K>
        where T : IOCPToken<K>, new()
        where K : new()
    {
        Socket skt;
        readonly SocketAsyncEventArgs saea;

        public IOCPNet()
        {
            saea = new SocketAsyncEventArgs();
            saea.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
        }
        int pid;
        public IOCPNet(int pid)
        {
            this.pid = pid;
            saea = new SocketAsyncEventArgs();
            saea.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
        }

        #region Client
        public T token;
        public void StartAsClient(string ip, int port)
        {
            IPEndPoint pt = new(IPAddress.Parse(ip), port);
            skt = new Socket(pt.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            saea.RemoteEndPoint = pt;

            IOCPTool.ColorLog(IOCPLogColor.Green, "Client Start...");

            StartConnect();
        }
        void StartConnect()
        {
            bool suspend = skt.ConnectAsync(saea);
            if (suspend == false)
            {
                ProcessConnect();
            }
        }
        void ProcessConnect()
        {
            token = new T
            {
                tokenID = pid
            };
            token.InitToken(skt);
        }
        public void ClosetClient()
        {
            if (token != null)
            {
                token.CloseToken();
                token = null;
            }
            if (skt != null)
            {
                skt = null;
            }
        }
        #endregion

        #region Server
        int curConnCount = 0;
        Semaphore acceptSeamaphore;
        IOCPTokenPool<T, K> pool;
        List<T> tokenLst;
        protected int backlog = 100;
        public void StartAsServer(string ip, int port, int maxConnCount)
        {
            curConnCount = 0;
            /*信号量是限制同时可以处理多少连接
             *比如这里设置1，那么同时有两个客户端连过来，只会第一个客户端连接接收完成，并进行数据收发处理。
             *实际上在客户端侧这个连接是建立成功了的，只是收发数据没有回应而已。
             *连接能建立成功就是无法正常并发处理业务而已。
             *只有等前面一个连接断开后，才会把原来缓存的数据收发一并处理掉。
             *如果此时客户端已经断开了，那么会报相关的Socket错误。
             */
            acceptSeamaphore = new Semaphore(maxConnCount, maxConnCount);
            pool = new IOCPTokenPool<T, K>(maxConnCount);
            for (int i = 0; i < maxConnCount; i++)
            {
                T token = new()
                {
                    tokenID = i
                };
                pool.Push(token);
            }
            tokenLst = new List<T>();

            IPEndPoint pt = new(IPAddress.Parse(ip), port);
            skt = new Socket(pt.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            skt.Bind(pt);
            skt.Listen(backlog);
            IOCPTool.ColorLog(IOCPLogColor.Green, "Server Start...");

            StartAccept();
        }
        void StartAccept()
        {
            saea.AcceptSocket = null;
            acceptSeamaphore.WaitOne();
            bool suspend = skt.AcceptAsync(saea);
            if (suspend == false)
            {
                ProcessAccept();
            }
        }
        void ProcessAccept()
        {
            if (tokenLst != null)
            {
                Interlocked.Increment(ref curConnCount);
                T token = pool.Pop();
                lock (tokenLst)
                {
                    tokenLst.Add(token);
                }
                // 在InitToken()之前指定关闭回调
                token.onTokenClose = OnTokenClose;
                // IOCPTool.ColorLog(IOCPLogColor.Green, "Client Idle,Allocate TokenID:{0}", token.tokenID);
                token.InitToken(saea.AcceptSocket);
                // tokenLst为null时，服务器已经关闭了，没有必要再去调用StartAccept()了
                StartAccept();
            }
        }
        void OnTokenClose(int tokenID)
        {
            int index = -1;
            // 移除token时查找index的多线程数据竞争bug,
            /* BUG描述：
             * 比如tokenLst中有2个Token,当客户端1关闭后index找到的是第0号,但此时还没有从TokenList中移除前,这时客户端2也关闭查找index找到的是1号,
             * 之后，第0号从tokenLst中移除了，再去移除第1号时，数据就为null了。
             * 
             * 解决方案：
             * 将tokenLst的锁移到最外层，在某个token断开查找定位index时就锁起来，直到tokenLst中对应索引号的数据移除完成。
             */
            lock (tokenLst)
            {
                for (int i = 0; i < tokenLst.Count; i++)
                {
                    if (tokenLst[i].tokenID == tokenID)
                    {
                        index = i;
                        break;
                    }
                }
                if (index != -1)
                {
                    pool.Push(tokenLst[index]);
                    tokenLst.RemoveAt(index);
                    Interlocked.Decrement(ref curConnCount);
                    acceptSeamaphore.Release(); // 如果acceptSeamaphore为0, 则调用acceptSeamaphore.WaitOne();的线程会被阻塞；
                                                // acceptSeamaphore.Release()会使acceptSeamaphore加1, 此时会唤醒一个调用了调用acceptSeamaphore.WaitOne()的线程。
                }
                else
                {
                    IOCPTool.Error("Token:{0} cannot find in server token List.", tokenID);
                }
            }
        }
        public void CloseServer()
        {
            for (int i = 0; i < tokenLst.Count; i++)
            {
                tokenLst[i].CloseToken();
            }
            tokenLst = null;
            if (skt != null)
            {
                skt.Close();
                skt = null;
            }
        }
        public List<T> GetTokenLst()
        {
            return tokenLst;
        }
        #endregion

        void IO_Completed(object sender, SocketAsyncEventArgs saea)
        {
            switch (saea.LastOperation)
            {
                case SocketAsyncOperation.Accept:
                    ProcessAccept();
                    break;
                case SocketAsyncOperation.Connect:
                    ProcessConnect();
                    break;
                default:
                    IOCPTool.Warn("The last operation completed on the socket was not a accept or connect op.");
                    break;
            }
        }
    }
}