using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CobolToCSharp.Screen
{
    public class ScreenElement
    {
        public string Label { get; set; }
        public ScreenBlock ScreenBlock { get; set; }
        public string[] Options { get; set; }
        public string OptionsJson
        {
            get
            {
                StringBuilder SB = new StringBuilder();
                foreach (var Option in Options)
                {
                    if (SB.Length > 0)
                        SB.Append(",");
                    Match MatchValue = new Regex("[0-9][ ]*-").Match(Option);
                    string Text = Option.Substring(MatchValue.Index+ MatchValue.Length).Trim().Replace(char.ConvertFromUtf32(160), " ");
                    string Value = MatchValue.Value.Replace("-", string.Empty).Trim();
                    SB.Append($"{{\"Label\": \"{Text}\",\"Value\": {Value}}}");
                }
                return SB.ToString();
            }
        }
        public string Name {
            get
            {
                return ScreenBlock?.Name;
            }
        }
        public string TagName
        {
            get
            {
                if(ScreenBlock == null)
                {
                    return "label";
                }
                else
                {
                    if (Options !=null && Options.Length>0)
                    {
                        return "select";
                    }
                    return "text";
                }
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return ScreenBlock!=null && !string.IsNullOrEmpty(ScreenBlock.OUTPUT) && string.IsNullOrEmpty(ScreenBlock.INPUT);
            }
        }
    }
}
