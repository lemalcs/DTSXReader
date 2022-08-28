using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        /// Contains the counters per elements into a DTSX.
        /// </summary>
        private Dictionary<string, int> elementCounters = new Dictionary<string, int>();

        /// <summary>
        /// Contains the whole structure of a DTSX.
        /// </summary>
        private List<DTSXItem> items = new List<DTSXItem>();

        /// <summary>
        /// The name of the DSTX file.
        /// </summary>
        public string DTSXName { get; private set; }

        /// <summary>
        /// The number of elements of a DTSX.
        /// </summary>
        private int itemCounter = 0;

        public Dictionary<string, int> ElementCounters 
        { 
            get => elementCounters; 
            private set => elementCounters = value; 
        }

        #endregion

        #region Methods


        /// <summary>
        /// Read a DTSX file.
        /// </summary>
        /// <param name="filePath">The path of DTSX file.</param>
        /// <returns>The list of elements of DTSX file.</returns>
        public List<DTSXItem> Read(string filePath)
        {
            using (FileStream fs = new FileStream(filePath,
                FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                DTSXName=Path.GetFileName(filePath);
                return ParseXML(fs);
            }
        }

        /// <summary>
        /// Read the structure of DTSX.
        /// </summary>
        /// <param name="stream">The stream containing the DTSX document.</param>
        /// <returns>The list of elements found.</returns>
        private List<DTSXItem> ParseXML(Stream stream)
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            
            using (XmlReader reader = XmlReader.Create(stream, settings))
            {
                int currentDepth=-1;
                string currentParent=string.Empty;
                Stack<Element> parentStack=new Stack<Element>();


                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:

                            DTSXItem childItem = null;
                            if(parentStack.Count>0)
                            {
                                parentStack.Peek().ChildrenCount++;
                                Console.WriteLine($"Child _{parentStack.Peek().ChildrenCount} of {parentStack.Peek().Name}.");
                                var currentFieldId = items.Where(x => x.ItemId == parentStack.Peek().Id).Select(x => x.FieldId).Max();
                                childItem = new DTSXItem
                                {
                                    DTSXName = DTSXName,
                                    ItemId = parentStack.Peek().Id,
                                    ItemType = parentStack.Peek().Name,
                                    FieldId = currentFieldId+1,
                                    FieldName = "_child_",
                                    Value = "",
                                    LinkedItemType = reader.Name
                                };
                                items.Add(childItem);
                            }
                            
                            Console.WriteLine("Start Element {0}", reader.Name);

                            itemCounter++;

                            DTSXItem newItem = new DTSXItem
                            {
                                DTSXName = DTSXName,
                                ItemId = itemCounter,
                                ItemType = reader.Name,
                                FieldId = 0,
                                FieldName = "_parent_id",
                                Value = (parentStack.Count > 0 ? parentStack.Peek().Id : 0).ToString(),
                                LinkedItemType = parentStack.Count > 0 ? parentStack.Peek().Name : "root"
                            };
                            items.Add(newItem);

                            if(childItem != null)
                                childItem.Value = itemCounter.ToString();

                            if (!reader.IsEmptyElement)
                            {
                                parentStack.Push(new Element { Name = reader.Name, ChildrenCount = 0, Id=itemCounter,ItemDetail=newItem });
                            }
                            
                            bool isEmptyElement=reader.IsEmptyElement;

                            currentDepth = reader.Depth;
                            IncreaseCounterForElement(reader.Name);
                            GetAtributes(reader);

                            break;
                        case XmlNodeType.Text:
                            Console.WriteLine("Text Node: {0}", reader.Value);
                            parentStack.Peek().ChildrenCount++;
                            Console.WriteLine($"Child _{parentStack.Peek().ChildrenCount} of {parentStack.Peek().Name}.");
                            currentDepth=reader.Depth;

                            // Add an entry for new item
                            itemCounter++;
                            
                            // Add an entry for child
                            items.Add(new DTSXItem
                            {
                                DTSXName = DTSXName,
                                ItemId = parentStack.Peek().ItemDetail.ItemId,
                                ItemType = parentStack.Peek().ItemDetail.ItemType,
                                FieldId = parentStack.Peek().ItemDetail.FieldId + 1,
                                FieldName = "_child_",
                                Value = itemCounter.ToString(),
                                LinkedItemType = "TEXT"
                            });


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
                                FieldId = 0,
                                FieldName = "value",
                                Value = reader.Value  
                            });

                            break;
                        case XmlNodeType.EndElement:
                            Console.WriteLine("End Element {0}", reader.Name);
                            parentStack.Pop();
                            break;
                        case XmlNodeType.CDATA:
                            if (parentStack.Count > 0)
                            {
                                parentStack.Peek().ChildrenCount++;
                                Console.WriteLine($"Child _{parentStack.Peek().ChildrenCount} of {parentStack.Peek().Name}.");
                            }

                            MemoryStream memoryStream = new MemoryStream();
                            byte[] cdataContent = Encoding.UTF8.GetBytes(reader.Value);
                            memoryStream.Write(cdataContent,0,cdataContent.Length);
                            memoryStream.Position = 0;
                            try
                            {
                                // Check whether the value is a valid XML string
                                ParseXML(memoryStream);
                            }
                            catch (XmlException)
                            {
                                Console.WriteLine("Text Node: {0}", reader.Value);
                                currentDepth = reader.Depth;
                            }
                            break;
                        default:
                            Console.WriteLine("Other node {0} with value {1}",
                                            reader.NodeType, reader.Value);
                            break;
                    }
                }
            }

            return items;
        }

        /// <summary>
        /// Increase the counter of elements found in the XML document.
        /// </summary>
        /// <param name="element">The element to count from.</param>
        /// <returns>The number of current counter for the element.</returns>
        private int IncreaseCounterForElement(string element)
        {
            if(ElementCounters.ContainsKey(element))
                ElementCounters[element]++;
            else
                ElementCounters.Add(element, 1);

            return ElementCounters[element];
        }

        /// <summary>
        /// Get the attributes of a XML element.
        /// </summary>
        /// <param name="reader">The stream of the XML document.</param>
        private void GetAtributes(XmlReader reader)
        {
            if (reader == null)
                return;

            if (!reader.HasAttributes)
                return;

            int fieldId = 1;
            while(reader.MoveToNextAttribute())
            {
                Console.WriteLine($"Attribute name {reader.Name} with value {reader.Value}");
                DTSXItem item = items.Last();
                items.Add(new DTSXItem
                {
                    DTSXName = item.DTSXName,
                    ItemId=item.ItemId,
                    ItemType=item.ItemType,
                    FieldId=fieldId,
                    FieldName=reader.Name,
                    Value=reader.Value
                });
                fieldId++;
            }
        }

        #endregion

    }
}
