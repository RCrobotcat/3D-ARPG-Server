﻿using PEUtils;
using RC_IOCPNet;
using RCCommon;
using RCProtocol;
using System.Collections.Concurrent;
using System.Numerics;

// 游戏内网络同步服务
namespace ARPGServer
{
    public class GameNet : ILogic
    {
        public IOCPNet<GameToken, NetMsg> gameNet;

        readonly ConcurrentQueue<GamePackage> serverPackages = new();
        readonly Dictionary<CMD, Action<GamePackage>> serverHandlers = new();

        public void Awake()
        {
            IOCPTool.LogFunc = PELog.Log;
            IOCPTool.WarnFunc = PELog.Warn;
            IOCPTool.ErrorFunc = PELog.Error;
            IOCPTool.ColorLogFunc = (color, msg) => { PELog.ColorLog((LogColor)color, msg); };

            AddServerHandler(CMD.OnClient2GameConnected, OnClient2GameConnected);
            AddServerHandler(CMD.OnClient2GameDisConnected, OnClient2GameDisConnected);

            AddServerHandler(CMD.AffirmEnterStage, AffirmEnterStage);
            AddServerHandler(CMD.ExitGame, ExitGame);

            AddServerHandler(CMD.SyncMovePos, SyncMovePos);

            gameNet = new();
            gameNet.StartAsServer("127.0.0.1", 19000, 5000);
        }

        public void Update()
        {
            while (!serverPackages.IsEmpty)
            {
                if (serverPackages.TryDequeue(out GamePackage package))
                {
                    if (serverHandlers.TryGetValue(package.message.cmd, out Action<GamePackage> handlerCb))
                    {
                        handlerCb.Invoke(package);
                    }
                }
            }
        }

        public void Destroy()
        {
            gameNet.CloseServer();
            gameNet = null;
            serverPackages.Clear();
            serverHandlers.Clear();
        }

        void OnClient2GameConnected(GamePackage pkg)
        {
            this.LogGreen($"{pkg.token.tokenID} new client Gameworld connected!");
        }
        void OnClient2GameDisConnected(GamePackage pkg)
        {
            this.LogRed($"{pkg.token.tokenID} new client Gameworld Disconnected!");
        }

        /// <summary>
        /// 确认进入场景
        /// </summary>
        void AffirmEnterStage(GamePackage pkg)
        {
            AffirmEnterStage affirmEnterStage = pkg.message.affirmEnterStage;
            GameEntity entity = new GameEntity(affirmEnterStage.roleID, affirmEnterStage.account, affirmEnterStage.stageName)
            {
                entityPos = new Vector3(affirmEnterStage.PosX, 0, affirmEnterStage.PosZ),
                gameToken = pkg.token
            };
            ARPGProcess.Instance.entitySystem.AddEntity(entity);
        }

        /// <summary>
        /// 同步移动位置
        /// </summary>
        void SyncMovePos(GamePackage pkg)
        {
            Vector3 targetPos = new Vector3(pkg.message.syncMovePos.PosX, 0, pkg.message.syncMovePos.PosZ);

            int roleID = pkg.message.syncMovePos.roleID;
            GameEntity entity = ARPGProcess.Instance.entitySystem.GetEntityByID(roleID);
            if (entity != null)
                entity.MoveComp.entityTargetPos = targetPos;
        }

        /// <summary>
        /// 退出游戏
        /// </summary>
        void ExitGame(GamePackage pkg)
        {
            int exitRoleID = pkg.message.exitGame.roleID;
            string exitAccount = pkg.message.exitGame.account;
            ARPGProcess.Instance.entitySystem.ExitEntityByID(exitRoleID);
            ARPGProcess.Instance.loginNet.RemoveAccountToken(exitAccount);
        }

        //-------------Tool Functions-------------//
        public void AddServerPackages(GamePackage package)
        {
            serverPackages.Enqueue(package);
        }
        public void AddServerHandler(CMD cmd, Action<GamePackage> handlerCb)
        {
            if (!serverHandlers.ContainsKey(cmd))
            {
                serverHandlers.Add(cmd, handlerCb);
            }
            else
            {
                this.Error($"Command: {cmd} already exists in server handler!");
            }
        }
    }

    public class GamePackage
    {
        public GameToken token;
        public NetMsg message;
        public GamePackage(GameToken token, NetMsg message)
        {
            this.token = token;
            this.message = message;
        }
    }
}