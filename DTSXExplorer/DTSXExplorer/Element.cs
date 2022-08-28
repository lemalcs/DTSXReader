using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DTSXExplorer
{
    /// <summary>
    /// Represent a element from XML format.
    /// </summary>
    internal class Element
    {
        // The identifier assigned to Element 
        public int Id { get; set; }

        /// <summary>
        /// The name of XML element.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The current number of children.
        /// </summary>
        public int ChildrenCount { get; set; }

        /// <summary>
        /// The details of item.
        /// </summary>
        public DTSXItem ItemDetail { get; set; }
    }
}
