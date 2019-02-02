using System;

namespace RdpAttackNotificator.Models
{
    public interface ILockTarget
    {
        Boolean AddToBlockList(String sourceIp);

        Boolean Login(String login, String password);

        void Close();
    }
}
