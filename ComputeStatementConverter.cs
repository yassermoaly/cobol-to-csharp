using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CobolToCSharp
{
    public class ComputeStatementConverter : IStatementConverter
    {
        public List<StatementType> StatementTypes => new List<StatementType>(new StatementType[] { StatementType.COMPUTE });

        public string Convert(string Line, Paragraph Paragraph, List<Paragraph> Paragraphs, Dictionary<string, string> CobolVariablesDataTypes = null)
        {
            if(Line.Contains("COMPUTE POINT-DT-YEAR = POINT-DT-YEAR - 1"))
            {
                int asdasd = 100;
            }
            Line = Line.RegexReplace("COMPUTE[ ]+", "COMPUTE", string.Empty).Trim();
            string[] Tokens = Line.Split('=');
            string LeftHand = Tokens[0];
            string RightHand = Tokens[1];
            if (RightHand.EndsWith("."))
                RightHand = RightHand.Remove(RightHand.Length - 1,1);

            string[] RightHandTokens = new Regex("[/*+-][ ]+").Split(RightHand);
            MatchCollection RightHandMatches = new Regex("[/*+-]").Matches(RightHand);
            StringBuilder SB = new StringBuilder();
            for (int i = 0; i < RightHandTokens.Length; i++)
            {
                SB.Append($"(double){NamingConverter.Convert(RightHandTokens[i].Trim())}");
                if(i+1< RightHandTokens.Length)
                {
                    SB.Append($"{RightHandMatches[i].Value} ");
                }                
            }

            string LeftHandDataType = HelpingFunctions.GetDatatype(LeftHand.Trim(), CobolVariablesDataTypes);
            return $"{NamingConverter.Convert(LeftHand.Trim())} = ({LeftHandDataType})({SB.ToString()});";


            //if (Line.EndsWith("."))
            //    Line.Remove(Line.Length - 1);
            //return $"{Line.RegexReplace("COMPUTE", string.Empty).Trim()};";
            //foreach (Match Match in new Regex(@"([/*+-=][ ]*(([a-zA-Z-]+)|([0-9]+)|([0-9]*\.[0-9]+)))").Matches(Line))
            //{
            //    int x = 100;
            //}
            //return Line.Replace("*", "//");
        }
        
    }
}
