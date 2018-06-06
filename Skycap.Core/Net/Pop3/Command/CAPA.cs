namespace Skycap.Net.Pop3.Command
{
    using Skycap.Net.Common.Connections;
    using Skycap.Net.Pop3;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text.RegularExpressions;

    [DebuggerDisplay("Commands = {Commands.Count}")]
    public class CAPA : Pop3Command
    {
        protected Dictionary<string, string> _commands;
        protected Regex regCommand = new Regex("([^ ]+) *(.*)", RegexOptions.IgnoreCase);

        public CAPA()
        {
            base.name = "CAPA";
            this.Commands = new Dictionary<string, string>();
        }

        public override Pop3Response Interact(IConnection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            connection.SendLine(this.CommandText);
            Pop3Response response = new Pop3Response(connection.ReceiveLine());
            if (response.Type == EPop3ResponseType.OK)
            {
                string str;
                while ((str = connection.ReceiveLine()) != ".")
                {
                    Match match = this.regCommand.Match(str);
                    if (match.Groups.Count > 2)
                    {
                        this.Commands.Add(match.Groups[1].Value, match.Groups[2].Value);
                    }
                    else if (match.Groups.Count > 1)
                    {
                        this.Commands.Add(match.Groups[1].Value, "");
                    }
                }
            }
            return response;
        }

        public virtual Dictionary<string, string> Commands
        {
            get
            {
                return this._commands;
            }
            protected set
            {
                this._commands = value;
            }
        }
    }
}

