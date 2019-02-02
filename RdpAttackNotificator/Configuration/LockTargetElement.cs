using System;
using System.Configuration;
using RdpAttackNotificator.Models;

namespace RdpAttackNotificator.Configuration
{
    public class LockTargetElement : ConfigurationElement
    {
       
        [ConfigurationProperty("Index", IsKey = true, IsRequired = true)]
        public Int32 Index
        {
            get => (Int32)this["Index"];
            set => this["Index"] = value;
        }
       
        [ConfigurationProperty("TargetType", IsKey = false, IsRequired = true)]
        public TargetType TargetType
        {
            get => (TargetType)this["TargetType"];
            set => this["TargetType"] = value;
        }

        [ConfigurationProperty("Target", IsKey = false, IsRequired = true)]
        public String Target
        {
            get => (String)this["Target"];
            set => this["Target"] = value;
        }

        [ConfigurationProperty("Port", IsKey = false, IsRequired = true)]
        public Int32 Port
        {
            get => (Int32)this["Port"];
            set => this["Port"] = value;
        }

        [ConfigurationProperty("User", IsKey = false, IsRequired = true)]
        public String User
        {
            get => (String)this["User"];
            set => this["User"] = value;
        }

        [ConfigurationProperty("Password", IsKey = false, IsRequired = true)]
        public String Password
        {
            get => (String)this["Password"];
            set => this["Password"] = value;
        }

        [ConfigurationProperty("BlackList", IsKey = false, IsRequired = true)]
        public String BlackList
        {
            get => (String)this["BlackList"];
            set => this["BlackList"] = value;
        }

        [ConfigurationProperty("LockPeriod", IsKey = false, IsRequired = true)]
        public String LockPeriod
        {
            get => (String)this["LockPeriod"];
            set => this["LockPeriod"] = value;
        }
    }
}
