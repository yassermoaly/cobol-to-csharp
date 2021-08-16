using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobolParser
{
    public static class NamingConverter
    {
        public static string Convert(string Name)
        {
            return Name.Replace("-", "_");
        }
    }
}
