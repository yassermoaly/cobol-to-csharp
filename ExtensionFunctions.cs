using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobolToCSharp
{
    public static class ExtensionFunctions
    {
        public static string PostionReplace(this string s, int index, int Length, string NewValue)
        {
            var aStringBuilder = new StringBuilder(s);
            aStringBuilder.Remove(index, Length);
            aStringBuilder.Insert(index, NewValue);
            return aStringBuilder.ToString();
        }
    }
}
