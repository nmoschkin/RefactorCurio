using System;
using System.Collections;

/* Unmerged change from project 'DataTools.Code (net48)'
Before:
using System.Collections.Generic;

namespace DataTools.Code.Markers
After:
using System.Collections.Generic;
using DataTools;
using DataTools;
using DataTools.Code;
using DataTools.Code.Markers
*/
using System.Collections.Generic;

namespace DataTools.Code
{
    /// <summary>
    /// Keeps contents of a file in memory both as a complete original text string and also lines (virtually)
    /// </summary>
    /// <remarks>
    /// Only one copy of the string is kept in memory. The line positions and lengths are recorded.<br />
    /// This is more durable than just parsing lines, alone, as this can deal with mixed line-endings, <br />
    /// and the original text of the document remains unchanged.
    /// <br/><br/>
    /// The text is mutable, but the lines are calculated from the text content, <br/>
    /// and individual lines are not mutable.
    /// </remarks>
    public sealed class FileLines : IReadOnlyList<string>
    {
        private string text;
        private int[] linePos;
        private int[] lineLen;

        public int Count => linePos?.Length ?? 0;

        private void Parse(string text)
        {
            this.text = text;

            var chars = text.ToCharArray();

            int i, c = chars.Length;

            int j = 0;

            var pos = new List<int>();
            var len = new List<int>();

            pos.Add(0);

            for (i = 0; i < c; i++)
            {
                var ch = chars[i];

                if (ch == '\n')
                {
                    len.Add(j);
                    j = 0;
                    pos.Add(i + 1);
                }
                else
                {
                    j++;
                }
            }

            len.Add(j);

            linePos = pos.ToArray();
            lineLen = len.ToArray();
        }

        /// <summary>
        /// Gets or sets the text content of this object
        /// </summary>
        public string Text
        {
            get => text;
            set => Parse(text);
        }

        public string this[int i]
        {
            get
            {
                if (i < 0 || i >= lineLen.Length) throw new ArgumentOutOfRangeException();
                return text.Substring(linePos[i], lineLen[i]).Replace("\r", "").Replace("\n", "");
            }
        }

        /// <summary>
        /// Create a new <see cref="FileLines"/> object from the given text
        /// </summary>
        /// <param name="text"></param>
        public FileLines(string text)
        {
            Parse(text);
        }

        /// <summary>
        /// Create an empty <see cref="FileLines"/> object
        /// </summary>
        public FileLines() { }


        /// <summary>
        /// Implicitly cast a <see cref="FileLines"/> object to a string
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator string(FileLines value)
        {
            return value.text;
        }

        /// <summary>
        /// Implicitly cast a string to a <see cref="FileLines"/> object
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator FileLines(string value)
        {
            return new FileLines(value);
        }

        public override string ToString()
        {
            return text ?? base.ToString();
        }

        public IEnumerator<string> GetEnumerator()
        {
            int i, c = Count;

            for (i = 0; i < c; i++)
            {
                yield return this[i];
            }

            yield break;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

    }
}