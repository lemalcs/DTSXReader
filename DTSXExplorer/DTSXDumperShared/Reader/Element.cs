namespace DTSXDumper
{
    /// <summary>
    /// Represent a element from XML format.
    /// </summary>
    public class Element
    {
        /// <summary>
        /// The current number of children, these include attributes and descendant tags 
        /// in XML file.
        /// </summary>
        public int ChildrenCount { get; private set; }

        /// <summary>
        /// The details of item.
        /// </summary>
        public DTSXItem ItemDetail { get; set; }

        /// <summary>
        /// Increases the number of children by 1.
        /// </summary>
        /// <returns>The current number of children.</returns>
        public int IncreaseChildrenCount()
        {
            return ++ChildrenCount;
        }

        /// <summary>
        /// Increases the number on children by a specific amount.
        /// </summary>
        /// <param name="childrenCount">The amount to increase to.</param>
        /// <returns>The current number of children.</returns>
        public int IncreaseChildrenCountBy(int childrenCount)
        {
            ChildrenCount += childrenCount;
            return ChildrenCount;
        }
    }
}
