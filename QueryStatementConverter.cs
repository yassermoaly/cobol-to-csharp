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

        public static Dictionary<string, string> CursorSelectQueries = new Dictionary<string, string>();
        private static readonly Regex RegexSelectStatement = new Regex($"^{"SELECT".RegexUpperLower()}.+{"INTO".RegexUpperLower()}.+{"FROM".RegexUpperLower()}");
        private static readonly Regex RegexInsertStatement = new Regex($@"^{"INSERT".RegexUpperLower()}.+{"INTO".RegexUpperLower()}.+({"SELECT".RegexUpperLower()}|{"VALUES".RegexUpperLower()}).+\(.+\)");
        private static readonly Regex RegexUpdateStatement = new Regex($@"^{"UPDATE".RegexUpperLower()}.+{"SET".RegexUpperLower()}.+{"WHERE".RegexUpperLower()}.+");
        private static readonly Regex RegexDeclareCursorStatement = new Regex($@"^{"DECLARE".RegexUpperLower()}.+{"CURSOR".RegexUpperLower()}.+{"FOR".RegexUpperLower()}.+{"SELECT".RegexUpperLower()}.+");
        private static readonly Regex RegexFetchCursorStatement = new Regex($@"^{"FETCH".RegexUpperLower()}.+{"INTO".RegexUpperLower()}.+");
        private static readonly Regex RegexOpenCursorStatement = new Regex($@"^{"OPEN".RegexUpperLower()}.+");
        private static readonly Regex RegexCloseCursorStatement = new Regex($@"^{"CLOSE".RegexUpperLower()}.+");
        private static readonly Regex RegexDeleteStatement = new Regex($@"^{"DELETE".RegexUpperLower()}.+({"FROM".RegexUpperLower()})*.+{"WHERE".RegexUpperLower()}.+");
        private static readonly Regex RegexSqlParameter = new Regex(":[ ]*[a-zA-Z][a-zA-Z0-9-]+");
        private static readonly Regex RegexFillInParameters = new Regex($"{"INTO".RegexUpperLower()}(.*?){"[ ]+FROM".RegexUpperLower()}");
        public List<StatementType> StatementTypes => new List<StatementType>(new StatementType[] { StatementType.QUERY });
        private string ExtractAndRenameParameters(ref string Line)
        {
            bool IsSelectStatement = RegexSelectStatement.IsMatch(Line);
            Match FillParametersMatch = RegexFillInParameters.Match(Line);
            string[] FillParameters = IsSelectStatement ? (FillParametersMatch.Success?FillParametersMatch.Value.Substring(4, FillParametersMatch.Value.Length - 8).Replace(":", string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries).Select(r => r.Trim()).ToArray(): new string[0]): new string[0];
            StringBuilder Query = new StringBuilder();
            Query.AppendLine("Parameters=new Dictionary<string,object>()");
            Query.AppendLine("{");            
            HashSet<string> Parameters = new HashSet<string>();
            string LineWithoutFillIn = IsSelectStatement?FillParametersMatch.Value.Length == 0 ? Line : Line.Replace(FillParametersMatch.Value, string.Empty): Line;
            bool HasEmptyParameters = true;
            foreach (Match Match in RegexSqlParameter.Matches(LineWithoutFillIn))
            {
                string ParameterName = NamingConverter.Convert(Match.Value).Replace(":", string.Empty).Trim();
                if (!Parameters.Contains(ParameterName))
                {
                    HasEmptyParameters = false;
                    Query.AppendLine($"    {{ \"{ NamingConverter.Convert(ParameterName)}\",{NamingConverter.Convert(ParameterName)} }},");
                    //Query.AppendLine($"Parameters.Add(\"{ NamingConverter.Convert(ParameterName)}\", {NamingConverter.Convert(ParameterName)});");
                    Line = Line.Replace(Match.Value, $":{NamingConverter.Convert(ParameterName)}");
                    Parameters.Add(ParameterName);
                }
            }
            if (HasEmptyParameters)
                return string.Empty;
            //Remove Last , 
            Query.Remove(Query.ToString().LastIndexOf(','), 1);
            Query.AppendLine("};");            
            return Query.ToString();
        }

       
        public string Convert(string Line, Paragraph Paragraph, List<Paragraph> Paragraphs, Dictionary<string,string> CobolVariablesDataTypes = null)
        {
            string OrLine = Line;
            Line = Paragraph.RegexEXECSQL.Replace(Line,string.Empty).RegexReplace("END-EXEC\\.*", string.Empty).Trim();           
            StringBuilder Query = new StringBuilder(ExtractAndRenameParameters(ref Line));         
            if (RegexSelectStatement.IsMatch(Line))
            {            
                Match FillParametersMatch = RegexFillInParameters.Match(Line);
                Line = $"{Line.Substring(0, FillParametersMatch.Index)} {Line.Substring(FillParametersMatch.Index+ FillParametersMatch.Length-4)}".Replace("  "," ");
                //Query.AppendLine("Parameters=new Dictionary<string,object>();");
                //foreach (Match Match in RegexSqlParameter.Matches(Line))
                //{                    
                //    string ParameterName = NamingConverter.Convert(Match.Value).Replace(":",string.Empty);
                //    Query.AppendLine($"Parameters.Add(\"{ ParameterName}\", {ParameterName});");
                //    Line = Line.Replace(Match.Value, $"@{ParameterName}");
                //}
                

                Query.AppendLine($"SQL = \"{Line}\";");
                Query.AppendLine($"DT = DBOPEARATION.ExecuteDT(SQL,Parameters,out SQLCODE);");
                string[] FillParameters = FillParametersMatch.Value.Substring(4, FillParametersMatch.Value.Length - 8).Replace(":",string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries).Select(r=>r.Trim()).ToArray();
                Match SelectParamatersMatch = new Regex($"^{"SELECT".RegexUpperLower()}.+{"FROM".RegexUpperLower()}").Match(Line);
                string[] SelectParameters = SelectParamatersMatch.Value.Substring(6, SelectParamatersMatch.Value.Length - 10).Split(',', StringSplitOptions.RemoveEmptyEntries).Select(r => r.Trim()).ToArray();
                Query.AppendLine("if (DT.Rows.Count > 0)");
                Query.AppendLine("{");
                for (int h = 0; h < FillParameters.Length; h++)
                {
                    string ConvertType = string.Empty;
                    string VariableDataType = CobolVariablesDataTypes[FillParameters[h].Replace("_","-")];
                    switch (VariableDataType)
                    {
                        case "double":
                            ConvertType = "ToDouble";
                            break;
                        case "long":
                            ConvertType = "ToInt64";
                            break;
                        case "class":
                        case "string":
                            ConvertType = "ToString";
                            break;
                        default:
                            ConvertType = "ToInt64";
                            break;
                    }
                    string ColumnName = new Regex("^[a-zA-Z][a-zA-Z0-9-]+$").IsMatch(SelectParameters[h]) ? $"\"{SelectParameters[h]}\"" : $"{h}";
                    Query.AppendLine($"    if(DT.Rows[0][{ColumnName}]!=DBNull.Value)");
                    Query.AppendLine($"        {NamingConverter.Convert(FillParameters[h].Replace("@", string.Empty))} = Convert.{ConvertType}(DT.Rows[0][{ColumnName}]);");
                    //if (new Regex("^[a-zA-Z][a-zA-Z0-9-]+$").IsMatch(SelectParameters[h]))
                    //    Query.AppendLine($"    {NamingConverter.Convert(FillParameters[h].Replace("@",string.Empty))} = Convert.{ConvertType}(DT.Rows[0][\"{SelectParameters[h]}\"]);");
                    //else
                        
                }
                Query.AppendLine("}");
                return Query.ToString();
            }
            else if (RegexInsertStatement.IsMatch(Line) || RegexUpdateStatement.IsMatch(Line) || RegexDeleteStatement.IsMatch(Line))
            {             
                Query.AppendLine($"SQL = \"{Line}\";");
                Query.AppendLine($"DBOPEARATION.ExecuteQuery(SQL,Parameters,out SQLCODE);");
                return Query.ToString();
            }
            
            else if (RegexDeclareCursorStatement.IsMatch(Line))
            {                
                Match DeclareCursor = new Regex($"^{"DECLARE".RegexUpperLower()}[ ]+.+{"CURSOR".RegexUpperLower()}[ ]+{"FOR".RegexUpperLower()} ").Match(Line);
                Line = Line.Substring(DeclareCursor.Index + DeclareCursor.Length);
                string CursorName = DeclareCursor.Value.Replace("DECLARE", string.Empty).Replace("CURSOR", string.Empty).Replace("FOR", string.Empty).Trim();
                Query.AppendLine($"SQL = \"{Line}\";");
                //Query.AppendLine($"Cursors.Add(new Cursor(\"{CursorName}\",SQL,Parameters));");              
                Query.AppendLine($"Cursors.AddNew(\"{CursorName}\",SQL,Parameters);");
                CursorSelectQueries.Add(CursorName, Line);
                return Query.ToString();
            }
            else if (RegexFetchCursorStatement.IsMatch(Line))
            {                
                Match FetchCursor = new Regex($"^{"FETCH".RegexUpperLower()}.+{"INTO".RegexUpperLower()}").Match(Line);
                string CursorName = FetchCursor.Value.RegexReplace("FETCH", string.Empty).RegexReplace("INTO", string.Empty).Trim();
                string[] FillParameters = Line.Substring(FetchCursor.Length).Split(',').Select(r => r.Replace("@", string.Empty).Trim()).ToArray();
                string SelectStatementQuery = CursorSelectQueries[CursorName];
                Match SelectStatement = new Regex($"^{"SELECT".RegexUpperLower()}.+{"FROM".RegexUpperLower()}").Match(SelectStatementQuery);
                string[] SelectParameters = SelectStatement.Value.RegexReplace("SELECT",string.Empty).RegexReplace("FROM",string.Empty).Split(',').Select(r=>r.Trim()).ToArray();
                Query = new StringBuilder();
                Query.AppendLine($"var {CursorName}_DR = Cursors.Get(\"{CursorName}\").Fetch(out SQLCODE);");
                Query.AppendLine($"if ({CursorName}_DR != null)");
                Query.AppendLine($"{{");
                for (int h = 0; h < FillParameters.Length; h++)
                {
                    if (new Regex("^[a-zA-Z][a-zA-Z0-9-]+$").IsMatch(SelectParameters[h]))
                        Query.AppendLine($"    {NamingConverter.Convert(FillParameters[h].Replace(":", string.Empty))} = Convert.ToInt64({CursorName}_DR[\"{SelectParameters[h]}\"]);");
                    else
                        Query.AppendLine($"    {NamingConverter.Convert(FillParameters[h].Replace(":", string.Empty))} = Convert.ToInt64({CursorName}_DR[{h}]);");
                }
                Query.AppendLine($"}}");
                return Query.ToString();
            }
            else if (RegexOpenCursorStatement.IsMatch(Line))
            {
                string CursorName = Line.RegexReplace("OPEN", string.Empty).Replace(".", string.Empty).Trim();
                Query = new StringBuilder();
                Query.AppendLine($"Cursors.Get(\"{CursorName}\").Open();");
                return Query.ToString();
            }
            else if (RegexCloseCursorStatement.IsMatch(Line))
            {
                string CursorName = Line.RegexReplace("CLOSE", string.Empty).Replace(".", string.Empty).Trim();
                Query = new StringBuilder();
                Query.AppendLine($"Cursors.Remove(Cursors.Get(\"{CursorName}\"));");
                return Query.ToString();
            }

            throw new Exception("SQL Statement is not recognized");
        }
    }
}
