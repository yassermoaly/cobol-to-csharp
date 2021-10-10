using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CobolToCSharp
{
    public class InspectStatementConverter : IStatementConverter
    {
        public List<StatementType> StatementTypes => new List<StatementType>(new StatementType[] { StatementType.INSPECT });

        public string Convert(string Line, Paragraph Paragraph, List<Paragraph> Paragraphs, Dictionary<string, string> CobolVariablesDataTypes = null)
        {
            if(new Regex($"{"INSPECT".RegexUpperLower()}[ ]+[a-zA-Z][a-zA-Z0-9-]+[ ]+{"REPLACING".RegexUpperLower()}[ ]+{"ALL".RegexUpperLower()}[ ]+[Xx]*\"[0-9A-Za-z /-_]+\"[ ]+{"BY".RegexUpperLower()}[ ]+[Xx]*\"[0-9A-Za-z /-_]+\"").IsMatch(Line))
            {
                Match VariableMatch = new Regex($"{"INSPECT".RegexUpperLower()}[ ]+[a-zA-Z][a-zA-Z0-9-]+[ ]+{"REPLACING".RegexUpperLower()}").Match(Line);
                string Variable = VariableMatch.Value.RegexReplace("INSPECT[ ]+", string.Empty).RegexReplace("[ ]+REPLACING", string.Empty).Trim();
                Match ReplaceMatch = new Regex($"{"ALL".RegexUpperLower()}[ ]+[Xx]*\"[0-9A-Za-z /-_]+\"[ ]+{"BY".RegexUpperLower()}[ ]+[Xx]*\"[0-9A-Za-z /-_]+\"").Match(Line);
                string[] ReplaceTokens = ReplaceMatch.Value.RegexReplace("ALL", " ").RegexReplace("BY", " ").Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(r => r.Trim()).ToArray();
                bool ReplaceFromIsHexa = ReplaceTokens[0].ToUpper().StartsWith("X");
                bool ReplaceToIsHexa = ReplaceTokens[1].ToUpper().StartsWith("X");
                Regex ReplaceValue = new Regex("\"[0-9A-Za-z /-_]+\"");
                string ReplaceFromValue = ReplaceValue.Match(ReplaceTokens[0]).Value;
                string ReplaceToValue = ReplaceValue.Match(ReplaceTokens[1]).Value;
                return $"{NamingConverter.Convert(Variable)} = {NamingConverter.Convert(Variable)}.Replace({ReplaceFromValue},{ReplaceFromIsHexa.ToString().ToLower()},{ReplaceToValue},{ReplaceToIsHexa.ToString().ToLower()});";
            }

            throw new Exception($"Invalid {StatementTypes.First().ToString()} Statement, {Line}");
        }
    }
}
