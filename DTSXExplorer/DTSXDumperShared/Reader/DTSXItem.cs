using System.Runtime.Serialization;

namespace DTSXDumper
{
    /// <summary>
    /// Holds information about a DTSX file.
    /// </summary>
    [DataContract]
    public class DTSXItem
    {
        /// <summary>
        /// The name of DTSX.
        /// </summary>
        [DataMember]
        public string DTSXName { get; set; }

        /// <summary>
        /// A unique ID assigned to a tag from XML document.
        /// </summary>
        [DataMember]
        public int ItemId { get; set; }

        /// <summary>
        /// The name of tag.
        /// </summary>
        [DataMember]
        public string ItemType { get; set; }

        /// <summary>
        /// A unique ID assigned to a attribute from XML document.
        /// </summary>
        [DataMember]
        public int FieldId { get; set; }

        /// <summary>
        /// The name of attribute.
        /// </summary>
        [DataMember]
        public string FieldName { get; set; }

        /// <summary>
        /// The value of attribute.
        /// </summary>
        [DataMember]
        public string Value { get; set; }

        /// <summary>
        /// The parent name (<see cref="ItemType"/>) of tag only if <see cref="FieldName"/> is _parent_id; 
        /// otherwise an empty string.
        /// </summary>
        [DataMember]
        public string LinkedItemType { get; set; }
    }
}
