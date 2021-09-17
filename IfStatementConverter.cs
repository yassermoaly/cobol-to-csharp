using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CobolToCSharp
{
    class IfStatementConverter : IStatementConverter
    {
        public List<StatementType> StatementTypes => new List<StatementType>(new StatementType[] { StatementType.IF, StatementType.ELSE_IF });
        private readonly Regex CobolVariable = new Regex("[a-zA-Z][a-zA-Z0-9]*[-][a-zA-Z0-9-]*");
        private string[] SpecialSplit(string Line,string[] Separators)
        {
            List<string> result = new List<string>();
            StringBuilder SB = new StringBuilder();
            foreach (char c in Line.ToCharArray())
            {
                SB.Append(c);
                if (Separators.Contains(SB.ToString()))
                {
                    result.Add(SB.ToString());
                    SB = new StringBuilder();
                }
            }
            result.Add(SB.ToString());
            return result.ToArray();
        }

        private string GetDatatype(string Token, Dictionary<string, string> CobolVariablesDataTypes = null)
        {
            if (Token.Equals("SPACES")) return "string";
            else if (Token.Equals("ZEROS") || Token.Equals("ZERO")) return "long";
            else if (new Regex("([Xx]*'.+')|([Xx]*\".+\")").IsMatch(Token))
                return "string";
            else if (new Regex("([0-9]+(\\.[0-9]+)*)").IsMatch(Token))
                return "double";
            else if (new Regex("([0-9]+)").IsMatch(Token))
                return "long";
            if (CobolVariablesDataTypes.ContainsKey(Token.Replace("_","-")))
                return CobolVariablesDataTypes[Token.Replace("_", "-")];
            return "undefined";

            //(([Xx]*'.+')|([Xx]*\".+\")|([0-9]+(\.[0-9]+)*)|([a-zA-Z-0-9]+))+[ ]*=[ ]*=[ ]*(([Xx]*'.+')|([Xx]*\".+\")|([0-9]+(\.[0-9]+)*)|([a-zA-Z-0-9]+))

        }
        public string Convert(string Line, Paragraph Paragraph, List<Paragraph> Paragraphs, Dictionary<string, string> CobolVariablesDataTypes = null)
        {
            //string[] Separators = new string[] { " "," OR ", " AND ", "=", "(", ")", "IF", "ELSE IF" };
            Line = Line.RegexReplace("IF", "if(").RegexReplace("ELSE", "else").Replace("=", "==").RegexReplace("AND", "&&").RegexReplace("OR", "||");
            foreach (var item in CobolVariable.Matches(Line))
            {
                Line = Line.Replace(item.ToString(), NamingConverter.Convert(item.ToString()));
            }
            foreach (var item in new Regex($"{"NOT".RegexUpperLower()}[ ]*=[ ]*=").Matches(Line))
            {
                Line = Line.Replace(item.ToString(), "!=");
            }
            foreach (var item in new Regex(">[ ]*=[ ]*=").Matches(Line))
            {
                Line = Line.Replace(item.ToString(), ">=");
            }
            foreach (var item in new Regex("<[ ]*=[ ]*=").Matches(Line))
            {
                Line = Line.Replace(item.ToString(), "<=");
            }
            int ExtraCounts = 0;
            foreach (Match item in new Regex(@"(\|\|[ ]*=[ ]*=|\|\|[ ]*![ ]*=|\|\|[ ]*>[ ]*=|\|\|[ ]*<[ ]*=|\|\|[ ]*<|\|\|[ ]*>|&&[ ]*=[ ]*=|&&[ ]*![ ]*=|&&[ ]*>[ ]*=|&&[ ]*<[ ]*=|&&[ ]*<|&&[ ]*>)").Matches(Line))
            {
                Regex VarRegex = new Regex("[a-zA-Z]+[a-zA-Z0-9_]*[ ]*(>[ ]*=|<[ ]*=|=[ ]*=|![ ]*=|<|>)");
                var index = item.Index + ExtraCounts;
                var VarName  = new Regex(">[ ]*=|<[ ]*=|=[ ]*=|![ ]*=|<|>").Replace(VarRegex.Matches(Line.Substring(0, index)).Last().Value,string.Empty).Trim();                
              
                Line = $"{Line.Substring(0, index)}{Line.Substring(index, 2)} {VarName}{Line.Substring(index + 2)}";
                ExtraCounts += VarName.Length + 1;
            }
            ExtraCounts = 0;
            foreach (Match item in new Regex(@"\|\|[ ]*(\+|\-)*[0-9]+").Matches(Line))
            {
                Regex VarRegex = new Regex("[a-zA-Z]+[a-zA-Z0-9_]*[ ]*(>[ ]*=|<[ ]*=|=[ ]*=|![ ]*=|<|>)");
                var index = item.Index + ExtraCounts;
                var VarNameAndOperator = VarRegex.Matches(Line.Substring(0, index)).Last().Value;
                Line = $"{Line.Substring(0, index)}{Line.Substring(index, 2)} {VarNameAndOperator}{Line.Substring(index + 2)}";
                ExtraCounts += VarNameAndOperator.Length + 1;

            }
            if (Line.Contains("IBUSERID_1W"))
            {
                int x123 = 100;
            }
            Dictionary<string, string> Replacements = new Dictionary<string, string>();
            foreach (Match Match in new Regex("(([Xx]*'.+')|([Xx]*\".+\")|([0-9]+(\\.[0-9]+)*)|([a-zA-Z_0-9]+))+[ ]*=[ ]*=[ ]*(([Xx]*'.+')|([Xx]*\".+\")|([0-9]+(\\.[0-9]+)*)|([a-zA-Z_0-9]+))").Matches(Line))
            {
                if (Replacements.ContainsKey(Match.Value))
                    continue;
                string[] Tokens = Match.Value.RegexReplace("=", " ").Split(' ', StringSplitOptions.RemoveEmptyEntries);

                string LeftHandType = GetDatatype(Tokens[0], CobolVariablesDataTypes);
                string RightHandType = GetDatatype(Tokens[1], CobolVariablesDataTypes);


                if (LeftHandType != RightHandType)
                {
                    if (LeftHandType != "undefined" && RightHandType != "undefined")
                    {
                        string LeftHand = Tokens[0];
                        string RightHand = Tokens[1];
                        if (LeftHandType == "string")
                            LeftHand = LeftHand.RegexReplace("X", string.Empty).Replace("'", "\"");
                        else
                            LeftHand = $"Convert.ToString({LeftHand})";
                        if (RightHandType == "string")
                            RightHand = RightHand.RegexReplace("X", string.Empty).Replace("'", "\"");
                        else
                            RightHand = $"Convert.ToString({RightHand})";
                        Replacements.Add(Match.Value, $"{LeftHand}=={RightHand}");
                    }
                }
            }
            //foreach (KeyValuePair<string,string> Replacement in Replacements)
            //{
            //    Line = Line.Replace(Replacement.Key, Replacement.Value);
            //}


            return $"{Line})";
        }
    }
}
