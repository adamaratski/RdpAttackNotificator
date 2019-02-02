using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RdpAttackNotificator.Configuration
{
    public class AppConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("OperationSystems")]
        public OperationSystemCollection OperationSystems
        {
            get => (OperationSystemCollection)base["OperationSystems"];

            set => base["OperationSystems"] = value;
        }

        [ConfigurationProperty("LockTargets")]
        public LockTargetCollection LockTargets
        {
            get => (LockTargetCollection)base["LockTargets"];

            set => base["LockTargets"] = value;
        }

        [ConfigurationProperty("WhiteList", IsKey = false, IsRequired = true, IsDefaultCollection = true)]
        public NameValueConfigurationCollection WhiteList
        {
            get => (NameValueConfigurationCollection)this["WhiteList"];
            set => this["WhiteList"] = value;
        }
    }
}
