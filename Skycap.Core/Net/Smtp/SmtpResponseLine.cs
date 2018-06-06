namespace Skycap.Net.Smtp
{
    using System;
    using System.Text.RegularExpressions;

    public class SmtpResponseLine
    {
        protected int _code;
        protected string _comment;
        protected bool _isLastLine;
        protected string _sourceResponseLine;

        public SmtpResponseLine(string line)
        {
            this.InitFromResponseString(line);
        }

        protected virtual void InitFromResponseString(string responseString)
        {
            if (!IsFormatCorrect(responseString))
            {
                throw new FormatException("SMTP response line does not match RFC");
            }
            this.SourceResponseLine = responseString;
            this.Code = int.Parse(responseString.Substring(0, 3));
            this.Comment = responseString.Substring(4);
            this.IsLastLine = responseString[3] == ' ';
        }

        public static bool IsFormatCorrect(string responseString)
        {
            return Regex.Match(responseString, @"(?<Code>[0-9]{3})(?<HasNextLine>[\s-]{1})(?<Message>.*)", RegexOptions.IgnoreCase).Success;
        }

        public override string ToString()
        {
            return this.SourceResponseLine;
        }

        public virtual int Code
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

        public virtual string Comment
        {
            get
            {
                return this._comment;
            }
            protected set
            {
                this._comment = value;
            }
        }

        public virtual bool IsLastLine
        {
            get
            {
                return this._isLastLine;
            }
            protected set
            {
                this._isLastLine = value;
            }
        }

        public virtual string SourceResponseLine
        {
            get
            {
                return this._sourceResponseLine;
            }
            protected set
            {
                this._sourceResponseLine = value;
            }
        }
    }
}

