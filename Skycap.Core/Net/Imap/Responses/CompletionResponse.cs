namespace Skycap.Net.Imap.Responses
{
    using System;
    using System.Text.RegularExpressions;

    public class CompletionResponse : IMAP4Response
    {
        private ECompletionResponseType _completionResult;
        protected string _message;
        protected const string IMAP4BAD = "BAD";
        protected const string IMAP4NO = "NO";
        protected const string IMAP4OK = "OK";

        public CompletionResponse(string message) : base(message)
        {
            base._response = message;
            this._message = "";
            this.CompletionResult = ECompletionResponseType.BAD;
            int index = message.IndexOf(' ');
            int num2 = message.IndexOf(' ', index + 1);
            if (index != -1)
            {
                string str;
                if (num2 != -1)
                {
                    str = message.Substring(index + 1, num2 - (index + 1));
                    this._message = message.Substring(num2 + 1);
                }
                else
                {
                    str = message.Substring(index + 1);
                }
                if (str == "OK")
                {
                    this.CompletionResult = ECompletionResponseType.OK;
                }
                else if (str == "NO")
                {
                    this.CompletionResult = ECompletionResponseType.NO;
                }
                else
                {
                    this.CompletionResult = ECompletionResponseType.BAD;
                }
            }
        }

        protected string GetMessage(string message)
        {
            return message;
        }

        public static bool IsCompletionResponse(string message)
        {
            return Regex.Match(message, @"[a-zA-Z0-9]+\s(OK|BAD|NO)($|\s.*)", RegexOptions.IgnoreCase).Success;
        }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(base._response))
            {
                return base._response;
            }
            return this.CompletionResult.ToString();
        }

        public ECompletionResponseType CompletionResult
        {
            get
            {
                return this._completionResult;
            }
            protected set
            {
                this._completionResult = value;
            }
        }

        public string Message
        {
            get
            {
                return this._message;
            }
        }
    }
}

