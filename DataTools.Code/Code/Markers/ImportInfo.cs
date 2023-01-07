using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace DataTools.Code.Markers
{
    internal class ImportInfo : IImportInfo
    {
        /// <summary>
        /// Parse import information from an attribute declare.
        /// </summary>
        /// <param name="attrdecl"></param>
        /// <returns></returns>
        public static ImportInfo Parse(string attrdecl, string method)
        {
            var rext = new Regex(@"DllImport\(""([a-zA-Z0-9.]+)"".*");
            var nobj = new ImportInfo();

            Regex rentry;

            var pubs = typeof(ImportInfo).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            var m = rext.Match(attrdecl);
            if (m.Success)
            {
                nobj.Library = m.Groups[1].Value;
                var ext = Path.GetExtension(nobj.Library);

                if (string.IsNullOrEmpty(ext) || ext == ".")
                {
                    nobj.Library += ".dll";
                }

                foreach (var prop in pubs)
                {
                    var pn = prop.Name;

                    switch (pn)
                    {
                        case nameof(CallingConvention):
                        case nameof(CharSet):
                            rentry = new Regex(@".*" + pn + @"\s*\=\s*([A-Za-z0-9.]+).*");
                            m = rentry.Match(attrdecl);

                            if (m.Success)
                            {
                                var sps = m.Groups[1].Value.Split('.');
                                var enumval = sps?.LastOrDefault();

                                if (pn == nameof(CallingConvention))
                                {
                                    if (Enum.TryParse<CallingConvention>(enumval, out var result))
                                    {
                                        nobj.CallingConvention = result;
                                    }
                                }
                                else
                                {
                                    if (Enum.TryParse<CharSet>(enumval, out var result))
                                    {
                                        nobj.CharSet = result;
                                    }
                                }
                            }
                            break;

                        case nameof(Library):
                            continue;

                        case nameof(EntryPoint):
                            rentry = new Regex(@".*" + pn + @"\s*\=\s*""(\w+)"".*");

                            m = rentry.Match(attrdecl);
                            if (m.Success)
                            {
                                prop.SetValue(nobj, m.Groups[1].Value);
                            }
                            break;

                        default:
                            rentry = new Regex(@".*" + pn + @"\s*\=\s*(\w+).*");

                            m = rentry.Match(attrdecl);

                            if (m.Success)
                            {
                                prop.SetValue(nobj, bool.Parse(m.Groups[1].Value));
                            }

                            break;
                    }
                }
            }

            if (string.IsNullOrEmpty(nobj.EntryPoint))
            {
                nobj.EntryPoint = method;
            }

            return nobj;
        }

        public virtual string Library { get; set; }
        public virtual bool BestFitMapping { get; set; }
        public virtual CallingConvention CallingConvention { get; set; } = CallingConvention.Winapi;
        public virtual CharSet CharSet { get; set; } = CharSet.Ansi;
        public virtual string EntryPoint { get; set; }
        public virtual bool ExactSpelling { get; set; }
        public virtual bool PreserveSig { get; set; }
        public virtual bool SetLastError { get; set; }
        public virtual bool ThrowOnUnmappableChar { get; set; }

        public override string ToString()
        {
            return $"{EntryPoint} [Module: {Library}, CharSet: {CharSet}]";
        }

        public bool Equals(IImportInfo other)
        {
            return
                this.Library == other.Library &&
                this.BestFitMapping == other.BestFitMapping &&
                this.CallingConvention == other.CallingConvention &&
                this.CharSet == other.CharSet &&
                this.EntryPoint == other.EntryPoint &&
                this.ExactSpelling == other.ExactSpelling &&
                this.PreserveSig == other.PreserveSig &&
                this.SetLastError == other.SetLastError &&
                this.ThrowOnUnmappableChar == other.ThrowOnUnmappableChar;
        }

        public override bool Equals(object obj)
        {
            if (obj is IImportInfo other) return Equals(other);
            return false;
        }

        public override int GetHashCode()
        {
            return
                (Library,
                BestFitMapping,
                CallingConvention,
                CharSet,
                EntryPoint,
                ExactSpelling,
                PreserveSig,
                SetLastError,
                ThrowOnUnmappableChar).GetHashCode();
        }

        public ImportInfo Clone()
        {
            return (ImportInfo)MemberwiseClone();
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
    }
}