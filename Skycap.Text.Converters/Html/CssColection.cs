namespace Skycap.Text.Converters.Html
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;

    public class CssColection : List<Css>
    {
        public int Add(Css css)
        {
            base.Add(css);
            return (base.Count - 1);
        }

        public int IndexOf(string name, CssType cssType, string strfor)
        {
            for (int i = 0; i < base.Count; i++)
            {
                if (((this[i].Type == cssType) && this[i].Name.Equals(name)) && this[i].For.Equals(strfor))
                {
                    return i;
                }
            }
            return -1;
        }

        public Css this[int index]
        {
            get
            {
                return (Css) base[index];
            }
            set
            {
                base[index] = value;
            }
        }
    }
}

