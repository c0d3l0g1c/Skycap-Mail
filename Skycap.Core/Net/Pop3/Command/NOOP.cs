namespace Skycap.Net.Pop3.Command
{
    using System;

    public class NOOP : Pop3Command
    {
        public NOOP()
        {
            base.name = "NOOP";
        }
    }
}

