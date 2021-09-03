using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CobolToCSharp
{

    public class SUBTRACTStatementConverter : IStatementConverter
    {
        public List<StatementType> StatementTypes => new List<StatementType>(new StatementType[] { StatementType.SUBTRACT });

        public string Convert(string Line, Paragraph Paragraph, List<Paragraph> Paragraphs)
        {

            if (new Regex(".+GIVING.+").IsMatch(Line))
            {
                Match SUBTRACTStatement = new Regex("^SUBTRACT.+GIVING.").Match(Line);
                string[] SUBTRACTVariables = Line.Substring(0, SUBTRACTStatement.Length).Replace("SUBTRACT", string.Empty).Replace("GIVING", string.Empty).Replace("FROM", ",").Split(',').Select(r => NamingConverter.Convert(r.Trim())).ToArray();
                string AssignVariable = NamingConverter.Convert((Line.Substring(SUBTRACTStatement.Length).Replace(".", string.Empty).Trim()));
                return $"{AssignVariable} = {string.Join(" - ", SUBTRACTVariables)};";
            }

            else if (new Regex(".+FROM.+").IsMatch(Line))
            {
                Match SUBTRACTStatement = new Regex("^SUBTRACT .+ FROM").Match(Line);
                string[] SUBTRACTVariables = Line.Substring(0, SUBTRACTStatement.Length).Replace("SUBTRACT", string.Empty).Replace("FROM", string.Empty).Split(',').Select(r => NamingConverter.Convert(r.Trim())).ToArray();
                string AssignVariable = NamingConverter.Convert((Line.Substring(SUBTRACTStatement.Length).Replace(".", string.Empty).Trim()));
                return $"{AssignVariable} -= {string.Join(" + ", SUBTRACTVariables)};";
            }
            throw new Exception("SUBTRACT statement is not recognized");
        }

        public List<CobolVariable> ExtractVariables(string Line, Paragraph Paragraph, List<Paragraph> Paragraphs, List<CobolVariable> DefinedCobolVariables)
        {
            return new List<CobolVariable>();
        }
    }
}
