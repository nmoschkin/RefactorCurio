using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSRefactorCurio.Helpers
{
    internal static class ReferenceFinder
    {
        private static List<string> MakeNS(string ns)
        {
            var sp = ns.Split('.');

            int i, j;
            int c = sp.Length;
            var sb = new StringBuilder();
            var l = new List<string>();

            for (i = 0; i < c; i++)
            {
                sb.Clear();

                for (j = 0; j <= i; j++)
                {
                    if (j > 0) sb.Append('.');
                    sb.Append(sp[j]);
                }

                l.Add(sb.ToString());
            }

            return l;
        }

        //public static IEnumerable<IMarker> FindReferences(this IMarker marker, IEnumerable<CSCodeFile> context)
        //{
        //    var fqn = marker.FullyQualifiedName;
        //    var nss = MakeNS(fqn);

        //    foreach (var mc in context)
        //    {
        //    }
        //}
    }
}