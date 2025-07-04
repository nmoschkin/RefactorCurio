﻿using DataTools.Essentials.Observable;
using DataTools.MathTools;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace DataTools.Code.JS
{
    /// <summary>
    /// Json to C# Class Generator
    /// </summary>
    public class CSJsonClassGenerator : ObservableBase, IJsonCSSettings
    {
        private string className;
        private string code;
        private FPType floatType = FPType.Decimal;
        private bool generateDocStrings = true;
        private bool genTC = true;
        private bool hastime = false;
        private IndeterminateType indType = IndeterminateType.Float;
        private IntType intType = IntType.Long;
        private int ipidx = 0;
        private bool isInvalid = true;
        private ObservableCollection<string> jsonComments = new ObservableCollection<string>();
        private bool mvvm = false;
        private string nameSpace;
        private string text;

        /// <summary>
        /// Instantiate a new <see cref="CSJsonClassGenerator"/> with the specified JSON text and class name.
        /// </summary>
        /// <param name="text">The JSON text to parse.</param>
        /// <param name="className">The name of the new class to generate.</param>
        /// <param name="nameSpace">The namespace to put the class in.</param>
        public CSJsonClassGenerator(string text, string className, string nameSpace)
        {
            this.text = text;
            ClassName = className;
            Namespace = nameSpace;

            JsonToCode();
        }

        /// <summary>
        /// Instantiate a new <see cref="CSJsonClassGenerator"/> with the specified class name.
        /// </summary>
        /// <param name="className">The name of the new class to generate.</param>
        /// <param name="nameSpace">The namespace to put the class in.</param>
        public CSJsonClassGenerator(string className, string nameSpace)
        {
            ClassName = className;
            Namespace = nameSpace;
        }

        /// <summary>
        /// Instantiate a new <see cref="CSJsonClassGenerator"/> with the specified class name.
        /// </summary>
        /// <param name="className">The name of the new class to generate.</param>
        public CSJsonClassGenerator(string className)
        {
            ClassName = className;
        }

        /// <summary>
        /// Instantiate a new <see cref="CSJsonClassGenerator"/>.
        /// </summary>
        public CSJsonClassGenerator()
        {
        }

        /// <summary>
        /// Gets or sets of the name of the main class.
        /// </summary>
        public string ClassName
        {
            get => className;
            set
            {
                SetProperty(ref className, value);
            }
        }

        /// <summary>
        /// Gets or sets what types will be used for floating point properties.
        /// </summary>
        public FPType FloatNumberType
        {
            get => floatType;
            set
            {
                SetProperty(ref floatType, value);
            }
        }

        /// <summary>
        /// Generate XML Document Markup
        /// </summary>
        public bool GenerateDocStrings
        {
            get => generateDocStrings;
            set
            {
                SetProperty(ref generateDocStrings, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to generate time conversion classes if UNIX-like time conversions are detected.
        /// </summary>
        /// <remarks>
        /// If this property is set to true, any long values from the example data that correspond to a UNIX-like (seconds, milliseconds, or nanoseconds) timestamp within 5 years of the current date will automatically be regarded as <see cref="DateTime"/> fields, and the required <see cref="JsonConverter{T}"/>-derived classes will be generated to decode these values.<br/><br/>
        /// If this property is set to false, long values will be converted, as-is.
        /// </remarks>
        public bool GenerateTimeConverter
        {
            get => genTC;
            set
            {
                SetProperty(ref genTC, value);
            }
        }

        /// <summary>
        /// Gets or sets what kind of number to default to if the number type is potentially indeterminate.
        /// </summary>
        public IndeterminateType IndeterminateType
        {
            get => indType;
            set
            {
                SetProperty(ref indType, value);
            }
        }

        /// <summary>
        /// Gets or sets what type will be used for integer properties.
        /// </summary>
        public IntType IntNumberType
        {
            get => intType;
            set
            {
                SetProperty(ref intType, value);
            }
        }

        public bool IsInvalid
        {
            get => isInvalid;
            protected set
            {
                SetProperty(ref isInvalid, value);
            }
        }

        public ObservableCollection<string> JsonComments
        {
            get => jsonComments;
            protected set
            {
                SetProperty(ref jsonComments, value);
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates generating code for MVVM property setters using SetProperty(ref T, value).
        /// </summary>
        public bool MVVMSetProperty
        {
            get => mvvm;
            set
            {
                SetProperty(ref mvvm, value);
            }
        }

        /// <summary>
        /// Gets or sets the optional namespace to put the generated class in.
        /// </summary>
        public string Namespace
        {
            get => nameSpace;
            set
            {
                SetProperty(ref nameSpace, value);
            }
        }

        /// <summary>
        /// The generated C# class code.
        /// </summary>
        public string OutputCode => code;

        /// <summary>
        /// Gets the input JSON
        /// </summary>
        public string Text
        {
            get => text;
            set
            {
                if (SetProperty(ref text, value))
                {
                    JsonToCode();
                }
            }
        }

        /// <summary>
        /// Parse the specified JSON text into a C# class.
        /// </summary>
        /// <param name="text">The JSON text to parse.</param>
        public void Parse(string text)
        {
            if (ClassName == null) throw new ArgumentNullException($"{nameof(ClassName)} cannot be null.");

            this.text = text.Trim();
            JsonToCode();
        }

        /// <summary>
        /// Parse the specified JSON text into a C# class of the specified name.
        /// </summary>
        /// <param name="text">The JSON text to parse.</param>
        /// <param name="className">The name of the new class to generate.</param>
        public void Parse(string text, string className)
        {
            ClassName = className.Trim();
            this.text = text.Trim();

            JsonToCode();
        }

        private void FindJsonComments(string text)
        {
            ipidx = 0;
            text = text.Replace("\r\n", "\n").Replace("\r", "\n");

            var lines = text.Split('\n');
            jsonComments.Clear();

            foreach (var t in lines)
            {
                int i = t.IndexOf("//");
                if (i != -1)
                {
                    var c = t.Substring(i + 2).Trim();

                    c = c.Substring(0, 1).ToUpper() + c.Substring(1);
                    jsonComments.Add(c);
                }
            }
        }

        private string FirstToUL(string text, bool toUpper)
        {
            char[] chars = text.ToCharArray();
            if (toUpper)
            {
                chars[0] = char.ToUpper(chars[0]);
            }
            else
            {
                chars[0] = char.ToLower(chars[0]);
            }

            return new string(chars);
        }

        private void JsonToCode()
        {
            hastime = false;

            string json = text?.Trim() ?? "";

            if (string.IsNullOrEmpty(json))
            {
                IsInvalid = true;
                code = null;

                jsonComments.Clear();
                OnPropertyChanged(nameof(OutputCode));

                return;
            }

            FindJsonComments(json);
            JToken jobj = null;

            try
            {
                jobj = JsonConvert.DeserializeObject<JToken>(json);
            }
            catch
            {
                IsInvalid = true;
                code = null;

                jsonComments.Clear();
                OnPropertyChanged(nameof(OutputCode));

                return;
            }

            List<ParsedClass> classes;

            var clsName = ClassName;
            bool merge = false;

            if (jobj is JArray jarr)
            {
                merge = true;

                int z = 1;

                classes = new List<ParsedClass>();

                foreach (JToken jt in jarr)
                {
                    classes.AddRange(ParseObject(jt, clsName + (++z).ToString(), MVVMSetProperty));
                }
            }
            else
            {
                classes = ParseObject(jobj, clsName, MVVMSetProperty);
            }

            var sb = new StringBuilder();

            var dsl = new List<DataSet<ParsedProperty>>();

            foreach (var cls in classes)
            {
                var ds = new DataSet<ParsedProperty>();

                foreach (var prop in cls.Properties)
                {
                    ds.Add(prop);
                }

                dsl.Add(ds);
            }

            if (merge)
            {
                // Use my handy data set tool
                DataSet<ParsedProperty> merged = dsl.First();

                // Get all properties in the parsed classes
                foreach (var ds in dsl)
                {
                    merged |= ds;
                }

                // Now, get the largest common subset.
                DataSet<ParsedProperty> common = merged;

                foreach (var ds in dsl)
                {
                    var anded = merged & ds;

                    if (common == null || common.Count < anded.Count)
                    {
                        common = anded;
                    }
                }

                var cls = new ParsedClass()
                {
                    Name = clsName,
                    Properties = merged,
                    PropertyChanged = MVVMSetProperty
                };

                sb.Append(PrintClass(cls));
                sb.AppendLine("");
            }
            else
            {
                foreach (var cls in classes)
                {
                    sb.Append(PrintClass(cls));
                    sb.AppendLine("");
                }
            }

            code = sb.ToString();

            OnPropertyChanged(nameof(OutputCode));
            IsInvalid = false;
        }

        private List<ParsedClass> ParseObject(JToken obj, string clsName, bool propChange)
        {
            List<ParsedClass> classes = new List<ParsedClass>();

            if (obj.Type == JTokenType.Object)
            {
                var pc = new ParsedClass()
                {
                    Name = clsName,
                    PropertyChanged = propChange
                };

                foreach (JProperty jp in obj)
                {
                    var newProp = ParseProperty(jp, propChange);
                    pc.Properties.Add(newProp);
                    if (newProp.PropertyClasses != null && newProp.PropertyClasses.Count > 0)
                    {
                        classes.AddRange(newProp.PropertyClasses);
                    }
                }

                classes.Insert(0, pc);
            }

            return classes;
        }

        private ParsedProperty ParseProperty(JProperty jp, bool propChange)
        {
            int arrdepth = 0;
            JToken val = jp.Value;
            List<ParsedClass> classes = null;

            if (val.Type == JTokenType.Array)
            {
                JToken jarr = val;

                while (jarr.Type == JTokenType.Array)
                {
                    try
                    {
                        jarr = (jarr as JArray)[0];
                        arrdepth++;
                    }
                    catch
                    {
                        break;
                    }
                }

                val = jarr;
            }

            string type;

            var propName = FirstToUL(jp.Name, true);
            var varName = FirstToUL(propName, false);

            string dgetter = null;
            string access = "public";
            string dprop = null;

            if (val.Type == JTokenType.Object)
            {
                classes = new List<ParsedClass>();
                classes.AddRange(ParseObject(val, propName + "Class", propChange));
                type = propName + "Class";
            }
            else
            {
                type = PrintType(val);

                if (type.StartsWith("dt") && varName != "sequence")
                {
                    if (propChange)
                    {
                        dprop = varName;
                    }
                    else
                    {
                        dprop = "Internal" + propName;
                    }

                    if (type == "dtmilli")
                    {
                        dgetter = $"TimeTypes.InMilliseconds";
                    }
                    else if (type == "dtsecs")
                    {
                        dgetter = $"TimeTypes.InSeconds";
                    }
                    else
                    {
                        dgetter = $"TimeTypes.InNanoseconds";
                    }

                    access = "public";

                    dprop = propName;
                    type = "DateTime";
                }

                if (varName == "sequence")
                {
                    type = "long";
                    dprop = null;
                }
            }

            if (arrdepth > 0)
            {
                for (int i = 0; i < arrdepth; i++)
                {
                    type = "List<" + type + ">";
                }
            }

            return new ParsedProperty()
            {
                Access = access,
                IsDate = dprop != null,
                Type = type,
                DateProperty = dprop,
                DateGetter = dgetter,
                PropertyChanged = propChange,
                PropertyName = propName,
                VariableName = varName,
                PropertyClasses = classes
            };
        }

        private string PrintClass(ParsedClass cls)
        {
            var sb = new StringBuilder();
            var ns = "";

            if (!string.IsNullOrEmpty(Namespace))
            {
                sb.AppendLine("namespace " + Namespace);
                sb.AppendLine("{");
                sb.AppendLine();
                ns = "    ";
            }

            sb.AppendLine(ns + "public class " + cls.Name);
            sb.AppendLine(ns + "{");

            foreach (var pp in cls.Properties)
            {
                sb.AppendLine("");
                sb.Append(PrintProperty(pp, ns));
            }

            sb.AppendLine(ns + "");
            sb.AppendLine(ns + "}");

            if (hastime && GenerateTimeConverter)
            {
                sb.AppendLine();

                sb.Append(AppResources.TimeSource);

                sb.AppendLine();
            }

            if (!string.IsNullOrEmpty(Namespace))
            {
                sb.AppendLine();
                sb.AppendLine("}");
            }

            return sb.ToString();
        }

        private string PrintProperty(ParsedProperty prop, string ns = "")
        {
            var sb = new StringBuilder();

            if (prop.PropertyChanged)
            {
                sb.AppendLine(ns + $"");
                sb.AppendLine(ns + $"    private {prop.Type} {prop.VariableName};");
            }

            var txt = prop.PropertyName;

            if (ipidx < jsonComments.Count) txt = jsonComments[ipidx++];

            if (generateDocStrings)
            {
                sb.AppendLine(ns + $"");
                sb.AppendLine(ns + $"    /// <summary>");
                sb.AppendLine(ns + $"    /// {txt}");
                sb.AppendLine(ns + $"    /// </summary>");
                sb.AppendLine(ns + $"    [JsonProperty(\"{prop.VariableName}\")]");
            }

            if (prop.DateProperty != null)
            {
                sb.AppendLine(ns + $"    [JsonConverter(typeof(AutoTimeConverter), {prop.DateGetter})]");
            }

            if (!prop.PropertyChanged)
            {
                sb.AppendLine(ns + $"    {prop.Access} {prop.Type} {prop.PropertyName} {{ get; set; }}");
            }
            else
            {
                sb.AppendLine(ns + $"    {prop.Access} {prop.Type} {prop.PropertyName}");
                sb.AppendLine(ns + "    {");

                sb.AppendLine(ns + $"        get => {prop.VariableName};");
                sb.AppendLine(ns + $"        set");
                sb.AppendLine(ns + $"        {{");
                sb.AppendLine(ns + $"            SetProperty(ref {prop.VariableName}, value);");
                sb.AppendLine(ns + $"        }}");
                sb.AppendLine(ns + "    }");
            }

            return sb.ToString();
        }

        private string PrintType(JToken jp)
        {
            long l;
            decimal de;
            double db;
            bool tt = false;

            switch (jp.Type)
            {
                case JTokenType.String:

                    if (long.TryParse(jp.ToString(), out l))
                    {
                        if (!GenerateTimeConverter || l == 0)
                        {
                            return IntNumberType == IntType.Int ? "int" : "long";
                        }

                        tt = true;
                        break;
                    }
                    if (FloatNumberType == FPType.Double && double.TryParse(jp.ToString(), out db))
                    {
                        return "double";
                    }
                    else if (FloatNumberType == FPType.Decimal && decimal.TryParse(jp.ToString(), out de))
                    {
                        return "decimal";
                    }

                    return "string";

                case JTokenType.Date:
                    return "DateTime";

                case JTokenType.Boolean:
                    return "bool";

                case JTokenType.Integer:

                    if (long.TryParse(jp.ToString(), out l))
                    {
                        if (GenerateTimeConverter)
                        {
                            tt = true;
                            break;
                        }
                    }

                    if (!int.TryParse(jp.ToString(), out _)) return "long";

                    return IntNumberType == IntType.Int ? "int" : "long";

                case JTokenType.Float:

                    if (long.TryParse(jp.ToString(), out l))
                    {
                        if (GenerateTimeConverter)
                        {
                            tt = true;
                            break;
                        }
                    }

                    return FloatNumberType == FPType.Decimal ? "decimal" : "double";

                case JTokenType.Guid:
                    return "Guid";

                default:
                    return "object";
            }

            if (tt)
            {
                DateTime t = DateTime.Parse("1970-01-01 00:00:00").ToUniversalTime();
                DateTime t2 = t;

                try
                {
                    t2 = t.AddMilliseconds(l);
                }
                catch
                {
                    t2 = t;
                }
                var now = DateTime.Now;

                if (now.Year - t2.Year < 5)
                {
                    hastime = true;
                    return "dtmilli";
                }

                try
                {
                    t2 = t.AddSeconds(l);
                }
                catch
                {
                    t2 = t;
                }

                if (now.Year - t2.Year < 5)
                {
                    hastime = true;
                    return "dtsecs";
                }

                try
                {
                    t2 = t.AddTicks(l / 100);
                }
                catch
                {
                    if (IndeterminateType == IndeterminateType.Float)
                    {
                        return FloatNumberType == FPType.Decimal ? "decimal" : "double";
                    }
                    else
                    {
                        return IntNumberType == IntType.Int ? "int" : "long";
                    }
                }

                if (now.Year - t2.Year < 5)
                {
                    hastime = true;
                    return "dtnano";
                }
                else
                {
                    if (IndeterminateType == IndeterminateType.Float)
                    {
                        return FloatNumberType == FPType.Decimal ? "decimal" : "double";
                    }
                    else
                    {
                        return IntNumberType == IntType.Int ? "int" : "long";
                    }
                }
            }
            else
            {
                return "object";
            }
        }

        private class ParsedClass
        {
            public ParsedClass(string name)
            {
                Name = name;
            }

            public ParsedClass()
            {
            }

            public string Name { get; set; }

            public List<ParsedProperty> Properties { get; set; } = new List<ParsedProperty>();
            public bool PropertyChanged { get; set; }
        }

        private class ParsedProperty
        {
            public string Access { get; set; }
            public string DateGetter { get; set; }
            public string DateProperty { get; set; }
            public bool IsDate { get; set; }
            public bool PropertyChanged { get; set; }
            public List<ParsedClass> PropertyClasses { get; set; }
            public string PropertyName { get; set; }
            public string Type { get; set; }
            public string VariableName { get; set; }

            public static bool operator !=(ParsedProperty val1, ParsedProperty val2)
            {
                if (val1 is object)
                {
                    return val1.Equals(val2);
                }
                else if (val2 is object)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public static bool operator ==(ParsedProperty val1, ParsedProperty val2)
            {
                if (val1 is object)
                {
                    return val1.Equals(val2);
                }
                else if (val2 is object)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }

            public override bool Equals(object obj)
            {
                if (obj is ParsedProperty other)
                {
                    return other.PropertyName == PropertyName && other.Type == Type;
                }
                else
                {
                    return false;
                }
            }

            public override int GetHashCode()
            {
                return (int)Streams.Crc32.Hash(Encoding.UTF8.GetBytes(PropertyName + Type));
            }

            public override string ToString()
            {
                return Type + " " + PropertyName;
            }
        }
    }
}