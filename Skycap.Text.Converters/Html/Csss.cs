namespace Skycap.Text.Converters.Html
{
    using System;

    public class Csss
    {
        private CssColection a = new CssColection();

        public Csss Clone()
        {
            Csss csss = new Csss();
            for (int i = 0; i < this.Css.Count; i++)
            {
                Css css = this.a[i];
                csss.Css.Add(css);
            }
            return csss;
        }

        public void SetNameValue(string cssName, string cssValue, CssType cssType, string strFor)
        {
            int num = this.a.IndexOf(cssName.ToLower(), cssType, strFor);
            if (num != -1)
            {
                Css css = this.a[num];
                css.Value = cssValue;
                this.a[num] = css;
            }
            else
            {
                Css css2 = new Css {
                    Name = cssName.ToLower(),
                    Value = cssValue,
                    Type = cssType,
                    For = strFor
                };
                this.a.Add(css2);
            }
        }

        public CssColection Css
        {
            get
            {
                return this.a;
            }
        }
    }
}

