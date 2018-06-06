namespace Skycap.Net.Common.Parsers
{
    using System;
    using System.Runtime.CompilerServices;

    public abstract class BaseParser
    {
        public event EventHandler FinalBoundaryReached;

        protected BaseParser()
        {
        }

        protected virtual void RaiseFinalBoundaryReached()
        {
            if (this.FinalBoundaryReached != null)
            {
                this.FinalBoundaryReached(this, new EventArgs());
            }
        }
    }
}

