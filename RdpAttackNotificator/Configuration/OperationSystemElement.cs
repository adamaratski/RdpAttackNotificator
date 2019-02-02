using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;

namespace RdpAttackNotificator.Configuration
{
    public class OperationSystemElement : ConfigurationElement
    {
        /// <summary>
        /// Gets or sets OS version.
        /// </summary>
        [ConfigurationProperty("Version", IsKey = true, IsRequired = true)]
        public String Version
        {
            get => (String)this["Version"];
            set => this["Version"] = value;
        }

        /// <summary>
        /// Gets or sets instance Id.
        /// </summary>
        [ConfigurationProperty("InstanceId", IsKey = false, IsRequired = true)]
        public Int32 InstanceId
        {
            get => (Int32)this["InstanceId"];
            set => this["InstanceId"] = value;
        }

        /// <summary>
        /// Gets or sets the source.
        /// </summary>
        [ConfigurationProperty("Source", IsKey = false, IsRequired = true)]
        public String Source
        {
            get => (String)this["Source"];
            set => this["Source"] = value;
        }

        [ConfigurationProperty("SearchPatterns", IsKey = false, IsRequired = true, IsDefaultCollection = true)]
        public NameValueConfigurationCollection SearchPatterns
        {
            get => (NameValueConfigurationCollection)this["SearchPatterns"];
            set => this["SearchPatterns"] = value;
        }

        [ConfigurationProperty("IsDefault", IsKey = false, IsRequired = true)]
        public Boolean IsDefault
        {
            get => (Boolean)this["IsDefault"];
            set => this["IsDefault"] = value;
        }
    }
}
