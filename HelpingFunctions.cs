using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CobolToCSharp
{
    public static class HelpingFunctions
    {
        public static string GetDatatypeConvertFunction(string Token, Dictionary<string, string> CobolVariablesDataTypes = null)
        {
            switch (GetDatatype(Token,CobolVariablesDataTypes))
            {
                case "string":
                    return "ToString";
                case "long":
                    return "ToInt64";
                case "double":
                    return "ToDouble";
                default:
                    throw new Exception("Unhandeled DataType");
            }
        }
        public static string GetDatatype(string Token, Dictionary<string, string> CobolVariablesDataTypes = null)
        {
            if (CobolVariablesDataTypes.ContainsKey(Token.Replace("_", "-")))
            {
                switch (CobolVariablesDataTypes[Token.Replace("_", "-")])
                {
                    case "long":
                        return "long";
                    case "double":
                        return "double";
                    default:
                        return "string";
                }
            }


            else if (Token.Equals("SPACES")) return "string";
            else if (Token.Equals("ZEROS") || Token.Equals("ZERO")) return "double";
            else if (Token.Equals("APPL-EXTENDED-STATUS") || Token.Equals("APPL-STATUS")) return "long";
            else if (new Regex("([Xx]*'.+')|([Xx]*\".+\")").IsMatch(Token))
                return "string";
            else if (new Regex("([0-9]+(\\.[0-9]+)*)").IsMatch(Token))
            {
                if (Token.Contains("."))
                    return "double";
                return "long";
            }
            return "undefined";
        }
    }
}
