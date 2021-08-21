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

        public string Convert(string Line, Paragraph Paragraph, List<Paragraph> Paragraphs)
        {

            if(new Regex(".+GIVING.+").IsMatch(Line))
            {
                Match ADDStatement = new Regex("^ADD.+GIVING.").Match(Line);
                string[] ADDVariables = Line.Substring(0, ADDStatement.Length).Replace("ADD", string.Empty).Replace("GIVING", string.Empty).Replace("TO",",").Split(',').Select(r => NamingConverter.Convert(r.Trim())).ToArray();
                string AssignVariable = NamingConverter.Convert((Line.Substring(ADDStatement.Length).Replace(".", string.Empty).Trim()));
                return $"{AssignVariable} = {string.Join(" + ", ADDVariables)};";
            }
           
            else if (new Regex(".+TO.+").IsMatch(Line))
            {
                Match ADDStatement = new Regex("^ADD .+ TO").Match(Line);
                string[] ADDVariables = Line.Substring(0, ADDStatement.Length).Replace("ADD", string.Empty).Replace("TO", string.Empty).Split(',').Select(r => NamingConverter.Convert(r.Trim())).ToArray();
                string AssignVariable = NamingConverter.Convert((Line.Substring(ADDStatement.Length).Replace(".", string.Empty).Trim()));
                return $"{AssignVariable} += {string.Join(" + ", ADDVariables)};";
            }
            throw new Exception("ADD statement is not recognized");
        }
    }
}
