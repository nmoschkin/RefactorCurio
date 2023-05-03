using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTools.Code.Project
{
    /// <summary>
    /// <see cref="ElementToken.WhichIsIn(ElementToken, ElementToken)"/> result value
    /// </summary>
    public enum WhichIn
    {
        /// <summary>
        /// Neither namespace is contained in the other
        /// </summary>
        Neither,

        /// <summary>
        /// Namespace A is contained in namespace B
        /// </summary>
        AInB,

        /// <summary>
        /// Namespace B is contained in namespace A
        /// </summary>
        BInA,

        /// <summary>
        /// Namespace A and namespace B are the same namespace
        /// </summary>
        Identical

    }

    /// <summary>
    /// Represents a fully-qualified element name token in the code tree
    /// </summary>
    public class ElementToken : 
        IEquatable<ElementToken>, 
        IEquatable<string>, 
        IComparable<ElementToken>, 
        IComparable<string>
    {
        /// <summary>
        /// Empty Element Token
        /// </summary>
        public static readonly ElementToken Empty = new ElementToken("");

        private static readonly object lockObj = new object();
        private static readonly SortedDictionary<string, ElementToken> namespaces = new SortedDictionary<string, ElementToken>();
        
        
        /// <summary>
        /// The constituent parts of the name
        /// </summary>
        /// <remarks>
        /// This field stores the official value of this object
        /// </remarks>
        protected string[] values = new string[0];

        static ElementToken()
        {
            namespaces.Add("", Empty);
        }

        private ElementToken(string value)
        {
            SetTokenValue(value);
        }

        /// <summary>
        /// Returns the full value of the token
        /// </summary>
        public string FullName
        {
            get => ToString();
        }

        /// <summary>
        /// Returns the identifier string
        /// </summary>
        public string Identifier
        {
            get => values.LastOrDefault() ?? "";
        }

        /// <summary>
        /// Returns true if this is value is empty
        /// </summary>
        public bool IsEmpty => values.Length == 0;

        /// <summary>
        /// Returns true if the name consists of only an identifier with no namespace path
        /// </summary>
        public bool IsNameOnly => values.Length == 1;

        /// <summary>
        /// Returns the namespace string
        /// </summary>
        public string Namespace
        {
            get => values.Length > 1 ? ToString(-1) : ToString();
        }

        /// <summary>
        /// The namespace cache dictionary
        /// </summary>
        public SortedDictionary<string, ElementToken> Namespaces => namespaces;

        /// <summary>
        /// Returns an array of the constituent elements
        /// </summary>
        public string[] Values
        {
            get => values.ToArray();
        }

        public static implicit operator ElementToken(string value)
        {
            return RetrieveOrCreateToken(value);
        }

        public static implicit operator string(ElementToken value)
        {
            return value.ToString();
        }

        public static bool operator !=(ElementToken left, ElementToken right)
        {
            if (left is null && right is null)
            {
                return false;
            }
            if (left is null || right is null)
            {
                return true;
            }

            return !left.Equals(right);
        }

        public static bool operator ==(ElementToken left, ElementToken right)
        {
            if (left is null && right is null)
            {
                return true;
            }
            if (left is null || right is null)
            {
                return false;
            }

            return left.Equals(right);
        }

        /// <summary>
        /// Retrieve or create a token.
        /// </summary>
        /// <param name="name">The name of the identifier or path</param>
        /// <typeparam name="T">The type based on <see cref="ElementToken"/> to create</typeparam>
        /// <returns>The requested element</returns>
        /// <remarks>
        /// There is only ever one instance of a <see cref="ElementToken"/> or derived class with any given value in the current runtime environment.
        /// <br /><br />
        /// If a name is requested that has not been created yet, an object of the specified type for this call <br/>
        /// will become the default value for the specified name until it is removed from the cache.<br /><br />
        /// If the element already exists and is the same type, the cached element is returned, otherwise a new element is returned.
        /// </remarks>
        public static T RetrieveOrCreateToken<T>(string name) where T: ElementToken, new()
        {
            lock (lockObj)
            {
                if (!namespaces.TryGetValue(name, out var token) || !(token is T ret))
                {                    
                    ret = new T();
                    ret.SetTokenValue(name);

                    if (token == null)
                    {
                        namespaces.Add(name, ret);
                    }
                }

                return ret;
            }
        }

        /// <summary>
        /// Retrieve or create a token.
        /// </summary>
        /// <param name="name">The name of the identifier or path</param>
        /// <returns></returns>
        /// <remarks>
        /// There is only ever one instance of <see cref="ElementToken"/> with any given value in the current runtime environment.
        /// </remarks>
        public static ElementToken RetrieveOrCreateToken(string name)
        {
            lock (lockObj)
            {
                if (!namespaces.TryGetValue(name, out var token) || !(token is ElementToken ret))
                {
                    ret = new ElementToken(name);

                    if (token == null)
                    {
                        namespaces.Add(name, ret);
                    }
                }

                return ret;
            }
        }

        /// <summary>
        /// Determine if either of the specified namespaces are contained in the other
        /// </summary>
        /// <param name="A">First namespace</param>
        /// <param name="B">Second namespace</param>
        /// <returns><see cref="WhichIn"/> enumeration.</returns>
        public static WhichIn WhichIsIn(ElementToken A, ElementToken B)
        {
            var c = A.values.Length;
            var d = B.values.Length;

            if (c < d)
            {
                for (var i = 0; i < c; i++)
                {
                    var s1 = A.values[i];
                    var s2 = B.values[i];

                    if (s1 != s2) return WhichIn.Neither;
                }

                return WhichIn.AInB;
            }
            else if (c > d)
            {
                for (var i = 0; i < c; i++)
                {
                    var s1 = A.values[i];
                    var s2 = B.values[i];

                    if (s1 != s2) return WhichIn.Neither;
                }

                return WhichIn.BInA;
            }
            else
            {
                for (var i = 0; i < c; i++)
                {
                    var s1 = A.values[i];
                    var s2 = B.values[i];

                    if (s1 != s2) return WhichIn.Neither;
                }

                return WhichIn.Identical;
            }
        }

        public int CompareTo(ElementToken other)
        {
            return string.Compare(ToString(), other.ToString());
        }

        public int CompareTo(string other)
        {
            return string.Compare(ToString(), other.ToString());
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            if (obj is ElementToken ns)
            {
                return Equals(ns);
            }
            else if (obj is string s)
            {
                return Equals(s);
            }
            else
            {
                return false;
            }
        }

        public bool Equals(ElementToken other)
        {
            if (other == null) return false;

            if (other.values.Length != values.Length) return false;
            var c = values.Length;
            for (var i = 0; i < c; i++)
            {
                if (other.values[i] != values[i]) return false;
            }

            return true;
        }

        public bool Equals(string other)
        {
            return ToString().Equals(other);
        }

        /// <summary>
        /// Fin all known descendants
        /// </summary>
        /// <returns></returns>
        public IList<ElementToken> FindKnownDescendants()
        {
            var l = new List<ElementToken>();

            var values = namespaces.Values.ToArray();

            foreach (var val in values)
            {
                if (val.IsDescendant(this))
                {
                    l.Add(val);
                }
            }

            return l;
        }

        /// <summary>
        /// Find all known direct children
        /// </summary>
        /// <returns></returns>
        public IList<ElementToken> FindKnownChildren()
        {
            var l = new List<ElementToken>();

            var values = namespaces.Values.ToArray();

            foreach (var val in values)
            {
                if (val.IsDescendant(this))
                {
                    l.Add(val);
                }
            }

            return l;
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        /// <summary>
        /// Get the parent namespace token
        /// </summary>
        /// <returns></returns>
        public ElementToken GetParent()
        {
            return RetrieveOrCreateToken(Namespace);
        }

        /// <summary>
        /// Returns true if the current instance is a child of the specified token
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public bool IsDescendant(ElementToken token)
        {
            var wi = WhichIsIn(this, token);
            return wi == WhichIn.AInB;
        }

        public bool IsDirectChild(ElementToken token)
        {
            return Namespace == token.FullName;
        }

        /// <summary>
        /// Returns true if the current instance is the parent of the specified token
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public bool IsParent(ElementToken token)
        {
            var wi = WhichIsIn(this, token);
            return wi == WhichIn.BInA;
        }

        /// <summary>
        /// Join this element to another
        /// </summary>
        /// <param name="next"></param>
        /// <returns></returns>
        public ElementToken Join(ElementToken next)
        {
            var s = string.Join(".", ToString(), next.ToString());
            return RetrieveOrCreateToken(s);
        }
        /// <summary>
        /// Returns the entire namespace
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Join(".", values);
        }

        /// <summary>
        /// Returns the namespace to the specified depth (beginning with 1)
        /// </summary>
        /// <param name="depth">The depth in stages from the beginning if positive, or stages from the end if negative.</param>
        /// <returns></returns>
        public string ToString(int depth)
        {
            var sb = new StringBuilder();
            var c = values.Length;
            
            if (depth < 0) depth = c + depth;

            for (var i = 0; i < depth && i < c; i++)
            {
                if (i > 0) sb.Append('.');
                sb.Append(values[i]);
            }

            return sb.ToString();
        }        

        /// <summary>
        /// Set the namespace to the specified value
        /// </summary>
        /// <param name="value"></param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="FormatException"></exception>
        protected virtual void SetTokenValue(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                values = new string[0];
                return;
            }

            if (char.IsDigit(value[0])) throw new ArgumentException("Invalid namespace");

            foreach (var ch in value)
            {
                if ("_@$.".Contains(ch)) continue;
                if (char.IsLetterOrDigit(ch)) continue;

                throw new ArgumentException("Invalid namespace");
            }

            var vl = value.Split('.').ToList();
            var c = vl.Count;
            for (var i = c - 1; i >= 0; i--)
            {
                vl[i] = vl[i].Trim();
                
                if (string.IsNullOrEmpty(vl[i]))
                {
                    if (i != c - 1 && i != 0) throw new FormatException("Two '.' delimiters in a row is not allowed");
                    vl.RemoveAt(i);
                }
            }

            values = vl.ToArray();
        }
    }
}
