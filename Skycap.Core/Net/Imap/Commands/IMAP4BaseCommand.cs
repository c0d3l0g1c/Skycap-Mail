namespace Skycap.Net.Imap.Commands
{
    using Skycap.Net.Imap;
    using Skycap.Net.Imap.Responses;
    using System;

    internal abstract class IMAP4BaseCommand
    {
        protected IInteractDispatcher _dispatcher;

        protected IMAP4BaseCommand()
        {
        }

        protected abstract CompletionResponse Behaviour();
        protected virtual void EndCommand()
        {
            this._dispatcher.ReleaseAccess();
        }

        public virtual CompletionResponse Interact(IInteractDispatcher dispatcher)
        {
            CompletionResponse response;
            this._dispatcher = dispatcher;
            this.StartCommand();
            try
            {
                response = this.Behaviour();
            }
            finally
            {
                this.EndCommand();
            }
            return response;
        }

        protected virtual void StartCommand()
        {
            this._dispatcher.GetAccess();
        }
    }
}

