// IOCP连接会话Token IOCP Network Session Token

using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace RC_IOCPNet
{
    public enum TokenState
    {
        None,
        Connected,
        DisConnected
    }

    public abstract class IOCPToken<T> where T : new()
    {
        public int tokenID;
        private readonly SocketAsyncEventArgs rcvSAEA;
        private readonly SocketAsyncEventArgs sndSAEA;
        private Socket skt;
        private List<byte> readLst = new();
        public Action<int> onTokenClose;
        public TokenState tokenState = TokenState.None;
        private bool isCaching = false;
        private readonly Queue<byte[]> cacheQue = new();
        private readonly object syncObj = new();
        public bool IsConnected
        {
            get
            {
                return tokenState == TokenState.Connected;
            }
        }
        public IOCPToken()
        {
            rcvSAEA = new SocketAsyncEventArgs();
            sndSAEA = new SocketAsyncEventArgs();
            rcvSAEA.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
            sndSAEA.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
            rcvSAEA.SetBuffer(new byte[2048], 0, 2048); // 缓存buffer，如果一次过来的数据很多就会分多次放入
        }

        public void InitToken(Socket skt)
        {
            this.skt = skt;
            // 如果未连接，则直接关闭Token，在OnConnected(bool result)中增加连接成功与否的传参。
            if (skt.Connected)
            {
                tokenState = TokenState.Connected;
                OnConnected(true);
                StartAsyncRcv();
            }
            else
            {
                // 通过传参让用户知道连接是否成功。
                OnConnected(false);
                // 连接不成功时会立即调用CloseToken();清理掉数据。
                CloseToken();
            }
        }
        void StartAsyncRcv()
        {
            bool suspend = skt.ReceiveAsync(rcvSAEA);
            if (suspend == false)
            {
                ProcessRcv();
            }
        }
        void ProcessRcv()
        {
            if (rcvSAEA.BytesTransferred > 0 && rcvSAEA.SocketError == SocketError.Success)
            {
                byte[] bytes = new byte[rcvSAEA.BytesTransferred];
                Buffer.BlockCopy(rcvSAEA.Buffer, 0, bytes, 0, rcvSAEA.BytesTransferred);
                readLst.AddRange(bytes);
                ProcessByteLst();
                StartAsyncRcv();
            }
            else
            {
                IOCPTool.Warn($"TokenID:{tokenID} Close:{1}", tokenID, rcvSAEA.SocketError.ToString());
                //避免主动调用ClosetClient()时重复触发关闭Token
                if (tokenState == TokenState.Connected)
                {
                    CloseToken();
                }
            }
        }
        void ProcessByteLst()
        {
            byte[] buff = IOCPTool.SplitLogicBytes(ref readLst);
            if (buff != null)
            {
                T msg = IOCPTool.DeSerialize<T>(buff);
                OnReceiveMsg(msg);
                ProcessByteLst();
            }
        }
        public bool SendMsg(T msg)
        {
            byte[] bytes = IOCPTool.PackLenInfo(IOCPTool.Serialize(msg));
            return SendMsg(bytes);
        }
        public bool SendMsg(byte[] bytes)
        {
            if (tokenState != TokenState.Connected)
            {
                IOCPTool.Warn($"TokenID:{tokenID} Connection is Disconnected,cannot send netService message.");
                return false;
            }

            lock (syncObj)
            {
                if (isCaching)
                {
                    cacheQue.Enqueue(bytes);
                    return true;
                }
                isCaching = true;
            }

            sndSAEA.SetBuffer(bytes, 0, bytes.Length);
            if (!skt.SendAsync(sndSAEA))
            {
                ProcessSend();
            }
            return true;
        }
        void ProcessSend()
        {
            if (sndSAEA.SocketError == SocketError.Success)
            {
                byte[] bytes = null;
                lock (syncObj)
                {
                    if (cacheQue.Count > 0)
                    {
                        bytes = cacheQue.Dequeue();
                    }
                    else
                    {
                        isCaching = false;
                    }
                }

                if (bytes != null)
                {
                    sndSAEA.SetBuffer(bytes, 0, bytes.Length);
                    if (!skt.SendAsync(sndSAEA))
                    {
                        ProcessSend();
                    }
                }
            }
            else
            {
                IOCPTool.Error($"TokenID:{tokenID} Process Send Error:{0}", sndSAEA.SocketError.ToString());
                CloseToken();
            }
        }
        void IO_Completed(object sender, SocketAsyncEventArgs saea)
        {
            switch (saea.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    ProcessRcv();
                    break;
                case SocketAsyncOperation.Send:
                    ProcessSend();
                    break;
                default:
                    IOCPTool.Warn($"TokenID: {tokenID} The last operation completed on the socket was not a receive or send operation.");
                    break;
            }
        }
        public void CloseToken()
        {
            OnDisConnected();

            readLst.Clear();
            cacheQue.Clear();
            isCaching = false;
            tokenState = TokenState.DisConnected;
            // skt相关的才需要判定是否为null，上面的这些无论是否为null都做数据清理。
            if (skt != null)
            {
                try
                {
                    skt.Shutdown(SocketShutdown.Send);
                }
                catch (Exception)
                {
                    IOCPTool.ColorLog(IOCPLogColor.Yellow, $"TokenID:{tokenID} Shutdown Socket Error.");
                }
                finally
                {
                    skt.Close();
                    skt = null;
                }
            }

            // 完成清理后才放回缓存池中
            onTokenClose?.Invoke(tokenID);
            onTokenClose = null;
        }

        protected abstract void OnConnected(bool result);
        protected abstract void OnReceiveMsg(T msg);
        protected abstract void OnDisConnected();
    }
}