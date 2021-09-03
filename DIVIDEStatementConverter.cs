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

        public string Convert(string Line, Paragraph Paragraph, List<Paragraph> Paragraphs)
        {
            if(new Regex("DIVIDE[ ]+[a-zA-Z0-9-]+[ ]+BY[ ]+([a-zA-Z0-9-]+|[0-9]*.[0-9]*)[ ]+GIVING[ ]+([a-zA-Z0-9-]+)( REMAINDER [a-zA-Z0-9-]+)*").IsMatch(Line))
            {
                string[] Fields = Line.Replace("DIVIDE", string.Empty).Replace("BY", string.Empty).Replace("GIVING", string.Empty).Replace("REMAINDER", string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(r => NamingConverter.Convert(r)).ToArray();
                StringBuilder SB = new StringBuilder();
                SB.AppendLine($"{Fields[2]} = (int)Math.Floor((double){Fields[0]} / (double){Fields[1]});");
                if(new Regex(".+REMAINDER").IsMatch(Line))
                    SB.AppendLine($"{Fields[3].Replace(".",string.Empty)} = (int)Math.Floor((double){Fields[0]} % (double){Fields[1]});");

                return SB.ToString();
            }
            throw new Exception($"Invalid {StatementTypes.First().ToString()} Statement, {Line}");
        }

        public List<CobolVariable> ExtractVariables(string Line, Paragraph Paragraph, List<Paragraph> Paragraphs, List<CobolVariable> DefinedCobolVariables)
        {
            return new List<CobolVariable>();
        }
    }
}
