using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CobolToCSharp
{
    public class ADDStatementConverter: IStatementConverter
    {
        public List<StatementType> StatementTypes => new List<StatementType>(new StatementType[] { StatementType.ADD });

        public string Convert(string Line, Paragraph Paragraph, List<Paragraph> Paragraphs, Dictionary<string,string> CobolVariablesDataTypes = null)
        {
       
            if(new Regex($".+{"GIVING".RegexUpperLower()}.+").IsMatch(Line))
            {
                Match ADDStatement = new Regex($"^{"ADD".RegexUpperLower()}.+{"GIVING".RegexUpperLower()}.").Match(Line);
                string[] ADDVariables = Line.Substring(0, ADDStatement.Length).RegexReplace("ADD[ ]+", string.Empty).RegexReplace("GIVING[ ]+", string.Empty).RegexReplace("TO[ ]+", ",").Split(new char[] { ',', ' ' },StringSplitOptions.RemoveEmptyEntries).Select(r => NamingConverter.Convert(r.Trim())).ToArray();
                string AssignVariable = NamingConverter.Convert((Line.Substring(ADDStatement.Length).Replace(".", string.Empty).Trim()));
                return $"{AssignVariable} = {string.Join(" + ", ADDVariables)};";
            }
           
            else if (new Regex($".+{"TO".RegexUpperLower()}.+").IsMatch(Line))
            {
                Match ADDStatement = new Regex($"^{"ADD".RegexUpperLower()}[ ]+.+[ ]+{"TO".RegexUpperLower()}").Match(Line);
                string[] ADDVariables = Line.Substring(0, ADDStatement.Length).RegexReplace("ADD", string.Empty).RegexReplace("TO", string.Empty).Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(r => NamingConverter.Convert(r.Trim())).ToArray();
                string AssignVariable = NamingConverter.Convert((Line.Substring(ADDStatement.Length).Replace(".", string.Empty).Trim()));
                StringBuilder SB = new StringBuilder();
                foreach (var Variable in AssignVariable.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                {
                    SB.AppendLine($"{Variable} += {string.Join(" + ", ADDVariables)};");
                }
                return SB.ToString();
            }
            throw new Exception("ADD statement is not recognized");
        }
    }
}
