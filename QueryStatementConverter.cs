using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CobolToCSharp
{
    public class QueryStatementConverter : IStatementConverter
    {
        private static readonly Regex RegexSelectStatement = new Regex("^SELECT.+INTO.+FROM");
        private static readonly Regex RegexInsertStatement = new Regex(@"^INSERT.+INTO.+VALUES.+\(.+\)");
        private static readonly Regex RegexUpdateStatement = new Regex(@"^UPDATE.+SET.+WHERE.+");
        private static readonly Regex RegexDeclareCursorStatement = new Regex(@"^DECLARE.+CURSOR.+FOR.+SELECT.+");
        private static readonly Regex RegexFetchCursorStatement = new Regex(@"^FETCH.+INTO.+");
        private static readonly Regex RegexOpenCursorStatement = new Regex(@"^OPEN.+");
        private static readonly Regex RegexCloseCursorStatement = new Regex(@"^CLOSE.+");
        private static readonly Regex RegexDeleteStatement = new Regex(@"^DELETE.+FROM.+WHERE.+");
        private static readonly Regex RegexSqlParameter = new Regex(":[a-zA-Z][a-zA-Z0-9-]+");
        public List<StatementType> StatementTypes => new List<StatementType>(new StatementType[] { StatementType.QUERY });

        public string Convert(string Line,Paragraph Paragraph, List<Paragraph> Paragraphs)
        {
            StringBuilder Query = new StringBuilder();
            Line = Line.Replace("EXEC SQL", string.Empty).Replace("END-EXEC.", string.Empty).Trim();
            if (RegexSelectStatement.IsMatch(Line))
            {
                
                Match FillParametersMatch = new Regex("INTO.+FROM").Match(Line);
                Line = $"{Line.Substring(0, FillParametersMatch.Index)} {Line.Substring(FillParametersMatch.Index+ FillParametersMatch.Length-4)}".Replace("  "," ");
                Query.AppendLine("Parameters=new Dictionary<string,object>();");
                foreach (Match Match in RegexSqlParameter.Matches(Line))
                {                    
                    string ParameterName = NamingConverter.Convert(Match.Value).Replace(":",string.Empty);
                    Query.AppendLine($"Parameters.Add(\"{ ParameterName}\", {ParameterName});");
                    Line = Line.Replace(Match.Value, $"@{ParameterName}");
                }
                

                Query.AppendLine($"SQL = \"{Line}\";");
                Query.AppendLine($"DT = DBOPEARATION.DBExecuteDT(SQL, ConnStrOracle, DT,Parameters);");


                string[] FillParameters = FillParametersMatch.Value.Substring(4, FillParametersMatch.Value.Length - 8).Replace(":",string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries).Select(r=>r.Trim()).ToArray();

                Match SelectParamatersMatch = new Regex("^SELECT.+FROM").Match(Line);
                string[] SelectParameters = SelectParamatersMatch.Value.Substring(6, SelectParamatersMatch.Value.Length - 10).Split(',', StringSplitOptions.RemoveEmptyEntries).Select(r => r.Trim()).ToArray();
                for (int h = 0; h < FillParameters.Length; h++)
                {
                    Query.AppendLine($"{FillParameters[h]} = DT.Rows[0][\"{SelectParameters[h]}\"];");
                }
                return Query.ToString();
            }
            else if (RegexInsertStatement.IsMatch(Line))
            {
                int x = 10;
            }
            else if (RegexUpdateStatement.IsMatch(Line))
            {
                int x = 10;
            }
            else if (RegexDeleteStatement.IsMatch(Line))
            {

            }
            else if (RegexDeclareCursorStatement.IsMatch(Line))
            {

            }
            else if (RegexFetchCursorStatement.IsMatch(Line))
            {

            }
            else if (RegexOpenCursorStatement.IsMatch(Line))
            {

            }
            else if (RegexCloseCursorStatement.IsMatch(Line))
            {

            }
            else
            {
                int x = 10;
            }
            return string.Empty;
        }
    }
}
