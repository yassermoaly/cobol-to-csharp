using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CobolToCSharp
{
    public class MULTIPLYStatementConverter: IStatementConverter
    {
        public List<StatementType> StatementTypes => new List<StatementType>(new StatementType[] { StatementType.MULTIPLY });

        public string Convert(string Line, Paragraph Paragraph, List<Paragraph> Paragraphs)
        {
            if(new Regex(@"MULTIPLY[ ]+([a-zA-Z0-9-]+)[ ]+BY[ ]+([a-zA-Z0-9-]+|[0-9]*.[0-9]*)[ ]+GIVING[ ]+([a-zA-Z0-9-]+)[ ROUNDED]*").IsMatch(Line))
            {
                string[] Fields = Line.Replace("MULTIPLY", string.Empty).Replace("BY", string.Empty).Replace("GIVING", string.Empty).Replace("ROUNDED", string.Empty).Split(' ',StringSplitOptions.RemoveEmptyEntries).Select(r=>NamingConverter.Convert(r)).ToArray();
                string RoundFunctin = new Regex(".+ROUNDED").IsMatch(Line) ? "Ceiling" : "Floor";
                return $"{Fields[2]} = (int)Math.Ceiling((double){Fields[0]} * (double){Fields[1]});";
            }
            throw new Exception($"Invalid {StatementTypes.First().ToString()} Statement, {Line}");
        }
    }
}
