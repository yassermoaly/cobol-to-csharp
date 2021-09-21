using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CobolToCSharp
{
    public class PerformStatementConverter : IStatementConverter
    {
        public List<StatementType> StatementTypes => new List<StatementType>(new StatementType[] { StatementType.PERFORM });

        public string Convert(string Line, Paragraph Paragraph, List<Paragraph> Paragraphs, Dictionary<string,string> CobolVariablesDataTypes = null)
        {
            if(new Regex($"{"PERFORM".RegexUpperLower()}[ ]+[a-zA-Z0-9-]+[ ]+{"THRU".RegexUpperLower()}[ ]+[a-zA-Z0-9-]+").IsMatch(Line))
            {
                StringBuilder SB = new StringBuilder();
                Match TokensMatch = new Regex($"{"PERFORM".RegexUpperLower()}[ ]+[a-zA-Z0-9-]+[ ]+{"THRU".RegexUpperLower()}[ ]+[a-zA-Z0-9-]+").Match(Line);
                string[] Tokens = TokensMatch.Value.RegexReplace("PERFORM", string.Empty).RegexReplace("THRU", string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(r => r.Trim().Replace(".",string.Empty)).ToArray();
                
                SB.AppendLine($"#region Preform {Tokens[0]} THRU {Tokens[1]}");
                Paragraph StartParagraph = Paragraphs.First(r => r.Name.Replace(".", string.Empty).Equals(Tokens[0]));
                Paragraph EndParagraph = Paragraphs.First(r => r.Name.Replace(".", string.Empty).Equals(Tokens[1]));
                int StartParagraphIndex = Paragraphs.IndexOf(StartParagraph);
                int EndParagraphIndex = Paragraphs.IndexOf(EndParagraph);

                List<Paragraph> PerformParagraphs = Paragraphs.Skip(StartParagraphIndex).Take(EndParagraphIndex - StartParagraphIndex + 1).ToList();


                SB.AppendLine($"PerformScope = new string[] {{{string.Join(',', PerformParagraphs.Select(r => $"\"{NamingConverter.Convert(r.Name)}\"").ToArray())}}};");
                SB.AppendLine($"TempStack = null;");

                foreach (var PerformParagraph in PerformParagraphs)
                {
                    SB.AppendLine($"if (!TempStack.HasGOTOOrEnd())");
                    SB.AppendLine($"{{");
                    SB.AppendLine($"    TempStack = FullStack.AddRangeAndReturnNew({NamingConverter.Convert(PerformParagraph.Name)}(false, PerformScope));");
                    SB.AppendLine($"    if (TempStack.HasGOTOOrEnd() && TempStack.GOTOOutOfScope(PerformScope)){{");
                    SB.AppendLine($"        return FullStack;");
                    SB.AppendLine($"    }}");
                    SB.AppendLine($"}}");
                    //string ParagraphName = P.Name.Replace(".", string.Empty);
                    //if (!Append)
                    //{
                    //    if (ParagraphName.Equals(Tokens[0]))
                    //    {
                    //        Append = true;                            
                    //        //SB.AppendLine($"if(!{NamingConverter.Convert(ParagraphName)}(true)){{return false;}}");
                    //    }
                    //}
                    //if(Append)                    
                    //{
                    //    SB.AppendLine($"if(!{NamingConverter.Convert(ParagraphName)}(true)){{return false;}}");
                    //    if (ParagraphName.Equals(Tokens[1]))
                    //        break;
                    //}
                }
                SB.AppendLine($"#endregion");
                return SB.ToString();
            }
            else if(new Regex($"{"PERFORM".RegexUpperLower()}[ ]+[a-zA-Z0-9-]+[ ]+{"UNTIL".RegexUpperLower()}.+").IsMatch(Line))
            {
                StringBuilder SB = new StringBuilder();
                Match PERFORM_NAME_MATCH = new Regex($"{"PERFORM".RegexUpperLower()}[ ]+[a-zA-Z0-9-]+[ ]+{"UNTIL".RegexUpperLower()}").Match(Line);
                string PerformName = NamingConverter.Convert(PERFORM_NAME_MATCH.Value.RegexReplace("PERFORM", string.Empty).RegexReplace("UNTIL", string.Empty).Trim());
                string Condition = Line.Substring(PERFORM_NAME_MATCH.Length);
                if (Condition.EndsWith(".")) 
                    Condition = Condition.Substring(0, Condition.Length - 1);
                 Condition = Condition.Replace("=", "==").RegexReplace("AND", "&&").RegexReplace("OR", "||");
                 Regex CobolVariable = new Regex("[a-zA-Z]+[-][a-zA-Z0-9-]*");
                foreach (Match item in CobolVariable.Matches(Condition))
                {
                    Condition = Condition.Replace(item.ToString(), NamingConverter.Convert(item.ToString()));
                }
                foreach (var item in new Regex($"{"NOT".RegexUpperLower()}[ ]*=[ ]*=").Matches(Condition))
                {
                    Condition = Condition.Replace(item.ToString(), "!=");
                }
                foreach (var item in new Regex(">[ ]*=[ ]*=").Matches(Condition))
                {
                    Condition = Condition.Replace(item.ToString(), ">=");
                }
                foreach (var item in new Regex("<[ ]*=[ ]*=").Matches(Condition))
                {
                    Condition = Condition.Replace(item.ToString(), "<=");
                }
                int ExtraCounts = 0;
                foreach (Match item in new Regex(@"(\|\|[ ]*=[ ]*=|\|\|[ ]*![ ]*=|\|\|[ ]*>[ ]*=|\|\|[ ]*<[ ]*=|\|\|[ ]*<|\|\|[ ]*>|&&[ ]*=[ ]*=|&&[ ]*![ ]*=|&&[ ]*>[ ]*=|&&[ ]*<[ ]*=|&&[ ]*<|&&[ ]*>)").Matches(Condition))
                {
                    Regex VarRegex = new Regex("[a-zA-Z]+[a-zA-Z0-9_]*[ ]*(>[ ]*=|<[ ]*=|=[ ]*=|![ ]*=|<|>)");
                    var index = item.Index + ExtraCounts;
                    var VarName = new Regex(">[ ]*=|<[ ]*=|=[ ]*=|![ ]*=|<|>").Replace(VarRegex.Matches(Condition.Substring(0, index)).Last().Value, string.Empty).Trim();

                    Condition = $"{Condition.Substring(0, index)}{Condition.Substring(index, 2)} {VarName}{Condition.Substring(index + 2)}";
                    ExtraCounts += VarName.Length + 1;
                }

                SB.AppendLine($"#region {Line}");
                SB.AppendLine($"for(;{Condition};){{");
                SB.AppendLine($"    if(FullStack.AddRangAndCheckHasGOTOOrEnd({PerformName}(false, null)))");
                SB.AppendLine($"        return FullStack;");
                SB.AppendLine($"}}");
                SB.AppendLine($"#endregion");
                
               
                return SB.ToString();

            }
            else if(new Regex($@"{"PERFORM".RegexUpperLower()}[ ]+[a-zA-Z0-9-]+\.*").IsMatch(Line))
            {
                StringBuilder SB = new StringBuilder();
                string PerformName = NamingConverter.Convert(Line.RegexReplace("PERFORM", string.Empty).Replace(".", string.Empty).Trim());
                SB.AppendLine($"#region {Line}");
                SB.AppendLine($"if(FullStack.AddRangAndCheckHasGOTOOrEnd({PerformName}(false, null)))");
                SB.AppendLine($"    return FullStack;");                
                SB.AppendLine($"#endregion");
                return SB.ToString();
            }
            throw new Exception($"Invalid {StatementTypes.First().ToString()} Statement, {Line}");
           
        }
    }
}
