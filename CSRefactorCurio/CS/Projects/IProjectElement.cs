namespace DataTools.CSTools
{
    /// <summary>
    /// Represents a basic project element.
    /// </summary>
    internal interface IProjectElement
    {
        /// <summary>
        /// The element type.
        /// </summary>
        ElementType ElementType { get; }

        /// <summary>
        /// The title of the element.
        /// </summary>
        string Title { get; }
    }
}