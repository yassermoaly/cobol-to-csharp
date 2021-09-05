using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobolToCSharp
{
    public static class CustomExtensions
    {
        public static void Merge<TKey,TValue>(this Dictionary<TKey, TValue> SourceDic, Dictionary<TKey, TValue> Dic)
        {
            foreach (var Key in Dic.Keys)
            {
                if (!SourceDic.ContainsKey(Key))
                    SourceDic.Add(Key, Dic[Key]);
            }
        }
    }
}
