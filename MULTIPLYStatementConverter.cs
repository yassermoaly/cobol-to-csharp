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

        public string Convert(string Line, Paragraph Paragraph, List<Paragraph> Paragraphs, Dictionary<string,string> CobolVariablesDataTypes = null)
        {
           
            if(new Regex($@"{"MULTIPLY".RegexUpperLower()}[ ]+([a-zA-Z0-9-]+)[ ]+{"BY".RegexUpperLower()}[ ]+([a-zA-Z0-9-]+|[0-9]*.[0-9]*)[ ]+{"GIVING".RegexUpperLower()}[ ]+([a-zA-Z0-9-]+)([ ]+{"ROUNDED".RegexUpperLower()})*").IsMatch(Line))
            {
                string[] Fields = Line.RegexReplace("MULTIPLY", string.Empty).RegexReplace("BY", string.Empty).RegexReplace("GIVING", string.Empty).RegexReplace("ROUNDED", string.Empty).Split(' ',StringSplitOptions.RemoveEmptyEntries).Select(r=>NamingConverter.Convert(r)).ToArray();
                string RoundFunctin = new Regex($".+{"ROUNDED".RegexUpperLower()}").IsMatch(Line) ? "Ceiling" : "Floor";
                return $"{NamingConverter.Convert(Fields[2])} = (long)Math.Ceiling((double){NamingConverter.Convert(Fields[0])} * (double){NamingConverter.Convert(Fields[1])});";
            }
            else if (new Regex($@"{"MULTIPLY".RegexUpperLower()}[ ]+([a-zA-Z0-9-]+|[0-9]*.[0-9]*)[ ]+{"BY".RegexUpperLower()}[ ]+([a-zA-Z0-9-]+|[0-9]*.[0-9]*)").IsMatch(Line))
            {
                string[] Fields = Line.RegexReplace("MULTIPLY", string.Empty).RegexReplace("BY", string.Empty).RegexReplace("GIVING", string.Empty).RegexReplace("ROUNDED", string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(r => NamingConverter.Convert(r)).ToArray();
                string RoundFunctin = new Regex($".+{"ROUNDED".RegexUpperLower()}").IsMatch(Line) ? "Ceiling" : "Floor";
                return $"{NamingConverter.Convert(Fields[1])} = (long)Math.Ceiling((double){NamingConverter.Convert(Fields[1])} * (double){NamingConverter.Convert(Fields[0])});";
            }
            throw new Exception($"Invalid {StatementTypes.First().ToString()} Statement, {Line}");
        }
    }
}
