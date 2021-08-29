using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace CobolToCSharp
{
    enum ParseMode
    {
        NONE,
        COLLECT_WORKING_STORAGE_SECTION,
        COLLECT_LINKAGE_SECTION,
        COLLECT_PROCEDURE_DIVISION
    }
    class Program
    {
        static IConfigurationRoot config;
        static Program()
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var builder = new ConfigurationBuilder()
                .AddJsonFile($"appsettings.json", true, true)
                .AddJsonFile($"appsettings.{env}.json", true, true)
                .AddEnvironmentVariables();
            config = builder.Build();
        }
        private static readonly string FileName = "sc700.cbl";
        private static readonly string NameSpace = "OSS_Domain";
     
        //private static readonly string FileName = "DEMO.cbl";
        //private static readonly string FileName = "small.cbl";
        private static int BlockCount = 0;
        #region Regex        
        private static Regex RegexCOMMENT = new Regex(@"^\*");
        private static string StringRegexStatement = "(MOVE|IF|ELSE[ ]+IF|END-IF|PERFORM|ELSE|DISPLAY|ADD|SUBTRACT|COMPUTE|CALL|DIVIDE|MULTIPLY|GO[ ]+TO|EXIT[ ]+PROGRAM|END[ ]+PROGRAM)";
        private static Regex RegexStatement = new Regex($"^{StringRegexStatement}");
        private static Regex RegexContainsStatement = new Regex($"{StringRegexStatement}");
        private static readonly Regex ParagraphRegex = new Regex(@"^[a-zA-Z0-9-_]+\.$");
        #endregion

        
        static void Main(string[] args)
        {           
            DateTime SD = DateTime.Now;
            Console.WriteLine("Start Processing...");
            Parse(FileName);
            Console.WriteLine($"Time Elapsed: {DateTime.Now.Subtract(SD).TotalMilliseconds} Milliseconds");
            Console.ReadLine();
        }
        private static string RemoveNumericsAtStart(string s)
        {
            Regex rgx = new Regex(@"^\d+");
            return rgx.Replace(s, string.Empty).Trim();
        }
        private static void SetBlocks(Paragraph Paragraph)
        {
            for (int i = 0; i < Paragraph.Statements.Count; i++)
            {
                var Statement = Paragraph.Statements[i];                
                switch (Statement.StatementType)
                {
                    case StatementType.IF:
                        // add open below
                        Paragraph.Statements.Insert(i + 1, new Statement()
                        {
                            Raw = String.Empty,
                            StatementType = StatementType.BEGIN_BLOCK
                        });
                        i++;
                        BlockCount++;
                        break;                    
                    case StatementType.ELSE:
                    case StatementType.ELSE_IF:
                        Paragraph.Statements.Insert(i, new Statement()
                        {
                            Raw = String.Empty,
                            StatementType = StatementType.END_BLOCK
                        });
                        i++;
                        Paragraph.Statements.Insert(i + 1, new Statement()
                        {
                            Raw = String.Empty,
                            StatementType = StatementType.BEGIN_BLOCK
                        });
                        i++;
                        break;
                    case StatementType.END_IF:
                        Statement.StatementType = StatementType.END_BLOCK;
                        // replace current to close
                        BlockCount--;
                        break;
                    default:
                        if (Statement.Raw.Trim().EndsWith('.'))
                        {   
                            int counter = 0;
                            while (BlockCount > 0)
                            {
                                Paragraph.Statements.Insert(i + 1 + counter, new Statement()
                                {
                                    Raw = String.Empty,
                                    StatementType = StatementType.END_BLOCK
                                });
                                // add close below
                                BlockCount--;
                                counter++;
                            }

                        }
                        break;
                }
            }
        }
        private static void ConvertParagraphs(List<Paragraph> Paragraphs)
        {
            foreach (var Paragraph in Paragraphs)
            {
                SetBlocks(Paragraph);
            }
            string ClassName = FileName.Replace(".cbl", string.Empty);
            using (StreamWriter CodeWriter = new StreamWriter($"{ClassName}.cs"))
            {
                CodeWriter.WriteLine($"using System;");
                CodeWriter.WriteLine($"using System.Collections.Generic;");
                CodeWriter.WriteLine($"using System.Linq;");
                CodeWriter.WriteLine($"using System.Text;");
                CodeWriter.WriteLine($"using System.Threading.Tasks;");
                CodeWriter.WriteLine($"namespace {NameSpace}");
                CodeWriter.WriteLine("{");
                CodeWriter.WriteLine($"    public class {ClassName} : {ClassName}Variables {{");                                
                using (StreamWriter LogWriter = new StreamWriter("compare-result.log"))
                {
                    for (int i = 0; i < Paragraphs.Count; i++)
                    {
                        var Paragraph = Paragraphs[i];
                        CodeWriter.WriteLine($"        public bool {NamingConverter.Convert(Paragraph.Name)}(bool ReturnBack)");
                        CodeWriter.WriteLine($"        {{");
                        bool LastStatementIsReturn = false;
                        ConvertParagraph(Paragraph, LogWriter,CodeWriter,out LastStatementIsReturn);
                        if (!LastStatementIsReturn)
                        {
                            if (i + 1 < Paragraphs.Count)
                                CodeWriter.WriteLine($"            return ReturnBack && {NamingConverter.Convert(Paragraphs[i + 1].Name)}(true);");
                            else
                                CodeWriter.WriteLine($"            return ReturnBack;");
                        }
                        CodeWriter.WriteLine($"        }}");
                    }                    
                }
                CodeWriter.WriteLine($"    }}");
                CodeWriter.WriteLine($"}}");
            }
        }
        private static void ConvertParagraph(Paragraph Paragraph, StreamWriter LogWriter, StreamWriter CodeWriter, out bool LastStatementIsReturn)
        {
            int TAP_Level = 3;
            string TAP = "    ";
            //StatementType[] SupportedTypes = new StatementType[] { StatementType.MOVE, StatementType.BEGIN_BLOCK, StatementType.COMMENT, StatementType.ELSE, StatementType.ELSE_IF, StatementType.IF, StatementType.QUERY, StatementType.ADD, StatementType.SUBTRACT,StatementType.MULTIPLY, StatementType.MULTIPLY,StatementType.DISPLAY, StatementType.CALL, StatementType. };
            Statement LastStatement = null;
            foreach (var Statement in Paragraph.Statements)
            {
                
                if (!string.IsNullOrEmpty(Statement.Converted.Trim()))
                {
                    if (Statement.StatementType == StatementType.END_BLOCK)
                        TAP_Level--;
                    StringBuilder SB = new StringBuilder();
                    string TAPSPACES = string.Empty;
                    for (int i = 0; i < TAP_Level; i++)
                    {
                        TAPSPACES += TAP;
                    }


                    SB.Append(Statement.Converted);
                    if (Statement.StatementType == StatementType.BEGIN_BLOCK)
                        TAP_Level++;

                    CodeWriter.WriteLine($"{TAPSPACES}{SB.ToString().Replace("\r\n", $"\r\n{TAPSPACES}")}");

                    LogWriter.WriteLine("*************************************************************************");
                    LogWriter.WriteLine("Raw:");
                    LogWriter.WriteLine(Statement.Raw);
                    LogWriter.WriteLine("-------------------------------------------------------------------------");
                    LogWriter.WriteLine("Converted:");
                    LogWriter.WriteLine(Statement.Converted);
                    LastStatement = Statement;
                }
            }

            LastStatementIsReturn = LastStatement != null && LastStatement.Converted.Trim().StartsWith("return");
        }
        
        private static int GetLevel(string s)
        {
            return int.Parse(new Regex(@"\d+[ ]+[a-zA-Z]+").Matches($" {s}").First().Value.Split(' ',StringSplitOptions.RemoveEmptyEntries).First());
        }
        private static void WriteAllVariables(List<CobolVariable> WORKING_STORAGE_VARIABLES, List<CobolVariable> LINKAGE_SECTION_VARIABLES)
        {
            string ClassName = FileName.Replace(".cbl", string.Empty);
            using (StreamWriter CodeWriter = new StreamWriter($"{ClassName}Variables.cs"))
            {
                CodeWriter.WriteLine($"using System;");
                CodeWriter.WriteLine($"using System.Collections.Generic;");
                CodeWriter.WriteLine($"using System.Linq;");
                CodeWriter.WriteLine($"using System.Text;");
                CodeWriter.WriteLine($"using System.Threading.Tasks;");
                CodeWriter.WriteLine($"namespace {NameSpace}");
                CodeWriter.WriteLine("{");
                CodeWriter.WriteLine($"    public class {ClassName}Variables : BaseBusiness {{");
                WriterVariables(WORKING_STORAGE_VARIABLES, CodeWriter);
                WriterVariables(LINKAGE_SECTION_VARIABLES, CodeWriter);
                CodeWriter.Write($"        }}");
                CodeWriter.Write($"    }}");
            }
           
        }
        private static void WriterVariables(List<CobolVariable> Variables, StreamWriter CodeWriter)
        {
            foreach (var Variable in Variables)
            {
                CodeWriter.WriteLine(Variable.ToString());

                if(Variable.Childs.Count>0)
                    WriterVariables(Variable.Childs, CodeWriter);                
            }
            
        }


        private static void Parse(string FilePath)
        {

            StringBuilder SBStatement = new StringBuilder();
            List<string> Lines = File.ReadAllLines(FilePath).ToList();
            List<Paragraph> Paragraphs = new List<Paragraph>();
            bool CollectSQL = false;           
            int RowNum = 0;
            int StatementRowNum = 0;
            string Line = string.Empty;
            int addedLines = 0;
            ParseMode ParseMode = ParseMode.NONE;

            CobolVariable WORKING_STORAGE_VARIABLE = new CobolVariable();
            CobolVariable CurrentVariable = new CobolVariable();
            CobolVariable LINKAGE_SECTION_VARIABLE = new CobolVariable();
            int? PreLevel = null;
            int? Level = null;
            int? ActualLevel = null;
            for (int i = 0; i < Lines.Count; i++)
            {               
                Line = Lines[i];
                if (!string.IsNullOrEmpty(Line))
                {
                    if (Line.Contains("WORKING-STORAGE SECTION."))
                    {
                        ParseMode = ParseMode.COLLECT_WORKING_STORAGE_SECTION;
                        PreLevel = null;
                        continue;
                    }
                    else if(Line.Contains("LINKAGE SECTION."))
                    {
                        ParseMode = ParseMode.COLLECT_LINKAGE_SECTION;
                        PreLevel = null;
                        continue;
                    }
                    else if (Line.Contains("PROCEDURE DIVISION"))
                    {
                        
                        if(string.IsNullOrEmpty(config["ProcedureDivisionEntryParagraph"]))
                            ParseMode = ParseMode.COLLECT_PROCEDURE_DIVISION;
                        else
                            ParseMode = ParseMode.NONE;
                        continue;
                    }
                    else if (!string.IsNullOrEmpty(config["ProcedureDivisionEntryParagraph"]) && Line.Contains(config["ProcedureDivisionEntryParagraph"]))
                    {
                        ParseMode = ParseMode.COLLECT_PROCEDURE_DIVISION;
                    }
                    switch (ParseMode)
                    {         
                        case CobolToCSharp.ParseMode.COLLECT_WORKING_STORAGE_SECTION:
                        case CobolToCSharp.ParseMode.COLLECT_LINKAGE_SECTION:
                            if (!string.IsNullOrEmpty(Line.Trim()))
                            {                               
                                if (CollectSQL)
                                {
                                    if (new Regex("END-EXEC").IsMatch(Line))
                                    {
                                        CollectSQL = false;
                                        continue;
                                    }
                                    continue;
                                }
                                if(new Regex("EXEC SQL.+END-EXEC").IsMatch(Line))
                                {
                                    continue;
                                }
                                else if(new Regex("EXEC SQL").IsMatch(Line))
                                {
                                    CollectSQL = true;
                                    continue;
                                }

                                if (RegexCOMMENT.IsMatch(Line)) continue;

                                if (SBStatement.Length > 0)
                                    SBStatement.Append(" ");
                                SBStatement.Append(Line);
                                if (SBStatement.ToString().Trim().EndsWith("."))
                                {
                                    
                                    Level = GetLevel(SBStatement.ToString());
                                    if (Level != 66 && Level != 77 && Level != 88)
                                    {
                                        if (PreLevel == null)
                                        {
                                            ActualLevel = 1;
                                            switch (ParseMode)
                                            {
                                                case CobolToCSharp.ParseMode.COLLECT_WORKING_STORAGE_SECTION:
                                                    CurrentVariable = WORKING_STORAGE_VARIABLE;
                                                    break;
                                                case CobolToCSharp.ParseMode.COLLECT_LINKAGE_SECTION:
                                                    CurrentVariable = LINKAGE_SECTION_VARIABLE;
                                                    break;                                                
                                            }                                            
                                        }
                                        else if (Level > PreLevel)
                                        {
                                            ActualLevel++;
                                            CurrentVariable = CurrentVariable.Childs.Last();
                                        }
                                        else if (Level < PreLevel)
                                        {
                                            ActualLevel--;
                                            CurrentVariable = CurrentVariable.Parent;
                                        }

                                        PreLevel = Level;
                                    }
                                    CurrentVariable.Childs.Add(new CobolVariable()
                                    {
                                        Parent = CurrentVariable,
                                        Raw = SBStatement.ToString(),
                                        Level = ActualLevel.Value,
                                        RowNumber = RowNum-1
                                    });
                                    SBStatement = new StringBuilder();
                                }
                            }


                            int x = 10;
                            break;                      
                        case CobolToCSharp.ParseMode.COLLECT_PROCEDURE_DIVISION:
                            Line = RemoveNumericsAtStart(Line);
                            if (CollectSQL)
                            {
                                SBStatement.Append(" ");
                                SBStatement.Append(Line);
                                if (Line.Equals("END-EXEC."))
                                {
                                    CollectSQL = false;
                                }
                                continue;
                            }
                            MatchCollection Collection = RegexContainsStatement.Matches(Line);
                            if (Collection.Count > 1)
                            {
                                char PreChar = Line[Collection[1].Index - 1];
                                char PostChar = Collection[1].Index + Collection[1].Length < Line.Length ? Line[Collection[1].Index + Collection[1].Length] : '_';
                                if ((PreChar == ' ' || PreChar == '.') && PostChar == ' ')
                                {
                                    Line = Lines[i].Substring(0, Collection[1].Index).Trim();
                                    Lines.Insert(i + 1, Lines[i].Substring(Collection[1].Index).Trim());
                                    Lines[i] = Line;
                                    addedLines++;
                                }


                            }
                            if (ParagraphRegex.IsMatch(Line))
                            {
                                if (Paragraphs.Count > 0)
                                    Paragraphs.Last().AddStatement(SBStatement.ToString(), StatementRowNum - addedLines);
                                StatementRowNum = RowNum;
                                SBStatement = new StringBuilder();
                                Paragraphs.Add(new Paragraph()
                                {
                                    Name = Line,
                                    Paragraphs = Paragraphs
                                });
                                continue;
                            }
                            else if (Paragraph.RegexEXECSQL.IsMatch(Line))
                            {
                                Paragraphs.Last().AddStatement(SBStatement.ToString(), StatementRowNum - addedLines);
                                CollectSQL = true;
                                StatementRowNum = RowNum;
                                SBStatement = new StringBuilder();
                                SBStatement.Append(Line);
                            }
                            else if (RegexCOMMENT.IsMatch(Line))
                            {
                                Paragraphs.Last().AddStatement(SBStatement.ToString(), RowNum - addedLines);
                            }
                            else if (RegexStatement.IsMatch(Line))
                            {
                                if (Paragraph.RegexELSE.IsMatch(SBStatement.ToString()) && Paragraph.RegexIF.IsMatch(Line))
                                {
                                    SBStatement.Append(" ");
                                    SBStatement.Append(Line);
                                }
                                else
                                {
                                    Paragraphs.Last().AddStatement(SBStatement.ToString(), StatementRowNum - addedLines);
                                    StatementRowNum = RowNum;
                                    SBStatement = new StringBuilder(Line);
                                }

                            }
                            else
                            {
                                SBStatement.Append(" ");
                                SBStatement.Append(Line);
                            }
                            break;                        
                    }                   
                }
            }           
           

                if(Paragraphs.Count>0)
                    Paragraphs.Last().AddStatement(SBStatement.ToString(), StatementRowNum- addedLines);

            WriteAllVariables(WORKING_STORAGE_VARIABLE.Childs,LINKAGE_SECTION_VARIABLE.Childs);

            ConvertParagraphs(Paragraphs);

        }
    }
}
