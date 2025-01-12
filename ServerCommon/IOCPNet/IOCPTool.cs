// IOCP工具类 IOCP Tool

using System;
using System.Collections.Generic;
using System.Text.Json;

namespace RC_IOCPNet
{
    public static class IOCPTool
    {
        public static byte[] SplitLogicBytes(ref List<byte> bytesLst)
        {
            byte[] buff = null;
            if (bytesLst.Count > 4)
            {
                byte[] data = bytesLst.ToArray();
                int len = BitConverter.ToInt32(data, 0);
                if (bytesLst.Count >= len + 4)
                {
                    buff = new byte[len];
                    Buffer.BlockCopy(data, 4, buff, 0, len);
                    bytesLst.RemoveRange(0, len + 4);
                }
            }
            return buff;
        }
        public static byte[] PackLenInfo(byte[] body)
        {
            int len = body.Length;
            byte[] pkg = new byte[len + 4];
            byte[] head = BitConverter.GetBytes(len);
            head.CopyTo(pkg, 0);
            body.CopyTo(pkg, 4);
            return pkg;
        }
        public static byte[] Serialize<T>(T msg) where T : new()
        {
            return JsonSerializer.SerializeToUtf8Bytes(msg);
        }
        public static T DeSerialize<T>(byte[] bytes) where T : new()
        {
            return JsonSerializer.Deserialize<T>(bytes);
        }

        #region LOG
        private static Action<string> logFunc;
        public static Action<string> LogFunc { get => logFunc; set => logFunc = value; }
        private static Action<IOCPLogColor, string> colorLogFunc;
        public static Action<IOCPLogColor, string> ColorLogFunc { get => colorLogFunc; set => colorLogFunc = value; }
        private static Action<string> warnFunc;
        public static Action<string> WarnFunc { get => warnFunc; set => warnFunc = value; }
        private static Action<string> errorFunc;
        public static Action<string> ErrorFunc { get => errorFunc; set => errorFunc = value; }

        public static void Log(string msg, params object[] args)
        {
            msg = string.Format(msg, args);
            if (LogFunc != null)
            {
                LogFunc(msg);
            }
            else
            {
                ConsoleLog(msg, IOCPLogColor.None);
            }
        }
        public static void ColorLog(IOCPLogColor color, string msg, params object[] args)
        {
            msg = string.Format(msg, args);
            if (ColorLogFunc != null)
            {
                ColorLogFunc(color, msg);
            }
            else
            {
                ConsoleLog(msg, color);
            }
        }
        public static void Warn(string msg, params object[] args)
        {
            msg = string.Format(msg, args);
            if (WarnFunc != null)
            {
                WarnFunc(msg);
            }
            else
            {
                ConsoleLog(msg, IOCPLogColor.Yellow);
            }
        }
        public static void Error(string msg, params object[] args)
        {
            msg = string.Format(msg, args);
            if (ErrorFunc != null)
            {
                ErrorFunc(msg);
            }
            else
            {
                ConsoleLog(msg, IOCPLogColor.Red);
            }
        }
        private static void ConsoleLog(string msg, IOCPLogColor color)
        {
            int threadID = Environment.CurrentManagedThreadId;
            msg = string.Format("Tid:{0} {1}", threadID, msg);
            switch (color)
            {
                case IOCPLogColor.Red:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine(msg);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case IOCPLogColor.Green:
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(msg);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case IOCPLogColor.Blue:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine(msg);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case IOCPLogColor.Cyan:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine(msg);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case IOCPLogColor.Magenta:
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine(msg);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case IOCPLogColor.Yellow:
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine(msg);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case IOCPLogColor.None:
                default:
                    Console.WriteLine(msg);
                    break;
            }
        }
        #endregion
    }

    public enum IOCPLogColor
    {
        None,
        Red,
        Green,
        Blue,
        Cyan,
        Magenta,
        Yellow
    }
}