using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using CobolToCSharp.Screen;

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
        
       private static readonly string WorkingDir = @"input";
       // private static readonly string FileName = "sc700.cbl";
        //private static readonly string FileName = "sc499.cbl";
        private static readonly string FileName = "sc500.cbl";
        //private static readonly string FileName = "DEMO.cbl";
        private static readonly string NameSpace = "OSS_Domain";
       //private static readonly string FileName = "DEMO.cbl";
        //private static readonly string FileName = "small.cbl";
       private static int BlockCount = 0;
       #region Regex        
        private static Regex RegexCOMMENT = new Regex(@"^\*");
        private static string StringRegexStatement = @"(MOVE|IF|ELSE[ ]+IF|END-IF|PERFORM|ELSE|DISPLAY|ADD|SUBTRACT|COMPUTE|CALL|DIVIDE|MULTIPLY|GO[ ]+TO|GO[ ]+|EXIT[ ]*\.|EXIT[ ]+PROGRAM|END[ ]+PROGRAM|STOP[ ]+RUN)".RegexUpperLower();
        private static Regex RegexStatement = new Regex($"^{StringRegexStatement}");
        private static Regex RegexContainsStatement = new Regex($"{StringRegexStatement}");
        private static readonly Regex ParagraphRegex = new Regex(@"^[a-zA-Z0-9-_]+\.$");
        #endregion

        private static CobolVariable WORKING_STORAGE_VARIABLE = new CobolVariable();
        private static CobolVariable LINKAGE_SECTION_VARIABLE = new CobolVariable();


        static CobolVariable FindCobolVariableByName(string Name, List<CobolVariable> CobolVariables)
        {
            foreach (var CobolVariable in CobolVariables)
            {
                if (CobolVariable.RawName == Name)
                    return CobolVariable;
                if (CobolVariable.Childs.Count > 0)
                {
                    var ChildResult = FindCobolVariableByName(Name, CobolVariable.Childs);
                    if (ChildResult != null)
                    {
                        return ChildResult;
                    }
                }
            }
            return null;
        }
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

        private static void ConvertParagraphs(List<Paragraph> Paragraphs, Dictionary<string, string> DataTypes)
        {
            string ClassName = FileName.Replace(".cbl", string.Empty);
            if (!Directory.Exists($@"{WorkingDir}\{ClassName}"))
            {
                Directory.CreateDirectory($@"{WorkingDir}\{ClassName}");
            }
            List<CobolVariable> CobolVariables = new List<CobolVariable>(WORKING_STORAGE_VARIABLE.Childs.ToArray());
            CobolVariables.AddRange(LINKAGE_SECTION_VARIABLE.Childs);
            
            foreach (var Paragraph in Paragraphs)
            {
                SetBlocks(Paragraph);               
            }            
            using (StreamWriter CodeWriter = new StreamWriter($@"{WorkingDir}\{ClassName}\{ClassName}.cs"))
            {
                CodeWriter.WriteLine($"using System;");
                CodeWriter.WriteLine($"using System.Collections.Generic;");
                CodeWriter.WriteLine($"using System.Linq;");
                CodeWriter.WriteLine($"using System.Text;");
                CodeWriter.WriteLine($"using System.Threading.Tasks;");
                CodeWriter.WriteLine($"namespace {NameSpace}");
                CodeWriter.WriteLine("{");
                CodeWriter.WriteLine($"    public partial class {ClassName} : IService {{");                                
                using (StreamWriter LogWriter = new StreamWriter($@"{WorkingDir}\compare-result.log"))
                {
                    CodeWriter.WriteLine($"        public string Name {{get{{return \"{ClassName}\";}}}}");
                    CodeWriter.WriteLine($"        public virtual void Run()");
                    CodeWriter.WriteLine($"        {{");
                    CodeWriter.WriteLine($"            {NamingConverter.Convert(Paragraphs.First().Name)}(true,null);");                    
                    CodeWriter.WriteLine($"        }}");
                    for (int i = 0; i < Paragraphs.Count; i++)
                    {
                        var Paragraph = Paragraphs[i];
                        CodeWriter.WriteLine($"        private List<Stack> {NamingConverter.Convert(Paragraph.Name)}(bool CallNext,string[] NextScope)");
                        CodeWriter.WriteLine($"        {{");
                        bool LastStatementIsReturn = false;
                        CodeWriter.WriteLine($"             List<Stack> FullStack = Stack.New(CallNext, \"{NamingConverter.Convert(Paragraph.Name)}\");");
                        ConvertParagraph(Paragraph, LogWriter,CodeWriter, DataTypes, out LastStatementIsReturn);
                        if (!LastStatementIsReturn)
                        {
                            if (i + 1 < Paragraphs.Count)
                            {
                                CodeWriter.WriteLine($"            if (CheckCallNext(CallNext, NextScope, \"{NamingConverter.Convert(Paragraphs[i + 1].Name)}\"))");
                                CodeWriter.WriteLine($"                 FullStack.AddRange({NamingConverter.Convert(Paragraphs[i + 1].Name)}(true, NextScope));");
                                CodeWriter.WriteLine($"            return FullStack;");
                            }
                                
                            else
                                CodeWriter.WriteLine($"            return FullStack;");
                        }
                        CodeWriter.WriteLine($"        }}");
                    }                    
                }
                CodeWriter.WriteLine($"    }}");
                CodeWriter.WriteLine($"}}");
            }
        }
        private static void ConvertParagraph(Paragraph Paragraph, StreamWriter LogWriter, StreamWriter CodeWriter, Dictionary<string, string> DataTypes, out bool LastStatementIsReturn)
        {
            int TAP_Level = 3;
            string TAP = "    ";
            //StatementType[] SupportedTypes = new StatementType[] { StatementType.MOVE, StatementType.BEGIN_BLOCK, StatementType.COMMENT, StatementType.ELSE, StatementType.ELSE_IF, StatementType.IF, StatementType.QUERY, StatementType.ADD, StatementType.SUBTRACT,StatementType.MULTIPLY, StatementType.MULTIPLY,StatementType.DISPLAY, StatementType.CALL, StatementType. };
            Statement LastStatement = null;
            foreach (var Statement in Paragraph.Statements)
            {
                Statement.CobolVariablesDataTypes = DataTypes;

                if(Statement.Converted.Equals("SQLCODE = GO = UPDT_TAB29 = ZERO;"))
                {
                    string asd = Statement.Converted;
                }

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


                    //CodeWriter.WriteLine($"{TAPSPACES}//******************************************************");
                    //CodeWriter.WriteLine($"{TAPSPACES}//{Statement.Raw}");
                    //CodeWriter.WriteLine($"{TAPSPACES}//******************************************************");
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

            LastStatementIsReturn = LastStatement!=null && LastStatement.StatementType == StatementType.GOTO;
          
        }
        
        private static int GetLevel(string s)
        {            
            return int.Parse(new Regex(@"\d+[ ]+[a-zA-Z]+").Matches($" {s.Replace("/",string.Empty)}").First().Value.Split(' ',StringSplitOptions.RemoveEmptyEntries).First());
        }
        private static Dictionary<string,string> WriteAllVariables(List<CobolVariable> WORKING_STORAGE_VARIABLES, List<CobolVariable> LINKAGE_SECTION_VARIABLES)
        {
            string ClassName = FileName.Replace(".cbl", string.Empty);
            if (!Directory.Exists($@"{WorkingDir}\{ClassName}"))
            {
                Directory.CreateDirectory($@"{WorkingDir}\{ClassName}");
            }

            Dictionary<string, string> DataTypes = new Dictionary<string, string>();
            Console.WriteLine("Write Variables");
           

            using (StreamWriter CodeWriter = new StreamWriter($@"{WorkingDir}\{ClassName}\{ClassName}Variables.cs"))
            {
                CodeWriter.WriteLine($"using System;");
                CodeWriter.WriteLine($"using System.Collections.Generic;");
                CodeWriter.WriteLine($"using System.Linq;");
                CodeWriter.WriteLine($"using System.Text;");
                CodeWriter.WriteLine($"using System.Threading.Tasks;");
                CodeWriter.WriteLine($"namespace {NameSpace}");
                CodeWriter.WriteLine($"{{");
                CodeWriter.WriteLine($"    public partial class {ClassName} : BaseBusiness");
                CodeWriter.WriteLine($"    {{");
                HashSet<string> ProcessedVariables = new HashSet<string>();
                DataTypes.Merge<string,string>(WriterVariables(WORKING_STORAGE_VARIABLES, CodeWriter, ProcessedVariables));
                DataTypes.Merge<string, string>(WriterVariables(LINKAGE_SECTION_VARIABLES, CodeWriter, ProcessedVariables));
                CodeWriter.WriteLine($"    }}");
                CodeWriter.WriteLine($"}}");
            }
            Console.WriteLine("Finish Write Variables");
            return DataTypes;
        }
        private static Dictionary<string,string> WriterVariables(List<CobolVariable> Variables, StreamWriter CodeWriter, HashSet<string> ProcessedVariables)
        {
            Dictionary<string, string> DataTypes = new Dictionary<string, string>();
            foreach (var Variable in Variables)
            {
                if (ProcessedVariables.Contains(Variable.RawName))
                    continue;
                string Converted = Variable.ToString();
                if (!DataTypes.ContainsKey(Variable.RawName))
                    DataTypes.Add(Variable.RawName, Variable.PropertyDataType);
                if (!string.IsNullOrEmpty(Converted))
                    CodeWriter.WriteLine(Converted);

                if (Variable.Childs.Count > 0)
                {
                    DataTypes.Merge<string, string>(WriterVariables(Variable.Childs, CodeWriter, ProcessedVariables));
                }
                ProcessedVariables.Add(Variable.RawName);
            }

            return DataTypes;


        }

       

        public static void ParseScreen(string FileName, List<CobolVariable> CobolVariables)
        {
            string ScrnFilePath = $@"{WorkingDir}\{FileName.Replace("sc", "f").Replace(".cbl", ".scrn")}";
            if (!File.Exists(ScrnFilePath)) return;
            string Text = DecodeArabic.DecodeFile(ScrnFilePath);
            List<ScreenBlock> ScreenBlocks = ScreenBlock.ExtractFromText(Text, new string[] { "RECORD", "SCREEN" });
            var Screen = ScreenBlocks.First(r => r.Type == "SCREEN");
            var ScreenVARIABLE = Screen.Childs.First(r => r.Type == "VARIABLE");

            Dictionary<int, List<ScreenElement>> ScreenElementsDict = new Dictionary<int, List<ScreenElement>>();

            var InputBindVariable = ScreenBlocks.First(r => r.Name == ScreenVARIABLE.INPUT);
            var OutBindVariable = ScreenBlocks.First(r => r.Name == ScreenVARIABLE.OUTPUT);

            CobolVariable INTRAN = FindCobolVariableByName("INTRAN", CobolVariables);
            CobolVariable OUTTRAN = FindCobolVariableByName("OUTTRAN", CobolVariables);
            int index = 0;
            List<CobolVariable> INTRANBaseChilds = INTRAN.GetBaseChilds();
            foreach (var InputBindVariableChild in InputBindVariable.Childs)
            {
                if(index< INTRANBaseChilds.Count)
                    InputBindVariableChild.BindName = NamingConverter.Convert(INTRANBaseChilds[index++].RawName);
            }
            List<CobolVariable> OUTTRANBaseChilds = OUTTRAN.GetBaseChilds();
            index = 0;
            foreach (var OutBindVariableChild in OutBindVariable.Childs)
            {
                if (index < OUTTRANBaseChilds.Count)
                    OutBindVariableChild.BindName = NamingConverter.Convert(OUTTRANBaseChilds[index++].RawName);
            }


            foreach (KeyValuePair<int,double> RowMapping in Screen.RowMappings)
            {
                List<ScreenBlock> Childs = Screen.Childs.Where(r => r.Y == RowMapping.Value && r.Type == "FIELD").OrderBy(r=>r.X).ToList();
                foreach (var Child in Childs)
                {
                    if (Child.IsLabel)
                    {
                        MatchCollection Matches = new Regex("[0-9]-[^0-9]+").Matches(Child.VALUE);
                        if (Matches.Count> 0)
                        {
                            ScreenElementsDict[RowMapping.Key].Last().Options = Matches.Select(r => r.Value).ToArray();
                        }
                        else
                        {
                            if(!ScreenElementsDict.ContainsKey(RowMapping.Key))
                                ScreenElementsDict.Add(RowMapping.Key, new List<ScreenElement>());
                            ScreenElementsDict[RowMapping.Key].Add(new ScreenElement()
                            {
                                Label = Child.VALUE
                            });
                        }
                    }
                    else
                    {
                        if (!ScreenElementsDict.ContainsKey(RowMapping.Key))
                            ScreenElementsDict.Add(RowMapping.Key, new List<ScreenElement>());
                        ScreenElementsDict[RowMapping.Key].Last().ScreenBlock = Child;
                    }
                }              
            }
            StringBuilder SBOptions = new StringBuilder();
            StringBuilder SBView = new StringBuilder("[");
            StringBuilder SBInputBindings = new StringBuilder();
            StringBuilder SBOutputBindings = new StringBuilder();
            foreach (int Key in ScreenElementsDict.Keys)
            {
                
                List<ScreenElement> ScreenElements = ScreenElementsDict[Key];
                int col = (int)Math.Ceiling(12.0 / (float)ScreenElements.Count);
                if (Key > 0)
                    SBView.Append(",");
                SBView.Append("{");
                SBView.Append("\"TagName\": \"div\",");
                SBView.Append("\"Class\": \"row\",");
                SBView.Append("\"Childs\": [");
                bool IsFirst = true;
                foreach (var ScreenElement in ScreenElements)
                {
                    if (string.IsNullOrEmpty(ScreenElement.Label))
                        continue;

                    if (!IsFirst)
                        SBView.Append(",");
                    SBView.Append("{");
                    SBView.Append("\"TagName\": \"div\"");
                    SBView.Append($",\"Class\": \"col-md-{col}\"");
                    SBView.Append(",\"Childs\": [");
                    SBView.Append("{");
                    SBView.Append($"\"TagName\": \"{ScreenElement.TagName}\"");
                    SBView.Append(",\"Class\": \"form-control\"");
                    SBView.Append($",\"Label\": \"{ScreenElement.Label.Replace(char.ConvertFromUtf32(160), " ")}\"");
                    if (!string.IsNullOrEmpty(ScreenElement.Name))
                    {
                        SBView.Append($",\"Bind\": \"item.{ScreenElement.Name}\"");
                        SBView.Append($",\"Name\": \"{ScreenElement.Name}\"");
                    }
                    if (ScreenElement.TagName == "select")
                    {
                        SBView.Append($",\"Options\": \"{ScreenElement.Name}-options\"");
                        if (SBOptions.Length > 0)
                            SBOptions.Append(",");
                        SBOptions.Append($"{{\"Name\": \"{ScreenElement.Name}-options\",\"Values\": [{ScreenElement.OptionsJson}]}}");
                    }
                    if (ScreenElement.IsReadOnly)
                    {
                        SBView.Append(",\"Attributes\": {");
                        SBView.Append("\"readonly\": \"readonly\"");
                        SBView.Append("}");
                    }
                    SBView.Append("}");
                    SBView.Append("]");
                    SBView.Append("}");
                    if (IsFirst)
                    {
                        IsFirst = false;
                    }             
                    
                    if(ScreenElement.ScreenBlock!=null)
                    {
                        if (!string.IsNullOrEmpty(ScreenElement.ScreenBlock.INPUT))
                        {                           
                            var BindVariable = InputBindVariable.Childs.First(r => r.Name == ScreenElement.ScreenBlock.INPUT);
                            if (!string.IsNullOrEmpty(BindVariable.BindName))
                            {
                                if (SBInputBindings.Length > 0)
                                    SBInputBindings.Append(",");
                                SBInputBindings.Append($"{{\"Bind\": \"{ScreenElement.ScreenBlock.Name}\",\"MapProperty\": \"{BindVariable.BindName}\"}}");
                            }
                        }
                        if (!string.IsNullOrEmpty(ScreenElement.ScreenBlock.OUTPUT))
                        {                           
                            var BindVariable = OutBindVariable.Childs.First(r => r.Name == ScreenElement.ScreenBlock.OUTPUT);
                            if (!string.IsNullOrEmpty(BindVariable.BindName))
                            {
                                if (SBOutputBindings.Length > 0)
                                    SBOutputBindings.Append(",");
                                SBOutputBindings.Append($"{{\"Bind\": \"{ScreenElement.ScreenBlock.Name}\",\"MapProperty\": \"{BindVariable.BindName}\"}}");
                            }
                        }
                    }
                }
                SBView.Append("]");
                SBView.Append("}");
              
            }
            SBView.Append("]");

            StringBuilder SBMessages = new StringBuilder();
            string[] Messages = Screen.Childs.First(r => r.Type == "MESSAGE").Raw.Replace("\r\n", string.Empty).Split(';', StringSplitOptions.RemoveEmptyEntries).Select(r => r.Trim()).ToArray();
            foreach (var Message in Messages)
            {
                if (!string.IsNullOrEmpty(Message))
                {
                    string MessageId = Message.Substring(0, Message.IndexOf("="));
                    string MessageText = Message.Substring(Message.IndexOf("=") + 1).Replace("\"", string.Empty).Replace(char.ConvertFromUtf32(160), " ");
                    if (SBMessages.Length > 0)
                        SBMessages.Append(",");
                    SBMessages.Append($"{{\"Id\": {MessageId},\"Text\": \"{MessageText}\"}}");
                }
            }



            #region Actions
            StringBuilder SBActions = new StringBuilder();
            SBActions.Append("[");
            SBActions.Append("{");
            SBActions.Append("\"Name\": \"inquiry\",");
            SBActions.Append("\"HeaderLabel\": \"الاشنراك فى الباقات المجمعة-استعلام\",");
            SBActions.Append("\"OutputBindings\": [");
            SBActions.Append("{");
            SBActions.Append("\"Bind\": \"SAVE_AREA\",");
            SBActions.Append("\"MapProperty\": \"SAVE_AREA\",");
            SBActions.Append("\"Secure\": true");
            SBActions.Append("}");
            SBActions.Append("],");
            SBActions.Append("\"SuccessCriteria\": {");
            SBActions.Append("\"PropertyName\": \"OUT_TRIGGER\",");
            SBActions.Append("\"ExpectedValue\": 15,");
            SBActions.Append("\"StatusMessageBinding\": \"OUT_TRIGGER_MESSAGE\"");
            SBActions.Append("},");
            SBActions.Append("\"Confirm\": {");
            SBActions.Append("\"Label\": \"استعلام\",");
            SBActions.Append("\"ConfirmationRequired\": false,");
            SBActions.Append("\"ClassName\": \"btn-primary\",");
            SBActions.Append("\"Icon\": \"fas fa-search\"");
            SBActions.Append("}");
            SBActions.Append("},");
            SBActions.Append("{");
            SBActions.Append("\"Name\": \"perform\",");
            SBActions.Append("\"HeaderLabel\": \"الاشنراك فى الباقات المجمعة-تنفيذ\",");
            SBActions.Append("\"InputBindings\": [");
            SBActions.Append("{");
            SBActions.Append("\"Bind\": \"SAVE_AREA\",");
            SBActions.Append("\"MapProperty\": \"SAVE_AREA\",");
            SBActions.Append("\"Secure\": true");
            SBActions.Append("}");
            SBActions.Append("],");
            SBActions.Append("\"OutputBindings\": [");
            SBActions.Append("],");
            SBActions.Append("\"SuccessCriteria\": {");
            SBActions.Append("\"PropertyName\": \"OUT_TRIGGER\",");
            SBActions.Append("\"ExpectedValue\": 31,");
            SBActions.Append("\"StatusMessageBinding\": \"OUT_TRIGGER_MESSAGE\"");
            SBActions.Append("},");
            SBActions.Append("\"Confirm\": {");
            SBActions.Append("\"Label\": \"تنفيذ\",");
            SBActions.Append("\"ClassName\": \"btn-success\",");
            SBActions.Append("\"ConfirmationRequired\": true,");
            SBActions.Append("\"Icon\": \"fas fa-save\"");
            SBActions.Append("}");     
            SBActions.Append("}");
            SBActions.Append("]");
            #endregion;

            StringBuilder SbJson = new StringBuilder();
            SbJson.Append($"{{\"Name\":\"sc700\",\"View\":{{\"Options\":[{SBOptions.ToString()}],\"Controls\":{SBView.ToString()}}},\"InputBindings\":[{SBInputBindings.ToString()}],\"OutputBindings\":[{SBOutputBindings.ToString()}],\"Messages\":[{SBMessages.ToString()}],\"Actions\":{SBActions.ToString()} }}");
            using (StreamWriter W = new StreamWriter("view.json"))
            {
                W.Write(SbJson.ToString());
            }
            
            

        }
        private static void Parse(string FileName)
        {
            
            StringBuilder SBStatement = new StringBuilder();          
            List<string> Lines = File.ReadAllLines($@"{WorkingDir}\{FileName}").ToList();
            List<Paragraph> Paragraphs = new List<Paragraph>();
            bool CollectSQL = false;           
            int RowNum = 0;
            int StatementRowNum = 0;
            string Line = string.Empty;
            int addedLines = 0;
            ParseMode ParseMode = ParseMode.NONE;
            int IncludeCount = 0;
           
            CobolVariable CurrentVariable = new CobolVariable();
            
            int? PreLevel = null;
            int? Level = null;
            int? ActualLevel = null;
            string IncludeFilePath = string.Empty;
            for (int i = 0; i < Lines.Count; i++)
            {
                RowNum++;
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
                        
                        if(string.IsNullOrEmpty(Config.Values["ProcedureDivisionEntryParagraph"]))
                            ParseMode = ParseMode.COLLECT_PROCEDURE_DIVISION;
                        else
                            ParseMode = ParseMode.NONE;
                        continue;
                    }
                    else if (!string.IsNullOrEmpty(Config.Values["ProcedureDivisionEntryParagraph"]) && Line.Contains(Config.Values["ProcedureDivisionEntryParagraph"]))
                    {
                        ParseMode = ParseMode.COLLECT_PROCEDURE_DIVISION;
                    }
                    switch (ParseMode)
                    {         
                        case CobolToCSharp.ParseMode.COLLECT_WORKING_STORAGE_SECTION:
                        case CobolToCSharp.ParseMode.COLLECT_LINKAGE_SECTION:
                            if (!string.IsNullOrEmpty(Line.Trim()))
                            {
                               
                                if (new Regex("EXEC SQL.+END-EXEC").IsMatch(Line))
                                {
                                    continue;
                                }
                                else if(new Regex("EXEC SQL").IsMatch(Line) || new Regex("COPY[ ]+\"[a-zA-Z0-9-]+\"\\.").IsMatch(Line))
                                {                                  
                                    string[] IncludeLines = null;
                                    while (true)
                                    {
                                        i++;
                                        Line = Lines[i].Replace('\t', ' ');
                                       
                                        if (new Regex("END-EXEC".RegexUpperLower()).IsMatch(Line))
                                            break;
                                       
                                        
                                        Match IncludeMatch = new Regex($@"{"INCLUDE".RegexUpperLower()}[ ]+[a-zA-Z0-9-_\.]+").Match(Line);
                                        Match CopyMatch = new Regex("COPY[ ]+\"[a-zA-Z0-9-]+\"\\.").Match(Line);
                                        if (IncludeMatch.Success || CopyMatch.Success)
                                        {
                                            string IncludeFileName = IncludeMatch.Success ? IncludeMatch.Value.RegexReplace("INCLUDE", string.Empty).Trim():CopyMatch.Value.RegexReplace("COPY", string.Empty).Replace("\"", string.Empty).Trim();
                                            IncludeFilePath = $@"{WorkingDir}\{IncludeFileName}";
                                                                                        
                                            if (File.Exists(IncludeFilePath))
                                            {
                                                IncludeLines = File.ReadAllLines(IncludeFilePath);
                                                IncludeCount++;
                                            }
                                            else
                                            {
                                                Console.WriteLine($"Missing file {IncludeFilePath}");
                                            }
                                            if (CopyMatch.Success) break;
                                        }
                                        
                                    }
                                    if (IncludeLines != null)
                                    {
                                        Lines.InsertRange(i+1,IncludeLines);
                                        addedLines+= IncludeLines.Length;                                        
                                    }
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
                                            while (true)
                                            {
                                                if(CurrentVariable.Level == Level)
                                                {
                                                    CurrentVariable = CurrentVariable.Parent;
                                                    break;
                                                }
                                                CurrentVariable = CurrentVariable.Parent;
                                            }
                                        }

                                        PreLevel = Level;
                                    }
                                    CurrentVariable.Childs.Add(new CobolVariable()
                                    {
                                        Parent = CurrentVariable,
                                        Raw = SBStatement.ToString(),
                                        Level = Level.Value,
                                        RowNumber = RowNum-1
                                    });
                                    SBStatement = new StringBuilder();
                                }
                            }
                            break;                      
                        case CobolToCSharp.ParseMode.COLLECT_PROCEDURE_DIVISION:
                            
                            Line = RemoveNumericsAtStart(Line);
                            if (CollectSQL)
                            {
                                SBStatement.Append(" ");
                                SBStatement.Append(Line);
                                if (new Regex("^END-EXEC".RegexUpperLower()).IsMatch(Line))
                                {
                                    CollectSQL = false;
                                }
                                continue;
                            }

                            if (Line.Contains("IF NOT TP-OK"))
                            {
                                int asd1 = 00;
                            }

                            MatchCollection Collection = RegexContainsStatement.Matches(Line);
                            if (Collection.Count > 1)
                            {
                                char PreChar = Line[Collection[1].Index - 1];
                                char PostChar = Collection[1].Index + Collection[1].Length < Line.Length ? Line[Collection[1].Index + Collection[1].Length] : '_';
                                if ((PreChar == ' ' || PreChar == '.') && PostChar == ' ')
                                {
                                    string temp = Line;
                                    Line = temp.Substring(0, Collection[1].Index).Trim();
                                    Lines.Insert(i + 1, temp.Substring(Collection[1].Index).Trim());
                                    Lines[i] = Line;
                                    addedLines++;
                                }
                            }                            
                            if (Paragraph.RegexEXECSQL.IsMatch(Line))
                            {
                                Paragraphs.Last().AddStatement(SBStatement.ToString(), StatementRowNum - addedLines);
                                CollectSQL = true;
                                StatementRowNum = RowNum;
                                SBStatement = new StringBuilder();
                                SBStatement.Append(Line);
                            }
                            else if (RegexCOMMENT.IsMatch(Line))
                            {
                                Paragraphs.Last().AddStatement(SBStatement.ToString(), StatementRowNum - addedLines);
                                StatementRowNum = RowNum;
                                SBStatement = new StringBuilder(string.Empty);
                                Paragraphs.Last().AddStatement(Line, RowNum - addedLines);
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
                            else if (ParagraphRegex.IsMatch(Line) && (string.IsNullOrEmpty(SBStatement.ToString()) || SBStatement.ToString().Trim().EndsWith(".")))
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

            List<CobolVariable> CobolVariables = new List<CobolVariable>(new CobolVariable[] { WORKING_STORAGE_VARIABLE, LINKAGE_SECTION_VARIABLE });
            ParseScreen(FileName, CobolVariables);
            
            var DataTypes = WriteAllVariables(WORKING_STORAGE_VARIABLE.Childs,LINKAGE_SECTION_VARIABLE.Childs);

        
            

           

            //ExtractUndefinedVariables(Paragraphs, CobolVariables);

            ConvertParagraphs(Paragraphs,DataTypes);

        }
    }
}
