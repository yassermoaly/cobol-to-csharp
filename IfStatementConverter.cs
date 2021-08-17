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
        private readonly Regex CobolVariable = new Regex("[a-zA-Z]+[-][a-zA-Z0-9-]*");
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
        public string Convert(string Line, Paragraph Paragraph, List<Paragraph> Paragraphs)
        {
            //string[] Separators = new string[] { " "," OR ", " AND ", "=", "(", ")", "IF", "ELSE IF" };
            Line = Line.Replace("IF", "if(").Replace("ELSE IF", "else if(").Replace("=", "==").Replace("AND", "&&").Replace("OR", "||");
            foreach (var item in CobolVariable.Matches(Line))
            {
                Line = Line.Replace(item.ToString(), NamingConverter.Convert(item.ToString()));
            }
            foreach (var item in new Regex("NOT[ ]*=[ ]*=").Matches(Line))
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



            return $"{Line})";
        }
    }
}
