namespace Skycap.Text.Converters.Html
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;

    public class Params : List<Param>
    {
        public int Add(Param param)
        {
            base.Add(param);
            return (base.Count - 1);
        }

        public int Add(string name, string value)
        {
            Param param = new Param(name, value);
            return this.Add(param);
        }

        public Param ByName(string name)
        {
            for (int i = 0; i < base.Count; i++)
            {
                if (this[i].Name.Trim().ToLower() == name.ToLower())
                {
                    return this[i];
                }
            }
            return null;
        }

        public override string ToString()
        {
            string str = "";
            for (int i = 0; i < base.Count; i++)
            {
                str = str + this[i].ToString() + " ";
            }
            return str;
        }

        public Param this[int index]
        {
            get
            {
                return (Param) base[index];
            }
            set
            {
                base[index] = value;
            }
        }
    }
}

