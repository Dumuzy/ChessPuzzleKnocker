using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace AwiUtils
{
    /// <summary> String-String-Dictionary mit Defaults. Wirft nie. </summary>
    public class Diss
    {
        public Diss(string pairs, string sdefaults, IEqualityComparer<string> eqComparer = null,
                char keyValueSeparator = ' ')
        {
            this.eqComparer = eqComparer;
            this.keyValueSeparator = keyValueSeparator;
            this.di = Helper.ToDictionary(pairs, eqComparer, keyValueSeparator);
            this.defaults = Helper.ToDictionary(sdefaults, eqComparer, keyValueSeparator);
        }

        public void Add(string key, string value) => di[key] = value;
        public void Add(string key, int value) => di[key] = value.ToString();

        public virtual string this[string key]
        {
            get
            {
                if (!di.TryGetValue(key, out string value))
                    defaults.TryGetValue(key, out value);
                return value;
            }
        }

        public int Int(string key) => Helper.ToInt(this[key]);

        public override string ToString()
        {
            string s = "";
            foreach (var kvp in di)
                s += kvp.Key + keyValueSeparator + kvp.Value + " ";
            return s;
        }

        public Li<string> ToLines(string prefix = "")
        {
            var lines = new Li<string>();
            foreach (var kvp in di)
                lines.Add(prefix + kvp.Key + keyValueSeparator + kvp.Value);
            return lines;
        }

        public void FromLines(Li<string> lines, string prefix = "")
        {
            foreach (var line in lines)
                if (line.StartsWith(prefix))
                {
                    var parts = line.Substring(0, prefix.Length).Split(keyValueSeparator, 2);
                    di.Add(parts[0], parts[1]);
                }
        }


        protected readonly Dictionary<string, string> defaults;
        protected Dictionary<string, string> di;

        IEqualityComparer<string> eqComparer;
        char keyValueSeparator;
    }
}
