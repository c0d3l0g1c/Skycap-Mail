using System;

namespace Skycap.Data
{
    /// <summary>
    /// Represents the various well known email servers.
    /// </summary>
    public enum WellKnownIncomingEmailServer
    {
        /// <summary>
        /// The imap protocol.
        /// </summary>
        Imap,
        /// <summary>
        /// The imap4 protocol.
        /// </summary>
        Imap4,
        /// <summary>
        /// The pop protocol.
        /// </summary>
        Pop,
        /// <summary>
        /// The pop3 protocol.
        /// </summary>
        Pop3,
        /// <summary>
        /// The mail protocol.
        /// </summary>
        Mail,
    }

    /// <summary>
    /// Represents the various well known outgoing email servers.
    /// </summary>
    public enum WellKnownOutgoingEmailServer
    { 
        /// <summary>
        /// The smtp protocol.
        /// </summary>
        Smtp,
        /// <summary>
        /// The smtp auth protocol.
        /// </summary>
        SmtpAuth,
        /// <summary>
        /// The mail protocol.
        /// </summary>
        Mail,
    }
}
