using DataTools.Code.Markers;

namespace CSRefactorCurio.Reporting
{
    internal class CSReference<T> where T : IMarker
    {
        public T ReferencedObject { get; set; }

        public T CallingObject { get; set; }

        public CSReference(T calling, T referenced)
        {
            ReferencedObject = referenced;
            CallingObject = calling;
        }

        public CSReference()
        {
        }

        public override string ToString()
        {
            return $"{CallingObject.Title} => {ReferencedObject.Title}";
        }
    }
}