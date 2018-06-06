namespace Skycap.Net.Smtp
{
    using Skycap.Net.Common.Connections;
    using Skycap.Net.Smtp.Exceptions;
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class SmtpResponse
    {
        protected ESmtpCommandResultCode _code;
        protected string _generalMessage;
        protected readonly List<SmtpResponseLine> _responseLines;
        protected ESmtpResponseType _type;
        public const string InvalidResponseFormatMessage = "Error in server response. Invalid format";

        public SmtpResponse(IConnection connection)
        {
            SmtpResponseLine line;
            this._responseLines = new List<SmtpResponseLine>();
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            do
            {
                line = new SmtpResponseLine(connection.ReceiveLine());
                this._responseLines.Add(line);
            }
            while (!line.IsLastLine);
            this.Type = GetResponseTypeFromLine(this._responseLines[0].SourceResponseLine);
            this.Code = (ESmtpCommandResultCode) this._responseLines[0].Code;
            this.GeneralMessage = this._responseLines[0].Comment;
        }

        public virtual string GetResponseText()
        {
            StringBuilder builder = new StringBuilder();
            foreach (SmtpResponseLine line in this.Lines)
            {
                builder.Append(line.SourceResponseLine + "\r\n");
            }
            return builder.ToString();
        }

        protected static ESmtpResponseType GetResponseTypeFromLine(string responseLine)
        {
            switch (responseLine[0])
            {
                case '2':
                    return ESmtpResponseType.PositiveCompletion;

                case '3':
                    return ESmtpResponseType.PositiveIntermediate;

                case '4':
                    return ESmtpResponseType.NegativeTransient;

                case '5':
                    return ESmtpResponseType.NegativePermanent;
            }
            throw new UnexpectedSMTPResponseException("Error in server response. Invalid format");
        }

        public virtual ESmtpCommandResultCode Code
        {
            get
            {
                return this._code;
            }
            protected set
            {
                this._code = value;
            }
        }

        public virtual string GeneralMessage
        {
            get
            {
                return this._generalMessage;
            }
            protected set
            {
                this._generalMessage = value;
            }
        }

        public virtual IEnumerable<SmtpResponseLine> Lines
        {
            get
            {
                return this._responseLines;
            }
        }

        public virtual ESmtpResponseType Type
        {
            get
            {
                return this._type;
            }
            protected set
            {
                this._type = value;
            }
        }
    }
}

