using DataTools.Code.Filtering.Base;
using DataTools.Code.Markers;

namespace DataTools.Code.Filtering
{
    /// <summary>
    /// A simple single positive identity filter rule.
    /// </summary>
    internal class MarkerKindFilterRule : MarkerFilterRule
    {
        public MarkerKindFilterRule(CodeElementType kind)
        {
            Kind = kind;
        }

        public CodeElementType Kind { get; }

        public override bool IsValid(IMarker item)
        {
            return Kind == item.Kind;
        }
    }
}