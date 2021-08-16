using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobolToCSharp
{
    public class MoveStatementConverter : IStatementConverter
    {
        public StatementType StatementType => StatementType.MOVE;

        public string Convert(string Line,Paragraph Paragraph, List<Paragraph> Paragraphs)
        {
            Line = Line.Replace("ALL SPACES", "SPACES");
            StringBuilder ConvertedLine = new StringBuilder();
            string[] Tokens = Line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if(Tokens[0].Equals("MOVE") && Tokens[2].Equals("TO"))
            {                
                string SetValue = Tokens[1];
                for (int i = 3; i < Tokens.Length; i++)
                {
                    if (i == Tokens.Length - 1)
                        Tokens[i] = Tokens[i].Replace(".", string.Empty);
                    ConvertedLine.Append($"{NamingConverter.Convert(Tokens[i])} = ");
                }
                ConvertedLine.Append($"{NamingConverter.Convert(Tokens[1])};");
                return ConvertedLine.ToString();
            }
            throw new Exception($"Invalid {StatementType.ToString()} Statement, {Line}");
        }
    }
}
