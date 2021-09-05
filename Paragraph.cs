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
     
        #region Regex
        public static Regex RegexGOTO = new Regex("^GO[ ]+TO");
        public static Regex RegexEXECSQL = new Regex("^EXEC[ ]+SQL");
        public static Regex RegexEXITPROGRAM = new Regex("^EXIT[ ]+PROGRAM");
        public static Regex RegexMOVE = new Regex("^MOVE");
        public static Regex RegexIF = new Regex("^IF");
        public static Regex RegexPERFORM = new Regex("^PERFORM");
        public static Regex RegexELSE = new Regex("^ELSE");
        public static Regex RegexELSE_IF = new Regex("^ELSE IF");
        public static Regex RegexEND_IF = new Regex("^END-IF");
        public static Regex RegexDISPLAY = new Regex("^DISPLAY");
        public static Regex RegexADD = new Regex("^ADD");
        public static Regex RegexSUBTRACT = new Regex("^SUBTRACT");
        public static Regex RegexCOMPUTE = new Regex("^COMPUTE");
        public static Regex RegexDIVIDE = new Regex("^DIVIDE");
        public static Regex RegexMULTIPLY = new Regex("^MULTIPLY");
        public static Regex RegexCALL = new Regex("^CALL");
        public static Regex RegexCOMMENT = new Regex(@"^\*");
        public static Regex RegexENDPROGRAM = new Regex(@"^END[ ]+PROGRAM");
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
            
            throw new Exception($"Statement Type not handeled {Line}");
        }

        public void AddStatement(string Statement,int RowNo)
        {
            if (!string.IsNullOrEmpty(Statement))
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
