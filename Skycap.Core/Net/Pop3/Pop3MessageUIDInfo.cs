namespace Skycap.Net.Pop3
{
    using System;
    using System.Diagnostics;

    [DebuggerDisplay("S = {SerialNumber}, U = {UniqueNumber}")]
    public class Pop3MessageUIDInfo
    {
        protected uint _serialNumber;
        protected string _uniqueNumber;

        public Pop3MessageUIDInfo(uint serialNumber, string uniqueNumber)
        {
            if (uniqueNumber == null)
            {
                throw new ArgumentNullException("uniqueNumber");
            }
            this.SerialNumber = serialNumber;
            this.UniqueNumber = uniqueNumber;
        }

        public virtual uint SerialNumber
        {
            get
            {
                return this._serialNumber;
            }
            protected set
            {
                this._serialNumber = value;
            }
        }

        public virtual string UniqueNumber
        {
            get
            {
                return this._uniqueNumber;
            }
            protected set
            {
                this._uniqueNumber = value;
            }
        }
    }
}

