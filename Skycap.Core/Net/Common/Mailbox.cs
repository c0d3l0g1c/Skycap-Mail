namespace Skycap.Net.Common
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.Serialization;

    using Skycap.Net.Imap.Collections;
    using Skycap.Net.Imap.Parsers;
    using Skycap.Net.Imap.Responses;

    [DataContract(IsReference = true)]
    public class Mailbox : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public const string DefaultHierarchyDelimiter = "/";
        protected string _displayName;

        private Mailbox()
            : this(string.Empty, null, string.Empty, new NameAttributesCollection(new string[0]))
        {

        }

        private Mailbox(string name, NameAttributesCollection attributes)
            : this(name, null, name, attributes)
        {

        }

        private Mailbox(string name, Mailbox parent, NameAttributesCollection attributes)
            : this(name, parent, name, attributes)
        {

        }

        private Mailbox(string name, Mailbox parent, string displayName, NameAttributesCollection attributes)
        {
            this.Parent = parent;
            this._displayName = displayName;
            this.Name = name;
            this.HierarchyDelimiter = "/";
            this.FullName = (parent == null ? name : parent.FullName + HierarchyDelimiter + name);
            this.Children = new MailboxCollection();
            this.Folder = GetFolder(Name);
        }

        private static Mailbox AssembleTree(IEnumerable<KeyValuePair<string[], Mailbox>> paths)
        {
            Mailbox item = new Mailbox();
            Stack<Mailbox> stack = new Stack<Mailbox>();
            int num = 1;
            stack.Push(item);
            foreach (KeyValuePair<string[], Mailbox> pair in paths)
            {
                while (num > pair.Key.Length)
                {
                    stack.Pop();
                    num--;
                }
                Mailbox mailbox2 = stack.Peek();
                mailbox2.Children.Add(pair.Value);
                pair.Value.Parent = mailbox2;
                stack.Push(pair.Value);
                num++;
            }
            return item;
        }

        private static List<KeyValuePair<string[], Mailbox>> BuildPathsList(IEnumerable<MatchedName> folderPathCollection)
        {
            List<KeyValuePair<string[], Mailbox>> list = new List<KeyValuePair<string[], Mailbox>>();
            foreach (MatchedName name in folderPathCollection)
            {
                string str = (name.HierarchyDelimeter == "NIL") ? "/" : name.HierarchyDelimeter;
                string[] key = name.Name.Split(new string[] { str }, StringSplitOptions.None);
                Mailbox mailbox = new Mailbox(key[key.Length - 1], name.Attributes) {
                    FullName = name.Name,
                    HierarchyDelimiter = str
                };
                list.Add(new KeyValuePair<string[], Mailbox>(key, mailbox));
            }
            return list;
        }

        public static Mailbox BuildTree(MatchedNameCollection folderPathCollection)
        {
            List<KeyValuePair<string[], Mailbox>> paths = BuildPathsList(folderPathCollection);
            IComparer<KeyValuePair<string[], Mailbox>> comparer = new PathComparer();
            paths.Sort(comparer);
            return AssembleTree(paths);
        }

        public static Mailbox Find(Mailbox root, string name)
        {
            string str = StringEncoding.EncodeMailboxName(name);
            if (root.Name == str)
            {
                return root;
            }
            foreach (Mailbox mailbox in root.Children)
            {
                Mailbox mailbox2 = Find(mailbox, name);
                if (mailbox2 != null)
                {
                    return mailbox2;
                }
            }
            return null;
        }

        public static Mailbox NewMailbox(string name)
        {
            return new Mailbox(name, new NameAttributesCollection(new List<string>().AsEnumerable()));
        }

        public static Mailbox NewMailbox(string name, Mailbox mailbox)
        {
            return new Mailbox(name, mailbox, new NameAttributesCollection(new List<string>().AsEnumerable()));
        }

        public static Mailbox NewMailbox(string name, Mailbox mailbox, string displayName)
        {
            return new Mailbox(name, mailbox, displayName, new NameAttributesCollection(new List<string>().AsEnumerable()));
        }

        [DataMember]
        public MailboxCollection Children
        {
            get;
            private set;
        }

        [DataMember]
        public string DisplayName
        {
            get
            {
                return _displayName;
            }
            set
            {
                if (!IsSystem
                 && !IsReserved)
                {
                    Name = value;
                    OnPropertyChanged(this, new PropertyChangedEventArgs("Name"));

                    Folder = GetFolder(Name);
                    OnPropertyChanged(this, new PropertyChangedEventArgs("Folder"));

                    FullName = (Parent == null ? Name : Parent.FullName + HierarchyDelimiter + Name);
                    OnPropertyChanged(this, new PropertyChangedEventArgs("FullName"));
                }
                if (_displayName != value)
                {
                    _displayName = value;
                    OnPropertyChanged(this, new PropertyChangedEventArgs("DisplayName"));
                }
            }
        }

        [DataMember]
        public string FullName
        {
            get;
            private set;
        }

        [DataMember]
        public string HierarchyDelimiter
        {
            get;
            private set;
        }

        [DataMember]
        public string Name
        {
            get;
            private set;
        }

        [DataMember]
        public Mailbox Parent
        {
            get;
            private set;
        }

        [DataMember]
        public MailboxFolders Folder
        {
            get;
            private set;
        }

        public bool IsSystem
        {
            get
            {
                if (string.IsNullOrEmpty(Name))
                    return false;
                return Name.ToLower().Contains("[gmail]");
            }
        }

        public bool IsReserved
        {
            get
            {
                return Folder != MailboxFolders.Folder;
            }
        }

        public override bool Equals(object obj)
        {
            Mailbox mailbox = (Mailbox)obj;
            return (FullName ?? Name).Equals(mailbox.FullName ?? mailbox.Name, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            return (FullName ?? Name).ToLower().GetHashCode();
        }

        public Mailbox Copy()
        {
            return (Mailbox)base.MemberwiseClone();
        }

        private class PathComparer : IComparer<KeyValuePair<string[], Mailbox>>
        {
            public int Compare(KeyValuePair<string[], Mailbox> x, KeyValuePair<string[], Mailbox> y)
            {
                int index = 0;
                while (((x.Key.Length > index) && (y.Key.Length > index)) && (x.Key[index] == y.Key[index]))
                {
                    index++;
                }
                if ((x.Key.Length > index) && (y.Key.Length > index))
                {
                    return x.Key[index].CompareTo((string) y.Key[index]);
                }
                return (x.Key.Length - y.Key.Length);
            }
        }

        private MailboxFolders GetFolder(string name)
        {
            switch (name.ToLower())
            {
                case "inbox":
                    DisplayName = "Inbox";
                    return MailboxFolders.Inbox;

                case "drafts":
                    return MailboxFolders.Drafts;

                case "outbox":
                    return MailboxFolders.Outbox;

                case "sent items":
                case "sent mail":
                case "sent":
                    return MailboxFolders.SentItems;

                case "junk mail":
                case "junk":
                case "spam":
                    return MailboxFolders.JunkMail;

                case "deleted items":
                case "trash":
                    return MailboxFolders.DeletedItems;

                default:
                    return MailboxFolders.Folder;
            }
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }
    }
}

