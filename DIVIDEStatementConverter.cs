using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CobolToCSharp
{
    public class DIVIDEStatementConverter : IStatementConverter
    {
        public List<StatementType> StatementTypes => new List<StatementType>(new StatementType[] { StatementType.DIVIDE });

        public string Convert(string Line, Paragraph Paragraph, List<Paragraph> Paragraphs, Dictionary<string,string> CobolVariablesDataTypes = null)
        {
            if(new Regex($"{"DIVIDE".RegexUpperLower()}[ ]+[a-zA-Z0-9-]+[ ]+{"BY".RegexUpperLower()}[ ]+([a-zA-Z0-9-]+|[0-9]*.[0-9]*)[ ]+{"GIVING".RegexUpperLower()}[ ]+([a-zA-Z0-9-]+)( {"REMAINDER".RegexUpperLower()} [a-zA-Z0-9-]+)*").IsMatch(Line))
            {
                string[] Fields = Line.RegexReplace("DIVIDE", string.Empty).RegexReplace("BY", string.Empty).RegexReplace("GIVING", string.Empty).RegexReplace("REMAINDER", string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(r => NamingConverter.Convert(r)).ToArray();
                StringBuilder SB = new StringBuilder();
                SB.AppendLine($"{Fields[2]} = (int)Math.Floor((double){Fields[0]} / (double){Fields[1]});");
                if(new Regex($".+{"REMAINDER".RegexUpperLower()}").IsMatch(Line))
                    SB.AppendLine($"{Fields[3].Replace(".",string.Empty)} = (int)Math.Floor((double){Fields[0]} % (double){Fields[1]});");

                return SB.ToString();
            }
            throw new Exception($"Invalid {StatementTypes.First().ToString()} Statement, {Line}");
        }
    }
}
