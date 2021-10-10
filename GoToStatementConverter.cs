using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CobolToCSharp
{
    public class GoToStatementConverter : IStatementConverter
    {
        public List<StatementType> StatementTypes => new List<StatementType>(new StatementType[] { StatementType.GOTO});

        public string Convert(string Line, Paragraph Paragraph, List<Paragraph> Paragraphs, Dictionary<string,string> CobolVariablesDataTypes = null)
        {
            if (new Regex($"^{"GO".RegexUpperLower()}[ ]+({"TO".RegexUpperLower()}[ ]+)*[a-zA-Z0-9-]+").IsMatch(Line))
            {
                StringBuilder SB = new StringBuilder();
              
                string FunctionName = Line.RegexReplace("GO[ ]+", string.Empty).RegexReplace("TO[ ]+", string.Empty).Replace(".", string.Empty).Trim();
                if (!string.IsNullOrEmpty(FunctionName))
                {
                    SB.AppendLine($"#region {Line}");
                    SB.AppendLine($"if(NextScope!=null && NextScope.Contains(\"{NamingConverter.Convert(FunctionName)}\"))");
                    SB.AppendLine($"    return FullStack.AddRangeAndReturnSource({NamingConverter.Convert(FunctionName)}(true, NextScope));");
                    SB.AppendLine($"else");
                    SB.AppendLine($"    return FullStack.AddRangeAndReturnSource({NamingConverter.Convert(FunctionName)}(true, null));");
                    SB.AppendLine($"#endregion");
                    return SB.ToString();
                }
                else
                {
                    throw new Exception($"Invalid {StatementTypes.First().ToString()} Statement, {Line}");
                }
               
            }
            throw new Exception($"Invalid {StatementTypes.First().ToString()} Statement, {Line}");
            
        }
    }
}
