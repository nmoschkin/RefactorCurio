using DataTools.Code.Markers;

namespace DataTools.Code.Filtering.Base
{
    public static class DefaultOrders
    {
        /// <summary>
        /// Gets the default sort order for filters, lists, and rules.
        /// </summary>
        public static readonly MarkerKind[] DefaultSortOrder = new MarkerKind[] {
            MarkerKind.Interface,
            MarkerKind.Class,
            MarkerKind.Record,
            MarkerKind.Struct,
            MarkerKind.Enum,
            MarkerKind.Const,
            MarkerKind.Delegate,
            MarkerKind.Constructor,
            MarkerKind.Destructor,
            MarkerKind.Method,
            MarkerKind.Property,
            MarkerKind.Field,
            MarkerKind.Operator,
            MarkerKind.EnumValue,
            MarkerKind.FieldValue,
            MarkerKind.Event,
            MarkerKind.Get,
            MarkerKind.Set,
            MarkerKind.Add,
            MarkerKind.Remove
        };

        public static readonly MarkerKind[] DefaultFQNFilter = new[]
        {
            MarkerKind.Namespace,
            MarkerKind.Class,
            MarkerKind.Interface,
            MarkerKind.Struct,
            MarkerKind.Record,
            MarkerKind.Enum,
            MarkerKind.Method,
            MarkerKind.Property,
            MarkerKind.Delegate,
            MarkerKind.Const,
            MarkerKind.Get,
            MarkerKind.Set,
            MarkerKind.Add,
            MarkerKind.Remove,
        };
    }
}