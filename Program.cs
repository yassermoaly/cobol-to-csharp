using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace CobolToCSharp
{
    class Program
    {
        private static readonly string FileName = "sc700.cbl";
        private static readonly string NameSpace = "OSS_Domain";

        //private static readonly string FileName = "DEMO.cbl";
        //private static readonly string FileName = "small.cbl";
        private static int BlockCount = 0;
        static int numberOfConvertedLines = 0;
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
        private static void SetBlock(Paragraph Paragraph)
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
                        if (Statement.Raw.EndsWith('.'))
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
                SetBlock(Paragraph);
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
                CodeWriter.WriteLine($"    public class {ClassName} : BaseBusiness {{");                                
                using (StreamWriter LogWriter = new StreamWriter("compare-result.log"))
                {
                    for (int i = 0; i < Paragraphs.Count; i++)
                    {
                        var Paragraph = Paragraphs[i];
                        CodeWriter.WriteLine($"        public bool {NamingConverter.Convert(Paragraph.Name)}(bool ReturnBack){{");
                        ConvertParagraph(Paragraph, LogWriter,CodeWriter);
                        if(i+1< Paragraphs.Count)
                            CodeWriter.WriteLine($"            return ReturnBack && {NamingConverter.Convert(Paragraphs[i+1].Name)}(true);");
                        else
                            CodeWriter.WriteLine($"            return ReturnBack;");
                        CodeWriter.WriteLine($"        }}");
                    }                    
                }
                CodeWriter.Write($"        }}");
                CodeWriter.Write($"    }}");
            }
        }
        private static void ConvertParagraph(Paragraph Paragraph,StreamWriter LogWriter, StreamWriter CodeWriter)
        {
            int TAP_Level = 3;
            string TAP = "    ";
            //StatementType[] SupportedTypes = new StatementType[] { StatementType.MOVE, StatementType.BEGIN_BLOCK, StatementType.COMMENT, StatementType.ELSE, StatementType.ELSE_IF, StatementType.IF, StatementType.QUERY, StatementType.ADD, StatementType.SUBTRACT,StatementType.MULTIPLY, StatementType.MULTIPLY,StatementType.DISPLAY, StatementType.CALL, StatementType. };
            foreach (var Statement in Paragraph.Statements)
            {
                
                if (!string.IsNullOrEmpty(Statement.Converted))
                {
                    if (Statement.StatementType == StatementType.END_BLOCK)
                        TAP_Level--;
                    StringBuilder SB = new StringBuilder();
                    string TAPSPACES = string.Empty;
                    for (int i = 0; i < TAP_Level; i++)
                    {
                        TAPSPACES += "    ";
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
                }
            }         
        }
        
        
        private static void Parse(string FilePath)
        {
            StringBuilder SBStatement = new StringBuilder();
            List<string> Lines = File.ReadAllLines(FilePath).Select(r => RemoveNumericsAtStart(r)).ToList();
            List<Paragraph> Paragraphs = new List<Paragraph>();
            bool CollectSQL = false;
            bool StartParse = false;
            int RowNum = 0;
            int StatementRowNum = 0;
            string Line = string.Empty;
            int addedLines = 0;
            for (int i = 0; i < Lines.Count; i++)
            {

                
                Line = Lines[i];

                RowNum++;
                if (!string.IsNullOrEmpty(Line))
                {
                    if (Line.Equals("P-INITIAL."))
                    {
                        StartParse = true;
                    }
                    if (StartParse)
                    {
                        
                        
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
                            char PostChar = Collection[1].Index + Collection[1].Length<Line.Length?Line[Collection[1].Index + Collection[1].Length]:'_';
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
                        else if (Line.Equals("EXEC SQL"))
                        {
                            Paragraphs.Last().AddStatement(SBStatement.ToString(), StatementRowNum- addedLines);
                            CollectSQL = true;
                            StatementRowNum = RowNum;
                            SBStatement = new StringBuilder();
                            SBStatement.Append(Line);
                        }
                        else if (RegexCOMMENT.IsMatch(Line))
                        {
                            Paragraphs.Last().AddStatement(SBStatement.ToString(), RowNum- addedLines);
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
                                Paragraphs.Last().AddStatement(SBStatement.ToString(), StatementRowNum- addedLines);
                                StatementRowNum = RowNum;
                                SBStatement = new StringBuilder(Line);
                            }
                            
                        }
                        else
                        {
                            SBStatement.Append(" ");
                            SBStatement.Append(Line);
                        }
                    }
                }
            }

            Paragraphs.Last().AddStatement(SBStatement.ToString(), StatementRowNum- addedLines);
           
            ConvertParagraphs(Paragraphs);

        }       
    }
}
