using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace siteReader
{
    public static class ExtensionMethods
    {
        public static int Remap(this int value, int from1, int to1, int from2, int to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }
    }
}
