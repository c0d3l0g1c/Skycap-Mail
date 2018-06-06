namespace Skycap.Net.Common.Collections
{
    using System;
    using System.Collections.Generic;

    public static class DictionaryExtenders
    {
        public static bool SmartAdd(IDictionary<string, string> dic, Dictionary<string, string> value)
        {
            if (dic == null)
            {
                throw new ArgumentNullException("dic");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            bool flag = true;
            foreach (KeyValuePair<string, string> pair in value)
            {
                if (!SmartAdd(dic, pair.Key, pair.Value))
                {
                    flag = false;
                }
            }
            return flag;
        }

        public static bool SmartAdd(IDictionary<string, string> dic, string key, string value)
        {
            if (dic == null)
            {
                throw new ArgumentNullException("dic");
            }
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (dic.ContainsKey(key))
            {
                return false;
            }
            dic.Add(key, value);
            return true;
        }
    }
}

