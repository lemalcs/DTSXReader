namespace DTSXExplorer
{
    /// <summary>
    /// Represent a element from XML format.
    /// </summary>
    internal class Element
    {
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
