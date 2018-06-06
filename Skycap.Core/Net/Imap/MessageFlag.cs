namespace Skycap.Net.Imap
{
    using System;
    using System.Runtime.Serialization;
    using System.Text.RegularExpressions;

    [DataContract]
    public class MessageFlag
    {
        private const string _exEmptyName = "Flag name can't be null or an empty string";
        private const string _NonStandardFlagWithouName = "Can't create non standart flag without name. Use ImapMessageFlag(string) constructor.";
        private EFlag _type;
        private readonly string flagString;

        public MessageFlag(EFlag value)
        {
            if (value == EFlag.NonStandart)
            {
                throw new ArgumentException("Can't create non standart flag without name. Use ImapMessageFlag(string) constructor.", "value");
            }
            this._type = value;
        }

        public MessageFlag(string value)
        {
            Regex regex = new Regex(@"\A(\w+|\*?)\Z");
            if (!regex.IsMatch(value))
            {
                throw new FormatException("Flag name doesn't match required format");
            }
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException("Flag name can't be null or an empty string", "value");
            }
            this._type = FromString(value);
            this.flagString = value;
        }

        public bool Equals(MessageFlag other)
        {
            if (object.ReferenceEquals(null, other))
            {
                return false;
            }
            if (!object.ReferenceEquals(this, other))
            {
                if (other._type != this._type)
                {
                    return false;
                }
                if (other._type == EFlag.NonStandart)
                {
                    return string.Equals(other.flagString, this.flagString, StringComparison.OrdinalIgnoreCase);
                }
            }
            return true;
        }

        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(null, obj))
            {
                return false;
            }
            if (object.ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != typeof(MessageFlag))
            {
                return false;
            }
            return this.Equals((MessageFlag) obj);
        }

        private static EFlag FromString(string value)
        {
            switch (value.ToLower())
            {
                case "deleted":
                    return EFlag.Deleted;

                case "answered":
                    return EFlag.Answered;

                case "draft":
                    return EFlag.Draft;

                case "flagged":
                    return EFlag.Flagged;

                case "recent":
                    return EFlag.Recent;

                case "seen":
                    return EFlag.Seen;
            }
            return EFlag.NonStandart;
        }

        public override int GetHashCode()
        {
            if (this._type == EFlag.NonStandart)
            {
                return ((this._type.GetHashCode() * 0x18d) ^ ((this.flagString != null) ? this.flagString.ToLower().GetHashCode() : 0));
            }
            return (this._type.GetHashCode() * 0x18d);
        }

        public override string ToString()
        {
            if (this._type != EFlag.NonStandart)
            {
                return this._type.ToString();
            }
            return this.flagString;
        }

        [DataMember]
        public EFlag Type
        {
            get
            {
                return this._type;
            }
            private set
            {
                this._type = value;
            }
        }
    }
}

