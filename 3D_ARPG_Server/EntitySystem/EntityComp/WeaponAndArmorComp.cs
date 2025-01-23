using RCCommon;
using RCProtocol;

namespace ARPGServer
{
    public class WeaponAndArmorComp : EntityComp, IUpdate
    {
        public string currentWeaponName = ""; // 当前武器名称
        string lastWeaponName = ""; // 上次武器名称

        public string currentArmorName = ""; // 当前护甲名称
        string lastArmorName = ""; // 上次护甲名称

        public void Update()
        {
            if (currentWeaponName != lastWeaponName)
            {
                lastWeaponName = currentWeaponName;
                SendSwitchWeapon();
            }
        }

        public void SendSwitchWeapon()
        {
            NetMsg msg = new NetMsg
            {
                cmd = CMD.SwitchWeapon,
                switchWeapon = new SwitchWeapon
                {
                    roleID = parent.roleID,
                    account = parent.account,
                    weaponName = currentWeaponName
                }
            };

            ARPGProcess.Instance.entitySystem.SendToAll(msg, parent.gameToken);
        }
    }
}