namespace Skycap.Net.Pop3.Command
{
    using System;

    public class RSET : Pop3Command
    {
        public RSET()
        {
            base.name = string.Format("RSET", new object[0]);
        }
    }
}

