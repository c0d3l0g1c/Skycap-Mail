namespace Skycap.Net.Imap.Commands
{
    using Skycap.Net.Imap;
    using Skycap.Net.Imap.Responses;
    using Skycap.Net.Imap.Sequences;
    using System;
    using System.Collections.Generic;
    using System.Text;

    internal class STORECommand : IMAP4BaseCommand
    {
        protected const string _exUnknownMode = "Unknown mode";
        private readonly IEnumerable<string> _flags;
        private readonly EFlagMode _mode;
        private readonly ISequence _sequence;

        public STORECommand(ISequence sequence, IEnumerable<string> flags, EFlagMode mode)
        {
            this._sequence = sequence;
            this._mode = mode;
            this._flags = flags;
        }

        protected override CompletionResponse Behaviour()
        {
            IMAP4Response response;
            string flagSetMode = GetFlagSetMode(this._mode);
            string command = string.Format("UID STORE {0} {1}FLAGS{2} ({3})", new object[] { this._sequence, flagSetMode, ".SILENT", JoinFlags(this._flags) });
            uint commandId = base._dispatcher.SendCommand(command);
            do
            {
                response = base._dispatcher.GetResponse(commandId);
            }
            while (!CompletionResponse.IsCompletionResponse(response.Response));
            return new CompletionResponse(response.Response);
        }

        private static string GetFlagSetMode(EFlagMode mode)
        {
            switch (mode)
            {
                case EFlagMode.Add:
                    return "+";

                case EFlagMode.Remove:
                    return "-";

                case EFlagMode.Replace:
                    return "";
            }
            throw new ArgumentException("Unknown mode", "mode");
        }

        private static string JoinFlags(IEnumerable<string> flags)
        {
            bool flag = true;
            StringBuilder builder = new StringBuilder();
            foreach (string str in flags)
            {
                builder.Append(!flag ? @" \" : @"\");
                builder.Append(str);
                flag = false;
            }
            return builder.ToString();
        }
    }
}

