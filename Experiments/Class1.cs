using System;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Experiments
{
    internal class Class1
    {
        public (int, int)? Run(int arg1, char ch, string bbvvv)
        {
            (int a, int b) = (arg1 ^ -1 >> (arg1 & 0xf), arg1 ^ -1 >> (arg1 & 0xf));

            return (a, b);
        }

        public bool Retest => false;

        public event EventHandler<MouseEventArgs> WireMeMatey
        {
            add { WireMeMatey += value; }
            remove { WireMeMatey -= value; }
        }
    }
}