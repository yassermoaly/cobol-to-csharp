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

        public static string FirstCharToUpper(this string input) => input switch
        {
            null => throw new ArgumentNullException(nameof(input)),
            "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
            _ => string.Concat(input[0].ToString().ToUpper(), input.AsSpan(1))
        };
    }
}
