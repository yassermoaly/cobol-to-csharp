using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace CobolParser
{
    class Program
    {
        private static readonly string FileName = "sc700.cbl";

        #region Regex
        private static Regex RegexGOTO = new Regex("^GO[ ]+TO");
        private static Regex RegexEXECSQL = new Regex("^EXEC[ ]+SQL");
        private static Regex RegexEXITPROGRAM = new Regex("^EXIT[ ]+PROGRAM");
        private static Regex RegexMOVE = new Regex("^MOVE");
        private static Regex RegexIF = new Regex("^IF");
        private static Regex RegexPERFORM = new Regex("^PERFORM");
        private static Regex RegexELSE = new Regex("^ELSE");
        private static Regex RegexDISPLAY = new Regex("^DISPLAY");
        private static Regex RegexADD = new Regex("^ADD");
        private static Regex RegexSUBTRACT = new Regex("^SUBTRACT");
        private static Regex RegexCOMPUTE = new Regex("^COMPUTE");
        private static Regex RegexDIVIDE = new Regex("^DIVIDE");
        private static Regex RegexMULTIPLY = new Regex("^MULTIPLY");
        private static Regex RegexCALL = new Regex("^CALL");
        private static Regex RegexCOMMENT = new Regex(@"^\*");
        private static Regex RegexStatement = new Regex("^(MOVE|IF|PERFORM|ELSE|DISPLAY|ADD|SUBTRACT|COMPUTE|CALL|DIVIDE|MULTIPLY|GO[ ]+TO|EXIT[ ]+PROGRAM)");
        private static readonly Regex ParagraphRegex = new Regex(@"^[a-zA-Z0-9-_]+\.$");
        #endregion

        static void Main(string[] args)
        {
          
            Console.WriteLine("Start Processing...");
            Process(FileName);           
        }
        private static string RemoveNumericsAtStart(string s)
        {
            Regex rgx = new Regex(@"^\d+");
            return rgx.Replace(s, string.Empty).Trim();
        }
        private static void ProcessParagraphs(List<Paragraph> Paragraphs)
        {
            int i = 0;
            foreach (var Paragraph in Paragraphs)
            {
                ProcessParagraph(++i,Paragraph);
            }
        }
        private static void ProcessParagraph(int Index,Paragraph Paragraph)
        {
            StringBuilder CSV = new StringBuilder();
            CSV.AppendLine("COBOL,C#");
            foreach (var Statement in Paragraph.Statements)
            {
                if(Statement.StatementType == StatementType.MOVE)
                {
                    CSV.AppendLine($"{Statement.Raw},{Statement.Converted}");
                    string C = Statement.Converted;
                }

            }
            if (!Directory.Exists("MoveStatements"))
                Directory.CreateDirectory("MoveStatements");
            using (StreamWriter Writer=new StreamWriter(@$"MoveStatements\{Index.ToString().PadLeft(3,'0')}-{Paragraph.Name}-MoveStatements.csv"))
            {
                Writer.Write(CSV);
            }
        }
        
        
        private static void Process(string FilePath)
        {
            StringBuilder SBStatement = new StringBuilder();
            string[] Lines = File.ReadAllLines(FilePath).Select(r => RemoveNumericsAtStart(r)).ToArray();
            List<Paragraph> Paragraphs = new List<Paragraph>();
            bool CollectSQL = false;
            bool StartParse = false;
            int RowNum = 0;
            int StatementRowNum = 0;
            foreach (var Line in Lines)
            {
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
                            SBStatement.Append("");
                            SBStatement.Append(Line);
                            if (Line.Equals("END-EXEC."))
                            {
                                Paragraphs.Last().AddStatement(SBStatement.ToString(), StatementRowNum);                               
                                CollectSQL = false;
                            }
                            continue;
                        }

                        if (ParagraphRegex.IsMatch(Line))
                        {
                            Paragraphs.Add(new Paragraph()
                            {
                                Name = Line
                            });
                            continue;
                        }
                        else if (Line.Equals("EXEC SQL"))
                        {
                            CollectSQL = true;
                            StatementRowNum = RowNum;
                            SBStatement = new StringBuilder();
                            SBStatement.Append(Line);                            
                        }
                        else if (RegexCOMMENT.IsMatch(Line))
                        {
                            Paragraphs.Last().Statements.Add(new Statement()
                            {
                                Raw = Line,
                                RowNo = RowNum,
                                StatementType = StatementType.COMMENT
                            });
                        }
                        else if (RegexStatement.IsMatch(Line))
                        {
                            Paragraphs.Last().AddStatement(SBStatement.ToString(), StatementRowNum);
                            StatementRowNum = RowNum;
                            SBStatement = new StringBuilder(Line);
                        }
                        else
                        {
                            SBStatement.Append(" ");
                            SBStatement.Append(Line);
                        }
                    }
                }               
            }
            ProcessParagraphs(Paragraphs);

        }

        //private static void Process(string FilePath)
        //{
        //    string[] PredefinedParagraphs = new string[] { "EXEC SQL", "END-EXEC." };
        //    List<string> SQLQueries = new List<string>();           
        //    List<string> ProcedureParagraphs = new List<string>();
        //    StringBuilder SBQuery = new StringBuilder();
        //    StringBuilder SBStatement = new StringBuilder();
        //    string[] Lines = File.ReadAllLines(FilePath).Select(r=> RemoveNumericsAtStart(r)).ToArray();
        //    List<Paragraph> Paragraphs = new List<Paragraph>();
        //    bool CollectSQL = false;
        //    bool StartParse = false;
        //    foreach (var Line in Lines.Where(L=>!string.IsNullOrEmpty(L)))
        //    {
        //        if (Line.Equals("P-INITIAL."))
        //        {
        //            StartParse = true;
        //           // continue;
        //        }
        //        if (StartParse)
        //        {
        //            if (CollectSQL)
        //            {
        //                if (SBQuery.Length > 0)
        //                    SBQuery.Append(" ");

        //                if (Line.Equals("END-EXEC."))
        //                {
        //                    CollectSQL = false;
        //                    SQLQueries.Add(SBQuery.ToString());
        //                }
        //                SBQuery.Append(Line);
        //            }


        //            if (Line.Equals("EXEC SQL"))
        //            {
        //                CollectSQL = true;
        //                SBQuery = new StringBuilder();
        //            }



        //            if ( !PredefinedParagraphs.Contains(Line) && ParagraphRegex.IsMatch(Line))
        //            {
        //                Paragraphs.Add(new Paragraph()
        //                {
        //                    Name = Line
        //                });
        //                continue;

        //            }


        //            Paragraphs.Last().Lines.Add(Line);

        //        }
        //    }
        //    ProcessParagraphs(Paragraphs);

        //}
    }
}
