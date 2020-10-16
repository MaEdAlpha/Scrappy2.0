using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scrappy2._0
{
    public static class DictionaryExtension
    {
        public static Dictionary<TKey, TValue> ShuffleDictionary<TKey, TValue>(this Dictionary<TKey, TValue> source)
        {
            Random r = new Random();
            return source.OrderBy(x => r.Next())
               .ToDictionary(item => item.Key, item => item.Value);
        }
    }
}
