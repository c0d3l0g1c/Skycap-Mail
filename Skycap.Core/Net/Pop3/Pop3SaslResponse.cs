namespace Skycap.Net.Pop3
{
    using System;

    public class Pop3SaslResponse : Pop3Response
    {
        public Pop3SaslResponse(string message)
        {
            if (message == null)
            {
                this.Type = EPop3ResponseType.ERR;
                base.Response = "The server doesn't return any response";
            }
            else
            {
                base.Response = message;
            }
        }

        protected override string ParseResponse(string message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }
            if (message.StartsWith("+OK"))
            {
                this.Type = EPop3ResponseType.OK;
                return message.Remove(0, 3).Trim();
            }
            if (message.StartsWith("+ "))
            {
                this.Type = EPop3ResponseType.OK;
                return message.Remove(0, 2).Trim();
            }
            this.Type = EPop3ResponseType.ERR;
            if (message.StartsWith("-ERR"))
            {
                return message.Remove(0, 4).Trim();
            }
            return message;
        }
    }
}

