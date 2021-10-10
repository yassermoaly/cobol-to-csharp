using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CobolToCSharp
{
    public class Paragraph
    {        
        #region 
        public static Regex RegexGOTO = new Regex("^GO[ ]+(TO)*".RegexUpperLower());
        public static Regex RegexEXECSQL = new Regex("^EXEC[ ]+SQL".RegexUpperLower());
        public static Regex RegexSTOPRUN = new Regex("^STOP[ ]+RUN".RegexUpperLower());        
        public static Regex RegexEXITPROGRAM = new Regex("(^EXIT)|(^EXIT[ ]+PROGRAM)".RegexUpperLower());
        public static Regex RegexMOVE = new Regex("^MOVE".RegexUpperLower());
        public static Regex RegexINSPECT = new Regex("^INSPECT".RegexUpperLower());
        public static Regex RegexACCEPT = new Regex("^ACCEPT[ ]+".RegexUpperLower());
        
        public static Regex RegexIF = new Regex("^IF".RegexUpperLower());
        public static Regex RegexPERFORM = new Regex("^PERFORM".RegexUpperLower());
        public static Regex RegexELSE = new Regex("^ELSE".RegexUpperLower());
        public static Regex RegexELSE_IF = new Regex("^ELSE IF".RegexUpperLower());
        public static Regex RegexEND_IF = new Regex("^END-IF".RegexUpperLower());
        public static Regex RegexDISPLAY = new Regex("^DISPLAY".RegexUpperLower());
        public static Regex RegexADD = new Regex("^ADD".RegexUpperLower());
        public static Regex RegexSUBTRACT = new Regex("^SUBTRACT".RegexUpperLower());
        public static Regex RegexCOMPUTE = new Regex("^COMPUTE".RegexUpperLower());
        public static Regex RegexDIVIDE = new Regex("^DIVIDE".RegexUpperLower());
        public static Regex RegexMULTIPLY = new Regex("^MULTIPLY".RegexUpperLower());
        public static Regex RegexCALL = new Regex("^CALL".RegexUpperLower());
        public static Regex RegexCOMMENT = new Regex(@"^\*");
        public static Regex RegexENDPROGRAM = new Regex(@"^END[ ]+PROGRAM".RegexUpperLower());
        private static readonly Regex ParagraphRegex = new Regex(@"^[a-zA-Z0-9-_]+\.$");
        #endregion
        public List<Paragraph> Paragraphs { get; set; }
        public Paragraph()
        {
            Statements = new List<Statement>();          
        }

        private string _Name;
        public string Name
        {
            get
            {
                return _Name.Replace(".",string.Empty);
            }
            set
            {
                _Name = value;
            }
        }
       
        public List<Statement> Statements { get; set; }

        public static StatementType GetStatementType(string Line)
        {

            if (RegexGOTO.IsMatch(Line))
                return StatementType.GOTO;
            else if (RegexEXECSQL.IsMatch(Line))
                return StatementType.QUERY;
            else if (RegexEXITPROGRAM.IsMatch(Line))
                return StatementType.EXITPROGRAM;
            else if (RegexMOVE.IsMatch(Line))
                return StatementType.MOVE;
            else if (RegexIF.IsMatch(Line))
                return StatementType.IF;
            else if (RegexPERFORM.IsMatch(Line))
                return StatementType.PERFORM;
            else if (RegexELSE_IF.IsMatch(Line))
                return StatementType.ELSE_IF;
            else if (RegexEND_IF.IsMatch(Line))
                return StatementType.END_IF;
            else if (RegexELSE.IsMatch(Line))
                return StatementType.ELSE;

            else if (RegexDISPLAY.IsMatch(Line))
                return StatementType.DISPLAY;

            else if (RegexADD.IsMatch(Line))
                return StatementType.ADD;

            else if (RegexSUBTRACT.IsMatch(Line))
                return StatementType.SUBTRACT;

            else if (RegexCOMPUTE.IsMatch(Line))
                return StatementType.COMPUTE;

            else if (RegexDIVIDE.IsMatch(Line))
                return StatementType.DIVIDE;

            else if (RegexMULTIPLY.IsMatch(Line))
                return StatementType.MULTIPLY;

            else if (RegexCALL.IsMatch(Line))
                return StatementType.CALL;

            else if (RegexCOMMENT.IsMatch(Line))
                return StatementType.COMMENT;
            else if (RegexENDPROGRAM.IsMatch(Line))
                return StatementType.END_PROGRAM;
            else if (RegexSTOPRUN.IsMatch(Line))
                return StatementType.STOP_RUN;
            else if (RegexINSPECT.IsMatch(Line))
                return StatementType.INSPECT;
            else if (RegexACCEPT.IsMatch(Line))
                return StatementType.ACCEPT;

            throw new Exception($"Statement Type not handeled {Line}");
        }

        public void AddStatement(string Statement,int RowNo)
        {
            if (!string.IsNullOrEmpty(Statement.Trim()))
            {
                Statements.Add(new Statement()
                {
                    Paragraph = this,
                    Raw = Statement,
                    RowNo = RowNo,
                    Paragraphs = Paragraphs,
                    StatementType = GetStatementType(Statement)
                });
            }
        }
    }
}
