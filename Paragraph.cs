﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CobolParser
{
    public class Paragraph
    {
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

        public Paragraph()
        {
            Statements = new List<Statement>();          
        }
        public string Name { get; set; }
       
        public List<Statement> Statements { get; set; }

        private static StatementType GetStatementType(string Line)
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
                    StatementType = GetStatementType(Statement)
                });
            }
        }
    }
}
