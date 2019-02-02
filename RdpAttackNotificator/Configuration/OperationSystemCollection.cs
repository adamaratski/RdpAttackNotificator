using System;
using System.Configuration;

namespace RdpAttackNotificator.Configuration
{
    [ConfigurationCollection(typeof(OperationSystemElement))]
    public class OperationSystemCollection : ConfigurationElementCollection
    {
        internal const String PropertyName = "OperationSystem";

        public override ConfigurationElementCollectionType CollectionType => ConfigurationElementCollectionType.BasicMapAlternate;

        protected override string ElementName => OperationSystemCollection.PropertyName;

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
            return new OperationSystemElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((OperationSystemElement)element).Version;
        }

       
        public OperationSystemElement this[int idx] => (OperationSystemElement)BaseGet(idx);
    }
}
