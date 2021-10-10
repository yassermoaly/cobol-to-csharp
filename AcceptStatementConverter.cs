using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CobolToCSharp
{
    public class AcceptStatementConverter : IStatementConverter
    {
        public List<StatementType> StatementTypes => new List<StatementType>(new StatementType[] { StatementType.ACCEPT });

        public string Convert(string Line, Paragraph Paragraph, List<Paragraph> Paragraphs, Dictionary<string, string> CobolVariablesDataTypes = null)
        {
            if(new Regex($"{"ACCEPT".RegexUpperLower()}[ ]+[a-zA-Z][a-zA-Z0-9-]+[ ]+{"FROM[ ]+DAY-OF-WEEK".RegexUpperLower()}").IsMatch(Line))
            {
                string VariableName = new Regex($"{"ACCEPT".RegexUpperLower()}[ ]+[a-zA-Z][a-zA-Z0-9-]+[ ]+{"FROM".RegexUpperLower()}").Match(Line).Value.RegexReplace("ACCEPT", string.Empty).RegexReplace("FROM", string.Empty).Trim();
                return $"{NamingConverter.Convert(VariableName)} = GetDayOfMonth();";
            }

            throw new Exception($"Invalid {StatementTypes.First().ToString()} Statement, {Line}");
        }
    }
}
