using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace DTSXExplorer
{
    /// <summary>
    /// Reads a DTSX file.
    /// </summary>
    internal class DTSXReader
    {
        #region Properties
        /// <summary>
        /// Contains the whole structure of a DTSX file.
        /// </summary>
        private List<DTSXItem> items = new List<DTSXItem>();

        /// <summary>
        /// The name of the DSTX file.
        /// </summary>
        public string DTSXName { get; private set; }

        /// <summary>
        /// The number of elements of a DTSX file.
        /// </summary>
        private int itemCounter = 0;

        #endregion

        #region Methods


        /// <summary>
        /// Reads a DTSX file.
        /// </summary>
        /// <param name="filePath">The path of DTSX file.</param>
        /// <returns>The list of elements found in the DTSX file.</returns>
        public List<DTSXItem> Read(string filePath)
        {
            using (FileStream fs = new FileStream(filePath,
                FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                DTSXName = Path.GetFileName(filePath);
                return ParseXML(fs);
            }
        }

        /// <summary>
        /// Reads the structure of a DTSX document.
        /// </summary>
        /// <param name="stream">The stream that contains the DTSX document.</param>
        /// <returns>The list of elements in the DTSX document.</returns>
        private List<DTSXItem> ParseXML(Stream stream)
        {
            XmlReaderSettings settings = new XmlReaderSettings();

            using (XmlReader reader = XmlReader.Create(stream, settings))
            {
                Stack<Element> parentStack = new Stack<Element>();

                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:

                            DTSXItem parentItem = null;
                            if (parentStack.Count > 0)
                            {
                                parentItem = new DTSXItem
                                {
                                    DTSXName = DTSXName,
                                    ItemId = parentStack.Peek().ItemDetail.ItemId,
                                    ItemType = parentStack.Peek().ItemDetail.ItemType,
                                    FieldId = parentStack.Peek().ChildrenCount + 1,
                                    FieldName = "_child_",
                                    Value = "",
                                    LinkedItemType = reader.Name
                                };
                                items.Add(parentItem);

                                parentStack.Peek().IncreaseChildrenCount();
                            }

                            itemCounter++;

                            // Add entry for parent
                            DTSXItem newItem = new DTSXItem
                            {
                                DTSXName = DTSXName,
                                ItemId = itemCounter,
                                ItemType = reader.Name,
                                FieldId = 0,
                                FieldName = "_parent_id",
                                Value = (parentStack.Count > 0 ? parentStack.Peek().ItemDetail.ItemId : 0).ToString(),
                                LinkedItemType = parentStack.Count > 0 ? parentStack.Peek().ItemDetail.ItemType : "root"
                            };
                            items.Add(newItem);

                            if (parentItem != null)
                                parentItem.Value = itemCounter.ToString();

                            // Add a new element to the stack only if it has descendants (elements)
                            if (!reader.IsEmptyElement)
                            {
                                parentStack.Push(new Element { ItemDetail = newItem });
                            }

                            int attributesCount = GetAtributes(reader);

                            if (newItem.ItemId == parentStack.Peek().ItemDetail.ItemId)
                                parentStack.Peek().IncreaseChildrenCountBy(attributesCount);

                            break;

                        case XmlNodeType.Text:
                            
                            // Add an entry for new item
                            itemCounter++;

                            // Add an entry for child
                            items.Add(new DTSXItem
                            {
                                DTSXName = DTSXName,
                                ItemId = parentStack.Peek().ItemDetail.ItemId,
                                ItemType = parentStack.Peek().ItemDetail.ItemType,
                                FieldId = parentStack.Peek().ChildrenCount + 1,
                                FieldName = "_child_",
                                Value = itemCounter.ToString(),
                                LinkedItemType = "TEXT"
                            });

                            parentStack.Peek().IncreaseChildrenCount();

                            // Add an entry for parent
                            items.Add(new DTSXItem
                            {
                                DTSXName = DTSXName,
                                ItemId = itemCounter,
                                ItemType = "TEXT",
                                FieldId = 0,
                                FieldName = "_parent_id",
                                Value = parentStack.Peek().ItemDetail.ItemId.ToString(),
                                LinkedItemType = parentStack.Peek().ItemDetail.ItemType
                            });

                            // Add an entry for text value
                            items.Add(new DTSXItem
                            {
                                DTSXName = DTSXName,
                                ItemId = itemCounter,
                                ItemType = "TEXT",
                                FieldId = 1,
                                FieldName = "value",
                                Value = reader.Value
                            });

                            break;

                        case XmlNodeType.EndElement:
                            parentStack.Pop();
                            break;

                        case XmlNodeType.CDATA:
                            
                            MemoryStream memoryStream = new MemoryStream();
                            byte[] cdataContent = Encoding.UTF8.GetBytes(reader.Value);
                            memoryStream.Write(cdataContent, 0, cdataContent.Length);
                            memoryStream.Position = 0;
                            try
                            {
                                // Check whether the value is a valid XML string
                                ParseXML(memoryStream);
                            }
                            catch (XmlException)
                            {
                                // Add an entry for new item
                                itemCounter++;

                                // Add an entry for the parent's child
                                items.Add(new DTSXItem
                                {
                                    DTSXName = DTSXName,
                                    ItemId = parentStack.Peek().ItemDetail.ItemId,
                                    ItemType = parentStack.Peek().ItemDetail.ItemType,
                                    FieldId = parentStack.Peek().ChildrenCount + 1,
                                    FieldName = "_child_",
                                    Value = itemCounter.ToString(),
                                    LinkedItemType = "TEXT"
                                });
                                
                                parentStack.Peek().IncreaseChildrenCount();

                                // Add an entry for parent
                                items.Add(new DTSXItem
                                {
                                    DTSXName = DTSXName,
                                    ItemId = itemCounter,
                                    ItemType = "TEXT",
                                    FieldId = 0,
                                    FieldName = "_parent_id",
                                    Value = parentStack.Peek().ItemDetail.ItemId.ToString(),
                                    LinkedItemType = parentStack.Peek().ItemDetail.ItemType
                                });

                                // Add an entry for text value
                                items.Add(new DTSXItem
                                {
                                    DTSXName = DTSXName,
                                    ItemId = itemCounter,
                                    ItemType = "TEXT",
                                    FieldId = 1,
                                    FieldName = "value",
                                    Value = reader.Value
                                });
                            }
                            break;

                        default:
                            // Don't add a section that is not used in DTSX files.
                            break;
                    }
                }
            }

            return items;
        }

        /// <summary>
        /// Gets the attributes of a XML element.
        /// </summary>
        /// <param name="reader">The stream of the XML document.</param>
        /// <returns>The number of attributes in the element.</returns>
        private int GetAtributes(XmlReader reader)
        {
            if (reader == null)
                return 0;

            if (!reader.HasAttributes)
                return 0;

            int fieldId = 1;
            while (reader.MoveToNextAttribute())
            {
                DTSXItem item = items.Last();
                items.Add(new DTSXItem
                {
                    DTSXName = item.DTSXName,
                    ItemId = item.ItemId,
                    ItemType = item.ItemType,
                    FieldId = fieldId,
                    FieldName = reader.Name,
                    Value = reader.Value
                });
                fieldId++;
            }

            return fieldId - 1;
        }

        #endregion

    }
}
