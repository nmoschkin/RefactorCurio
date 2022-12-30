namespace DataTools.CSTools
{
    /// <summary>
    /// A simple single positive identity filter rule based on level and kind.
    /// </summary>
    internal class MarkerLevelFilterRule : MarkerKindFilterRule
    {
        /// <summary>
        /// Create a new market filter level rule.
        /// </summary>
        /// <param name="kind">The kind</param>
        /// <param name="level">The level</param>
        public MarkerLevelFilterRule(MarkerKind kind, int level) : base(kind)
        {
            Level = level;
        }

        /// <summary>
        /// Gets the allowed level for this rule.
        /// </summary>
        public int Level { get; }

        public override bool IsValid(IMarker item)
        {
            return Kind == item.Kind && Level == item.Level;
        }
    }
}