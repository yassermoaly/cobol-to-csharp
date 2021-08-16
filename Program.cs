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
        //private static readonly string FileName = "sc700.cbl";
        private static readonly string FileName = "small.cbl";

        static int numberOfConvertedLines = 0;
        #region Regex        
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
            numberOfConvertedLines = Paragraphs.Count;
            foreach (var Paragraph in Paragraphs)
            {
                ProcessParagraph(++i,Paragraph);
            }

            int x = 10;
        }
        private static void ProcessParagraph(int Index,Paragraph Paragraph)
        {
            //StringBuilder CSV = new StringBuilder();
            //CSV.AppendLine("COBOL,C#");
            foreach (var Statement in Paragraph.Statements)
            {
                if(Statement.StatementType == StatementType.GOTO)
                {
                    //CSV.AppendLine($"{Statement.Raw},{Statement.Converted}");
                    string C = Statement.Converted;
                    numberOfConvertedLines++;
                }           
            }                  
            //if (!Directory.Exists("MoveStatements"))
            //    Directory.CreateDirectory("MoveStatements");
            //using (StreamWriter Writer=new StreamWriter(@$"MoveStatements\{Index.ToString().PadLeft(3,'0')}-{Paragraph.Name}-MoveStatements.csv"))
            //{
            //    Writer.Write(CSV);
            //}
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
                            SBStatement.Append(" ");
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
                                Name = Line,
                                Paragraphs = Paragraphs
                            });
                            continue;
                        }
                        else if (Line.Equals("EXEC SQL"))
                        {                                                  
                            Paragraphs.Last().AddStatement(SBStatement.ToString(), StatementRowNum);
                            CollectSQL = true;
                            StatementRowNum = RowNum;
                            SBStatement = new StringBuilder();
                            SBStatement.Append(Line);                            
                        }
                        else if (RegexCOMMENT.IsMatch(Line))
                        {
                            Paragraphs.Last().AddStatement(SBStatement.ToString(), RowNum);                            
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

            if (Paragraph.GetStatementType(SBStatement.ToString()) != StatementType.QUERY)
            {
                Paragraphs.Last().AddStatement(SBStatement.ToString(), StatementRowNum);
            }
            ProcessParagraphs(Paragraphs);

        }       
    }
}
