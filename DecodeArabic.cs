using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CobolToCSharp
{
    public class DecodeArabic
    {
             
        public static string DecodeFromISO_8859_6(string Message)
        {
            byte[] bytes = Message.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries)
                     .Select(s => (byte)Convert.ToInt32(s, 8))
                     .ToArray();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            return Encoding.GetEncoding("ISO-8859-6").GetString(bytes);
        }
        public static string Decode(string Message)
        {
            foreach (Match Match in new Regex(@"\\[0-9]+").Matches(Message))
            {
                string DecodedValue = DecodeFromISO_8859_6(Match.Value);
                Message = Message.Replace(Match.Value, DecodedValue);
            }
            return Message;
        }


        public static string DecodeFile(string FilePath)
        {
            string[] Lines = File.ReadAllLines(FilePath);
            StringBuilder SB = new StringBuilder();
            using (StreamWriter Writer = new StreamWriter("f700-out.scrn"))
            {
                foreach (var Line in Lines)
                {
                    SB.AppendLine(DecodeArabic.Decode(Line));
                }
            }
            return SB.ToString();
        }
    }
}
