using DataTools.Code.Markers;
using DataTools.Text;

using System;
using System.Linq;
using System.Text;

namespace DataTools.Code.CS.Emit
{
    internal static class CSEntityBuilder
    {
        public static string PrintAccess(IMarker marker)
        {
            var access = marker.AccessModifiers;

            if (access == (AccessModifiers.Internal | AccessModifiers.Internal))
            {
                return "protected internal";
            }
            else if (access == AccessModifiers.None)
            {
                switch (marker.Kind)
                {
                    case MarkerKind.Class:
                    case MarkerKind.Struct:
                    case MarkerKind.Record:
                    case MarkerKind.Enum:
                    case MarkerKind.Delegate:
                    case MarkerKind.Interface:
                        return "internal";

                    default:
                        return "private";
                }
            }

            return TextTools.OneSpace(access.ToString().Replace(",", " ").ToLower().Trim());
        }

        /// <summary>
        /// Build the code block for the specified marker.
        /// </summary>
        /// <param name="marker">The marker that provides context and establishes structure.</param>
        /// <param name="body">The inner code (if applicable)</param>
        /// <returns>The rendered code block.</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <remarks>
        /// If you pass a type-level block, you must pass at least one body value.
        /// <br /><br />
        /// For type-level blocks, body values will be concatenated together as lines.
        /// <br /><br />
        /// For properties, if a single body is provided, then a child specifying get or set must be present.
        /// <br /><br />
        /// For events, either zero or exactly 2 bodies must be provided, for add and remove.
        /// </remarks>
        public static string BuildCodeBlock(IMarker marker, params string[] body)
        {
            var sb = new StringBuilder();

            if (marker.Attributes != null && marker.Attributes.Count > 0)
            {
                foreach (var attrib in marker.Attributes)
                {
                    sb.AppendLine($"[{attrib}]");
                }
            }

            if (marker.IsNew)
            {
                sb.Append("new ");
            }

            sb.Append(PrintAccess(marker));

            if (marker.IsPartial)
            {
                sb.Append(" partial");
            }

            if (marker.IsStatic)
            {
                sb.Append(" static");
            }
            if (marker.IsExtern)
            {
                sb.Append(" extern");
            }

            if (marker.IsUnsafe)
            {
            }

            if (marker.IsAbstract)
            {
                if (body != null || body.Length != 0)
                {
                    throw new ArgumentException("Abstract members cannot have bodies");
                }

                sb.Append(" abstract");
            }
            else if (marker.IsSealed)
            {
                sb.Append(" sealed");
            }
            else if (marker.IsVirtual)
            {
                sb.Append(" virtual");
            }
            else if (marker.IsOverride)
            {
                sb.Append(" override");
            }

            var mk = marker.Kind;
            switch (marker.Kind)
            {
                case MarkerKind.Class:
                case MarkerKind.Struct:
                case MarkerKind.Record:
                case MarkerKind.Enum:
                case MarkerKind.Interface:

                    if (marker.IsNew || marker.IsExtern || marker.IsAsync || marker.IsExplicit || marker.IsImplicit || marker.IsRef)
                    {
                        throw new ArgumentException("Type-level entities cannot possess: IsNew, IsExtern, IsAsync, IsExplicit, IsImplicit, IsRef");
                    }

                    if (marker.Kind != MarkerKind.Class && (marker.IsAbstract || marker.IsSealed || marker.IsStatic))
                    {
                        throw new ArgumentException("Only class types can be abstract, static, or sealed.");
                    }

                    if (body == null || body.Length == 0) throw new ArgumentException("Type must have a body.");

                    sb.Append($" {marker.Kind.ToString().ToLower()} {marker.Name}");

                    if (!string.IsNullOrEmpty(marker.Generics))
                    {
                        sb.Append(marker.Generics);
                    }

                    if (!string.IsNullOrEmpty(marker.InheritanceString))
                    {
                        sb.AppendLine($" : {marker.InheritanceString}");
                    }

                    if (!string.IsNullOrEmpty(marker.WhereClause))
                    {
                        sb.AppendLine(marker.WhereClause);
                    }

                    sb.AppendLine("{");
                    sb.AppendLine(string.Join("\r\n", body));
                    sb.AppendLine("}");

                    return sb.ToString();

                default:
                    break;
            }

            if (mk == MarkerKind.Operator)
            {
                if (!marker.IsStatic) throw new ArgumentException("Operator must be static");

                if (body == null || body.Length == 0) throw new ArgumentException("Operator must have a body");

                if (marker.DataType == "bool" && !marker.IsExplicit && !marker.IsImplicit)
                {
                    sb.Append($" bool operator {marker.Name}");
                }
                else
                {
                    if (marker.IsImplicit)
                    {
                        sb.Append($" implicit operator {marker.Name}");
                    }
                    else if (marker.IsExplicit)
                    {
                        sb.Append($" explicit operator {marker.Name}");
                    }
                    else
                    {
                        throw new ArgumentException("Operator is neither explicit nor implicit, and data type is not boolean");
                    }
                }

                sb.Append($"({marker.MethodParamsString})");

                if (body[0].StartsWith("=>"))
                {
                    sb.Append(" " + body[0]);
                    if (!body[0].Trim().EndsWith(";")) sb.Append(";");
                }
                else
                {
                    sb.AppendLine();
                    sb.AppendLine("{");
                    sb.AppendLine(string.Join("\r\n", body));
                    sb.AppendLine("}");
                }
            }
            else if (mk == MarkerKind.Method)
            {
                if (!marker.IsAbstract && (body == null || body.Length == 0)) throw new ArgumentException("Method must have a body.");

                if (marker.IsAsync) sb.Append(" async");

                if (marker.IsRef) sb.Append(" ref");

                sb.Append($" {marker.DataType} {marker.Name}");
                if (!string.IsNullOrEmpty(marker.Generics))
                {
                    sb.Append(marker.Generics);
                }
                sb.AppendLine($"({marker.MethodParamsString})");
                if (!string.IsNullOrEmpty(marker.WhereClause))
                {
                    sb.Append(marker.WhereClause);
                }

                if (marker.IsAbstract)
                {
                    sb.AppendLine(";");
                    return sb.ToString();
                }

                if (body.Length == 1 && body[0].StartsWith("=>"))
                {
                    sb.Append(" " + body[0]);
                    if (!body[0].Trim().EndsWith(";")) sb.Append(";");
                }
                else
                {
                    sb.AppendLine();
                    sb.AppendLine("{");
                    sb.AppendLine(string.Join("\r\n", body));
                    sb.AppendLine("}");
                }

                return sb.ToString();
            }
            else if (mk == MarkerKind.Delegate)
            {
                sb.Append($" delegate {marker.DataType} {marker.Name}");
                if (!string.IsNullOrEmpty(marker.Generics))
                {
                    sb.Append(marker.Generics);
                }
                sb.Append($"({marker.MethodParamsString})");
                if (!string.IsNullOrEmpty(marker.WhereClause))
                {
                    sb.Append(marker.WhereClause);
                }

                sb.AppendLine(";");
                return sb.ToString();
            }
            else if (mk == MarkerKind.Field)
            {
                if (marker.IsReadOnly)
                {
                    sb.Append(" readonly");
                }

                sb.Append($" {marker.DataType} {marker.Name}");
                if (body != null && body.Length == 1 && body[0].StartsWith("= "))
                {
                    sb.Append(" " + body[0]);
                    if (!body[0].Trim().EndsWith(";")) sb.Append(";");
                }

                sb.AppendLine();
                return sb.ToString();
            }
            else if (mk == MarkerKind.Property || mk == MarkerKind.This)
            {
                if (marker.IsRef) sb.Append(" ref");
                sb.Append($" {marker.DataType} {marker.Name}");

                if (marker.Kind == MarkerKind.This)
                {
                    sb.Append($"[{marker.MethodParamsString}]");
                }

                bool hg = false;
                bool hs = false;

                if (marker.Children != null)
                {
                    foreach (IMarker m in marker.Children)
                    {
                        if (m.Kind == MarkerKind.Set)
                        {
                            hs = true;
                        }
                        else if (m.Kind == MarkerKind.Get)
                        {
                            hg = true;
                        }
                    }
                }

                if (hg && !hs && body != null && body.Length == 1 && body[0].StartsWith("=>"))
                {
                    sb.Append(" " + body[0]);
                    if (!body[0].Trim().EndsWith(";")) sb.Append(";");
                    return sb.ToString();
                }

                if (marker.Kind == MarkerKind.This)
                {
                    if (body == null || body.Length == 0) throw new ArgumentException("This indexer must have bodies");
                    else if (hg == hs && body.Length != 2) throw new ArgumentException("Too few arguments for this member.");
                    else if (hg != hs && body.Length != 1) throw new ArgumentException("Too many arguments for this member.");
                }

                if ((body == null || body.Length == 0) || (marker.Kind != MarkerKind.This && body != null && body.Length == 1 && body[0].StartsWith("= ")))
                {
                    if (!hg && !hs)
                    {
                        sb.Append(" { get; set; }");
                        if (body?.Length == 1)
                        {
                            sb.Append(" " + body[0]);
                            if (!body[0].Trim().EndsWith(";")) sb.Append(";");
                        }

                        sb.AppendLine();

                        return sb.ToString();
                    }
                    else
                    {
                        sb.Append(" {");
                        if (hg) sb.Append(" get;");
                        if (hs) sb.Append(" set;");
                        sb.Append(" }");

                        if (body?.Length == 1)
                        {
                            sb.Append(" " + body[0]);
                            if (!body[0].Trim().EndsWith(";")) sb.Append(";");
                        }

                        sb.AppendLine();

                        return sb.ToString();
                    }
                }
                else
                {
                    if (body.Length == 1 && hg && hs) throw new ArgumentException("Too few arguments for provided IMarker.");
                    else if (body.Length == 2 && (hg ^ hs)) throw new ArgumentException("Too many arguments for provided IMarker.");
                    else if (body.Length > 2) throw new ArgumentException("Too many arguments for provided IMarker.");

                    if (body.Length == 2 && (hg == hs))
                    {
                        sb.AppendLine();
                        sb.AppendLine("{");

                        if (body[0].StartsWith("=>"))
                        {
                            sb.Append("    get " + body[0]);
                            if (!body[0].Trim().EndsWith(";")) sb.Append(";");
                            sb.AppendLine();
                        }
                        else
                        {
                            sb.AppendLine("    get");
                            sb.AppendLine("    {");
                            sb.AppendLine("    " + body[0]);
                            sb.AppendLine("    }");
                        }

                        if (body[1].StartsWith("=>"))
                        {
                            sb.Append("    set " + body[1]);
                            if (!body[1].Trim().EndsWith(";")) sb.Append(";");
                            sb.AppendLine();
                        }
                        else
                        {
                            sb.AppendLine("    set");
                            sb.AppendLine("    {");
                            sb.AppendLine("    " + body[1]);
                            sb.AppendLine("    }");
                        }

                        sb.AppendLine("}");
                        return sb.ToString();
                    }
                    else if (body.Length == 1)
                    {
                        sb.AppendLine();
                        sb.AppendLine("{");

                        if (body[0].StartsWith("=>"))
                        {
                            if (hg) sb.Append("    get " + body[0]);
                            else if (hs) sb.Append("    set " + body[0]);
                            if (!body[0].Trim().EndsWith(";")) sb.Append(";");
                            sb.AppendLine();
                        }
                        else
                        {
                            if (hg) sb.AppendLine("    get");
                            else if (hs) sb.AppendLine("    set");
                            sb.AppendLine("    {");
                            sb.AppendLine("    " + body[0]);
                            sb.AppendLine("    }");
                        }

                        sb.AppendLine("}");

                        return sb.ToString();
                    }
                }
            }
            else if (mk == MarkerKind.Event)
            {
                sb.Append($" event {marker.DataType} {marker.Name}");

                bool hg = false;
                bool hs = false;

                if (marker.Children != null)
                {
                    foreach (IMarker m in marker.Children)
                    {
                        if (m.Kind == MarkerKind.Add)
                        {
                            hs = true;
                        }
                        else if (m.Kind == MarkerKind.Remove)
                        {
                            hg = true;
                        }
                    }
                }

                if (hs != hg) throw new ArgumentException("Marker has invalid children. Events must provide both add and remove.");

                if (body == null || body.Length == 0)
                {
                    if (!hg && !hs)
                    {
                        sb.AppendLine(";");
                        return sb.ToString();
                    }
                    else
                    {
                        throw new ArgumentException("Event add and remove children were indicated, but no bodies were passed.");
                    }
                }
                else
                {
                    if (body.Length < 2) throw new ArgumentException("Too few arguments for provided IMarker.");
                    else if (body.Length > 2) throw new ArgumentException("Too many arguments for provided IMarker.");

                    if (body.Length == 2 && (hg == hs))
                    {
                        sb.AppendLine();
                        sb.AppendLine("{");

                        if (body[0].StartsWith("=>"))
                        {
                            sb.Append("    add " + body[0]);
                            if (!body[0].Trim().EndsWith(";")) sb.Append(";");
                            sb.AppendLine();
                        }
                        else
                        {
                            sb.AppendLine("    add");
                            sb.AppendLine("    {");
                            sb.AppendLine("    " + body[0]);
                            sb.AppendLine("    }");
                        }

                        if (body[1].StartsWith("=>"))
                        {
                            sb.Append("    remove " + body[1]);
                            if (!body[1].Trim().EndsWith(";")) sb.Append(";");
                            sb.AppendLine();
                        }
                        else
                        {
                            sb.AppendLine("    remove");
                            sb.AppendLine("    {");
                            sb.AppendLine("    " + body[1]);
                            sb.AppendLine("    }");
                        }

                        sb.AppendLine("}");
                        return sb.ToString();
                    }
                }
            }

            return sb.ToString();
        }
    }
}