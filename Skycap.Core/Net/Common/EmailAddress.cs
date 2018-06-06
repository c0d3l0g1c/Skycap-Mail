namespace Skycap.Net.Common
{
    using System;
    using System.Diagnostics;
    using System.Runtime.Serialization;
    using System.Text.RegularExpressions;
    using Windows.UI;
    using Windows.UI.Xaml.Media;

    [DataContract]
    [DebuggerDisplay("{DisplayName}<{Address}>")]
    public class EmailAddress
    {
        protected string _address;
        protected string _displayName;
        protected const string _regExp = @"^[\w-]+(?:\.[\w-]+)*@(?:[\w-]+\.)+[a-zA-Z]{2,7}$";
        protected const string EMailIsNotProperlyFormattedMessage = "E-mail is not properly formatted";

        public EmailAddress(string emailAddress)
        {
            this.InitEmailAddressField(emailAddress);
        }

        public EmailAddress(string emailAddress, string displayName)
        {
            this.InitEmailAddressField(emailAddress);
            this.DisplayName = (emailAddress == displayName ? "" : displayName);
        }

        public virtual string GetEmailString()
        {
            return string.Format("<{0}>", this.Address);
        }

        protected virtual void InitEmailAddressField(string emailAddress)
        {
            if (emailAddress == null)
            {
                throw new ArgumentNullException("emailAddress");
            }
            if (!IsValid(emailAddress))
            {
                throw new FormatException("E-mail is not properly formatted");
            }
            this.Address = emailAddress;
        }

        public static bool IsValid(string emailAddress)
        {
            Regex regex = new Regex(@"^ *(?:[\w\d\.!#$%&'*+\-/=?^_`{|}~-]+)@(?:[\w\d\.!#$%&'*+\-/=?^_`{|}~]+)(?:\.[a-zA-Z]{2,7})? *$", RegexOptions.IgnoreCase);
            return regex.Match(emailAddress).Success;
        }

        public override bool Equals(object obj)
        {
            return ToString().ToString() == obj.ToString();
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(this.DisplayName))
            {
                return string.Format("{0} {1}", this.DisplayName, this.GetEmailString());
            }
            return this.Address;
        }

        [DataMember]
        public virtual string Address
        {
            get
            {
                return this._address;
            }
            set
            {
                if (!IsValid(value))
                {
                    throw new FormatException("E-mail is not properly formatted");
                }
                this._address = value;
            }
        }

        [DataMember]
        public virtual string DisplayName
        {
            get
            {
                return this._displayName;
            }
            set
            {
                this._displayName = value;
            }
        }

        [IgnoreDataMember]
        public virtual string DisplayNameAlternate
        {
            get
            { 
                return (string.IsNullOrEmpty(DisplayName) ? Address : DisplayName);
            }
        }

        [IgnoreDataMember]
        public virtual string ToolTip
        {
            get
            {
                return (string.IsNullOrEmpty(DisplayName) ? null : DisplayName + Environment.NewLine) + Address;
            }
        }
    }
}

