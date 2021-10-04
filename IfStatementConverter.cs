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
            else if (Token.Equals("ZEROS") || Token.Equals("ZERO")) return "numeric";
            else if (new Regex("([Xx]*'.+')|([Xx]*\".+\")").IsMatch(Token))
                return "string";
            else if (new Regex("([0-9]+(\\.[0-9]+)*)").IsMatch(Token))
                return "numeric";           
            if (CobolVariablesDataTypes.ContainsKey(Token.Replace("_","-")))
            {
                switch (CobolVariablesDataTypes[Token.Replace("_", "-")])
                {
                    case "long":
                    case "double":
                        return "numeric";
                    default:
                        return "string";
                }
            }
                
            return "undefined";

            //(([Xx]*'.+')|([Xx]*\".+\")|([0-9]+(\.[0-9]+)*)|([a-zA-Z-0-9]+))+[ ]*=[ ]*=[ ]*(([Xx]*'.+')|([Xx]*\".+\")|([0-9]+(\.[0-9]+)*)|([a-zA-Z-0-9]+))

        }
        public string Convert(string Line, Paragraph Paragraph, List<Paragraph> Paragraphs, Dictionary<string, string> CobolVariablesDataTypes = null)
        {
            //string[] Separators = new string[] { " "," OR ", " AND ", "=", "(", ")", "IF", "ELSE IF" };
            Line = Line.RegexReplace("IF(?![A-Z])", "IF", "if(").RegexReplace("ELSE(?![A-Z])", "ELSE", "else").Replace("=", "==").RegexReplace("[^a-zA-Z]AND(?![A-Z])", "AND", "&&").RegexReplace("[^a-zA-Z]OR(?![A-Z])","OR", "||");
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
            int ExtraAddedCharacters = 0;
            if (Line.Contains("if( SV_VLI       == 500      &&"))
            {
                int x = 10;
            }
            foreach (Match Match in new Regex("(([Xx]*'[^']+')|([Xx]*\"[^\"]\")|([0-9]+(\\.[0-9]+)*)|([a-zA-Z_0-9]+))+[ ]*(=[ ]*= | ![ ]*=)[ ]*(([Xx]*'[^']+')|([Xx]*\"([^\"]+)\")|([0-9]+(\\.[0-9]+)*)|([a-zA-Z_0-9]+))").Matches(Line))
            {
                

                string[] Tokens = Match.Value.Split(new char[] { '=','!'}, StringSplitOptions.RemoveEmptyEntries).Select(r=>r.Trim()).ToArray();
                if (Tokens.Length != 2)
                {
                    throw new Exception("Invalid If Regex");
                }
                string LeftHandType = GetDatatype(Tokens[0], CobolVariablesDataTypes);
                string RightHandType = GetDatatype(Tokens[1], CobolVariablesDataTypes);
                
                string LeftHand = Tokens[0].Trim();
                string RightHand = Tokens[1].Trim();

                bool ApplyChange = false;
                if (LeftHand.StartsWith("X\""))
                {
                    LeftHand = $"{LeftHand.Substring(1)}.GetStringValueFromHexa()";
                    ApplyChange = true;
                }
                if (RightHand.StartsWith("X\""))
                {
                    RightHand = $"{RightHand.Substring(1)}.GetStringValueFromHexa()";
                    ApplyChange = true;
                }
                if(LeftHandType != RightHandType)
                {
                    ApplyChange = true;
                }


                if (ApplyChange)
                {
                    if (LeftHandType != "undefined" && RightHandType != "undefined")
                    {
    
                        if (LeftHandType != "string")
                            LeftHand = $"Convert.ToString({LeftHand})";
                        if (RightHandType != "string")
                            RightHand = $"Convert.ToString({RightHand})";

                        //if (RightHandType == "string")
                        //{

                        //    RightHand = RightHand.Trim();
                        //    if (RightHand.ToUpper().StartsWith("X"))
                        //    {
                        //        RightHand = $"{RightHand.Substring(1)}.GetStringValueFromHexa()";
                        //    }
                        //    else
                        //    {
                        //        RightHand = RightHand.Replace("'", "\"");
                        //    }

                        //}
                        //else
                        //    RightHand = $"Convert.ToString({RightHand})";

                        string Operator = string.Empty;
                        if (new Regex("=[ ]*=").IsMatch(Match.Value))
                            Operator = "==";
                        else if (new Regex("![ ]*=").IsMatch(Match.Value))
                            Operator = "!=";
                        else
                            throw new Exception("Invalid operator type");

                        string NewValue = $"{LeftHand} {Operator} {RightHand}";
                        Line = Line.PostionReplace(Match.Index+ ExtraAddedCharacters, Match.Length, NewValue);
                        ExtraAddedCharacters += NewValue.Length - Match.Length;
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
