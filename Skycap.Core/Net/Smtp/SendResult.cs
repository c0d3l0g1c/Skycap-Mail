namespace Skycap.Net.Smtp
{
    using System;
    using System.Collections.Generic;
    using Skycap.Net.Common;

    public class SendResult
    {
        protected readonly List<EmailAddress> _invalidAddresses;
        protected SmtpResponse _lastResponse;
        protected ESendResult _result;

        public SendResult(ESendResult result, SmtpResponse lastResponse)
        {
            this._invalidAddresses = new List<EmailAddress>();
            if (lastResponse == null)
            {
                throw new ArgumentNullException("lastResponse");
            }
            this.Result = result;
            this.LastResponse = lastResponse;
        }

        public SendResult(ESendResult result, SmtpResponse lastResponse, IEnumerable<EmailAddress> invalidEmails)
        {
            this._invalidAddresses = new List<EmailAddress>();
            if (lastResponse == null)
            {
                throw new ArgumentNullException("lastResponse");
            }
            if (invalidEmails == null)
            {
                throw new ArgumentNullException("invalidEmails");
            }
            this.Result = result;
            this._invalidAddresses.AddRange(invalidEmails);
            this.LastResponse = lastResponse;
        }

        public virtual IEnumerable<EmailAddress> InvalidAddresses
        {
            get
            {
                return this._invalidAddresses;
            }
        }

        public virtual bool IsSuccessful
        {
            get
            {
                if (this.Result != ESendResult.Ok)
                {
                    return (this.Result == ESendResult.OkWithInvalidEmails);
                }
                return true;
            }
        }

        public virtual SmtpResponse LastResponse
        {
            get
            {
                return this._lastResponse;
            }
            protected set
            {
                this._lastResponse = value;
            }
        }

        public virtual ESendResult Result
        {
            get
            {
                return this._result;
            }
            set
            {
                this._result = value;
            }
        }
    }
}

