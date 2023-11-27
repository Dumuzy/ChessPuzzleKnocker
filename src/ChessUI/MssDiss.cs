using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Text.RegularExpressions;
using AwiUtils;

namespace ChessKnocker
{
    internal class MssDiss : Diss
    {
        public MssDiss(string pairs = "") : base(pairs, sdefaults, null, '=')
        {

        }

        public override string this[string key]
        {
            get
            {
                if (!di.TryGetValue(key, out string value))
                    defaults.TryGetValue(key, out value);

                value = ReplaceDollarVariables(value);
                value = value.Replace('§', ' ');
                return value;
            }
        }

        private string ReplaceDollarVariables(string value)
        {
                var m = Regex.Match(value, @"([\w§]*)\$(\w+)");
                if (m.Success)
                    value = m.Groups[1].Value + this[m.Groups[2].Value];
            return value;
        }

        public void IncVal(string key) => this.Add(key, this.Int(key) + this.Int("dd" + key));

        static readonly string sdefaults = "numlevelsinrow=8 source=licsv target=$square";
    }
}
