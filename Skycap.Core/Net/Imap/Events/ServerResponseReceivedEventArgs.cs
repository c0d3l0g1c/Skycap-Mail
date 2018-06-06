namespace Skycap.Net.Imap.Events
{
    using Skycap.Net.Imap.Responses;
    using System;

    public class ServerResponseReceivedEventArgs : EventArgs
    {
        private readonly IMAP4Response _response;

        public ServerResponseReceivedEventArgs(IMAP4Response response)
        {
            this._response = response;
        }

        public IMAP4Response ReceivedResponse
        {
            get
            {
                return this._response;
            }
        }
    }
}

