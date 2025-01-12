// 通用计算工具
// Common Calculation Tool

using System;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace RCCommon
{
    public class PECalculateTool
    {
        /// <summary>
        /// 基于分线ID + stageID 生成 WorldID
        /// </summary>
        public static ulong GetWorldID(int prefix, int stageID)
        {
            // ulong为64位无符号整数, prefix的前32位作为前32位 + 32位stageID(int)作为后32位
            return ((ulong)prefix << 32) + (ulong)stageID;
        }
        public static int GetPrefixID(ulong worldID)
        {
            return (int)(worldID >> 32);
        }
        public static int GetStageID(ulong worldID)
        {
            return (int)(worldID & 0X0000FFFF);
        }


        /// <summary>
        /// 模拟MD5加密
        /// </summary>
        /// <param name="input">需要加密的字符串</param>
        public static string GetMD5Hash(string input)
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = MD5.HashData(inputBytes);
            StringBuilder stringBuilder = new();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                stringBuilder.Append(hashBytes[i].ToString("x2"));
            }
            return stringBuilder.ToString();
        }

        static readonly Random rd = new();
        public static Vector3 GetRandomPositionXZ(Vector3 startPos, float radius)
        {
            double angle = rd.NextDouble() * 2 * Math.PI;
            float x = (float)Math.Cos(angle);
            float z = (float)Math.Sin(angle);
            float distance = (float)Math.Sqrt(rd.NextDouble()) * radius;
            return new Vector3(startPos.X + x * distance, startPos.Y, startPos.Z + z * distance);
        }
        public static Vector3 GetRandomRotationXZ()
        {
            double angle = rd.NextDouble() * 2 * Math.PI;
            float x = (float)Math.Cos(angle);
            float z = (float)Math.Sin(angle);
            return Vector3.Normalize(new Vector3(x, 0, z));
        }
    }
}
