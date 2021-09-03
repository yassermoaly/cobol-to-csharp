using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CobolToCSharp
{
    public static class NamingConverter
    {
        static Regex CobolVariablesRegex = new Regex("^[a-zA-Z][a-zA-Z0-9-]+");
        public static string Convert(string Name)
        {
            if(CobolVariablesRegex.IsMatch(Name))
                return Name.Replace("-", "_").Replace(".",string.Empty);

            return Name;
        }
    }
}
