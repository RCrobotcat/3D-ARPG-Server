namespace RCProtocol
{
    public enum CMD
    {
        None,

        OnClient2LoginConnected,
        OnClient2LoginDisConnected,
        OnClient2BattleConnected,
        OnClient2BattleDisConnected,

        ReqAccountLogin, // Request Account Login.
        RespAccountLogin, // Response Account Login.
        NtfEnterStage, // Notify Enter Stage.
        AffirmEnterStage, // Affirm Enter Stage.

        NtfRoleData, // Notify Role Data.

        ReqRoleToken, // Request Role Token.
        RespRoleToken, // Response Role Token.
        ReqTokenAccess, // Request Token Access.
        RespTokenAccess, // Response Token Access.

        SyncMovePos, // Synchronize Move Position. 客户端同步移动位置
        PushStateData, // Push State Data. 推送状态数据

        #region Battle进程内部协议 Battle process internal protocol 
        /// <summary>
        /// B2B: Battle to Battle
        /// </summary>
        B2B_SyncView, // 强制同步视野数据 Force synchronization of view data
        B2B_SyncCore, // 强制同步核心数据 Force synchronization of core data
        #endregion
    }

    public enum ErrorCode
    {
        None,

        acct_not_exist,

        /// <summary>
        /// 当前区服的战斗进程离线
        /// </summary>
        battle_process_disconnected,

        //ReqAcctLogin
        /// <summary>
        /// 账号已在login登录
        /// </summary>
        acct_online_login,
        /// <summary>
        /// 账号已在data登录
        /// </summary>
        acct_online_data,
        /// <summary>
        /// 账号对应data区服不存在
        /// </summary>
        acct_l2d_offline,
        /// <summary>
        /// 账号已封禁
        /// </summary>
        //acct_forbidden,

        //ReqTokenAccess
        token_already_online,
        token_error,
        token_not_exist,
        /// <summary>
        /// token已经过期
        /// </summary>
        token_expired,

    }
}
