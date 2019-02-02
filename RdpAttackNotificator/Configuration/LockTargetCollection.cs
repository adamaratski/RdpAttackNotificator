using System;
using System.Configuration;

namespace RdpAttackNotificator.Configuration
{
    [ConfigurationCollection(typeof(LockTargetElement))]
    public class LockTargetCollection : ConfigurationElementCollection
    {
        internal const String PropertyName = "LockTarget";

        public override ConfigurationElementCollectionType CollectionType => ConfigurationElementCollectionType.BasicMapAlternate;

        protected override string ElementName => LockTargetCollection.PropertyName;

        protected override Boolean IsElementName(string elementName)
        {
            return elementName.Equals(PropertyName, StringComparison.InvariantCultureIgnoreCase);
        }
       
        public override bool IsReadOnly()
        {
            return false;
        }
       
        protected override ConfigurationElement CreateNewElement()
        {
            return new LockTargetElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((LockTargetElement)element).Index;
        }

       
        public LockTargetElement this[int idx] => (LockTargetElement)BaseGet(idx);
    }
}
