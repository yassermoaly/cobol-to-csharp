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
        static Dictionary<string, string> CursorSelectQueries = new Dictionary<string, string>();
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
        private string ExtractAndRenameParameters(ref string Line)
        {
            StringBuilder Query = new StringBuilder();
            Query.AppendLine("Parameters=new Dictionary<string,object>();");
            HashSet<string> Parameters = new HashSet<string>();
            foreach (Match Match in RegexSqlParameter.Matches(Line))
            {
                string ParameterName = NamingConverter.Convert(Match.Value).Replace(":", string.Empty);
                if (!Parameters.Contains(ParameterName))
                {
                    Query.AppendLine($"Parameters.Add(\"{ ParameterName}\", {ParameterName});");
                    Line = Line.Replace(Match.Value, $"@{ParameterName}");
                    Parameters.Add(ParameterName);
                }
            }
            
            return Query.ToString();
        }
        public string Convert(string Line,Paragraph Paragraph, List<Paragraph> Paragraphs)
        {
            string OrLine = Line;
            Line = Line.Replace("EXEC SQL", string.Empty).Replace("END-EXEC.", string.Empty).Trim();           
            StringBuilder Query = new StringBuilder(ExtractAndRenameParameters(ref Line));         
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
                    if(new Regex("^[a-zA-Z][a-zA-Z0-9-]+$").IsMatch(SelectParameters[h]))
                        Query.AppendLine($"{NamingConverter.Convert(FillParameters[h])} = DT.Rows[0][\"{SelectParameters[h]}\"];");
                    else
                        Query.AppendLine($"{NamingConverter.Convert(FillParameters[h])} = DT.Rows[0][\"{h}\"];");
                }
                return Query.ToString();
            }
            else if (RegexInsertStatement.IsMatch(Line) || RegexUpdateStatement.IsMatch(Line) || RegexDeleteStatement.IsMatch(Line))
            {             
                Query.AppendLine($"SQL = \"{Line}\";");
                Query.AppendLine($"DT = DBOPEARATION.DBExecuteDT(SQL, ConnStrOracle, DT,Parameters);");
                return Query.ToString();
            }
            
            else if (RegexDeclareCursorStatement.IsMatch(Line))
            {                
                Match DeclareCursot = new Regex("^DECLARE.+CURSOR FOR ").Match(Line);
                Line = Line.Substring(DeclareCursot.Index + DeclareCursot.Length);
                string CursorName = DeclareCursot.Value.Replace("DECLARE", string.Empty).Replace("CURSOR", string.Empty).Replace("FOR", string.Empty).Trim();
                Query.AppendLine($"SQL = \"{Line}\";");
                Query.AppendLine($"Cursors.Add(new Cursor(\"{CursorName}\",SQL,Parameters));");              
                CursorSelectQueries.Add(CursorName, Line);
                return Query.ToString();
            }
            else if (RegexFetchCursorStatement.IsMatch(Line))
            {                
                Match FetchCursor = new Regex("^FETCH.+INTO").Match(Line);
                string CursorName = FetchCursor.Value.Replace("FETCH", string.Empty).Replace("INTO", string.Empty).Trim();
                string[] FillParameters = Line.Substring(FetchCursor.Length).Split(',').Select(r => r.Replace("@", string.Empty).Trim()).ToArray();
                string SelectStatementQuery = CursorSelectQueries[CursorName];
                Match SelectStatement = new Regex("^SELECT.+FROM").Match(SelectStatementQuery);
                string[] SelectParameters = SelectStatement.Value.Replace("SELECT",string.Empty).Replace("FROM",string.Empty).Split(',').Select(r=>r.Trim()).ToArray();
                Query = new StringBuilder();
                Query.AppendLine($"var {CursorName}_DR = Cursors.Get(\"{CursorName}\").Fetch();");
                for (int h = 0; h < FillParameters.Length; h++)
                {
                    if (new Regex("^[a-zA-Z][a-zA-Z0-9-]+$").IsMatch(SelectParameters[h]))
                        Query.AppendLine($"{NamingConverter.Convert(FillParameters[h])} = {CursorName}_DR[\"{SelectParameters[h]}\"];");
                    else
                        Query.AppendLine($"{NamingConverter.Convert(FillParameters[h])} = {CursorName}_DR[\"{h}\"];");
                }

                return Query.ToString();
            }
            else if (RegexOpenCursorStatement.IsMatch(Line))
            {
                string CursorName = Line.Replace("OPEN", string.Empty).Replace(".", string.Empty).Trim();
                Query = new StringBuilder();
                Query.AppendLine($"Cursors.Get(\"{CursorName}\").Open();");
                return Query.ToString();
            }
            else if (RegexCloseCursorStatement.IsMatch(Line))
            {
                string CursorName = Line.Replace("CLOSE", string.Empty).Replace(".", string.Empty).Trim();
                Query = new StringBuilder();
                Query.AppendLine($"Cursors.Remove(\"{CursorName}\");");
                return Query.ToString();
            }

            throw new Exception("SQL Statement is not recognized");
        }
    }
}
