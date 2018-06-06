namespace Skycap.Net.Smtp
{
    using System;

    public enum ESmtpCommandResultCode
    {
        APasswordTransitionIsNeeded = 0x1b0,
        AuthenticationRequired = 530,
        AuthenticationSuccessful = 0xeb,
        BadSequenceOfCommands = 0x1f7,
        CannotVerifyUser_ButWillAcceptMessageAndAttemptDelivery = 0xfc,
        CommandNotImplemented = 0x1f6,
        CommandParameterNotImplemented = 0x1f8,
        EncryptionRequiredForRequestedAuthenticationMechanism = 0x21a,
        HelpMessage = 0xd6,
        None = 0,
        RequestedActionAborted_ErrorInProcessing = 0x1c3,
        RequestedActionNotTaken_MailboxNameNotAllowed = 0x229,
        RequestedActionNotTaken_MailboxUnavailable = 550,
        RequestedMailActionAborted_ExceededStorageAllocation = 0x228,
        RequestedMailActionNotTaken_MailboxUnavailable = 450,
        RequestedMailActionOkay_Completed = 250,
        ServiceClosingTransmissionChannel = 0xdd,
        ServiceNotAvailable_ClosingTransmissionChannel = 0x1a5,
        ServiceReady = 220,
        StartMailInput = 0x162,
        SyntaxError_CommandUnrecognized = 500,
        SyntaxErrorInParametersOrArguments = 0x1f5,
        SystemStatus_OrSystemHelpReply = 0xd3,
        TemporaryAuthenticationFailure = 0x1c6,
        TransactionFailed = 0x22a,
        UserNotLocal_PleaseTry = 0x227,
        UserNotLocal_WillForwardTo = 0xfb,
        WaitingForAuthentication = 0x14e
    }
}

