namespace Skycap.Text.Converters.Html
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;

    public class HTMLNodes : List<HTMLNode>
    {
        public int Add(HTMLNode node)
        {
            base.Add(node);
            return (base.Count - 1);
        }

        public HTMLNode this[int index]
        {
            get
            {
                return (HTMLNode) base[index];
            }
            set
            {
                base[index] = value;
            }
        }
    }
}

