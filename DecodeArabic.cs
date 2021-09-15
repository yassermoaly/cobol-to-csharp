using System;
using System.Collections.Generic;
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

    }
}
