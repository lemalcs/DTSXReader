using System.Runtime.Serialization;

namespace DTSXExplorer
{
    [DataContract]
    internal class DTSXItem
    {
        [DataMember]
        public string DTSXName { get; set; }

        [DataMember]
        public int ItemId { get; set; }

        [DataMember]
        public string ItemType { get; set; }

        [DataMember]
        public int FieldId { get; set; }

        [DataMember]
        public string FieldName { get; set; }

        [DataMember]
        public string Value { get; set; }

        [DataMember]
        public string LinkedItemType { get; set; }
    }
}
