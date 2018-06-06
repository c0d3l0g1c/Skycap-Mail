namespace Skycap.Net.Smtp
{
    using Skycap.Net.Common.Connections;
    using System;

    public class AuthCommandResponse : SmtpResponse
    {
        protected EAuthCommandStage _stage;

        public AuthCommandResponse(IConnection connection, EAuthCommandStage stage) : base(connection)
        {
            this.Stage = stage;
        }

        public EAuthCommandStage Stage
        {
            get
            {
                return this._stage;
            }
            private set
            {
                this._stage = value;
            }
        }
    }
}

