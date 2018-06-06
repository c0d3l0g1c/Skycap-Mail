namespace Skycap.Net.Imap
{
    using System;
    using System.Text.RegularExpressions;

    public class Keyword
    {
        protected const string forbidden = "%*\"\\](){ ";
        protected string keyword;

        public Keyword(string keyword)
        {
            Regex regex = new Regex("^[^ %*\\\"{()\\]\\\\]+$");
            if (!regex.Match(keyword).Success)
            {
                throw new ArgumentException("keyword contain forbidden characters");
            }
            this.keyword = keyword;
        }

        public override string ToString()
        {
            return this.keyword;
        }
    }
}

