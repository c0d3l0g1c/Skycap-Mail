namespace Skycap.Net.Imap.Commands
{
    using Skycap.Net.Common.Configurations;
    using Skycap.Net.Common.Exceptions;
    using System;
    using System.Collections.Generic;

    internal class AuthenticationMethodFactory
    {
        public static BaseAUTHENTICATECommand CreateAuthenticateCommand(EAuthenticationType type, string username, string password)
        {
            return CreateAuthenticateCommand(GetAuthenticationMethodName(type), username, password);
        }

        public static BaseAUTHENTICATECommand CreateAuthenticateCommand(string methodName, string username, string password)
        {
            if (methodName == "PLAIN")
            {
                return new PLAINAUTHENTICATECommand(username, password);
            }
            if (methodName == "LOGIN")
            {
                return new LOGINAUTHENTICATECommand(username, password);
            }
            if (methodName != "DIGEST-MD5")
            {
                throw new AuthenticationMethodNotSupportedException(methodName);
            }
            // TODO:
            return null;
        }

        public static string GetAuthenticationMethodName(EAuthenticationType type)
        {
            switch (type)
            {
                case EAuthenticationType.Plain:
                    return "PLAIN";

                case EAuthenticationType.Login:
                    return "LOGIN";
            }
            return "NONE";
        }

        public static BaseAUTHENTICATECommand GetBestAuthenticateCommand(List<string> capabilities, string username, string password)
        {
            if (capabilities.Contains("LOGIN"))
            {
                return new LOGINAUTHENTICATECommand(username, password);
            }
            if (capabilities.Contains("PLAIN"))
            {
                return new PLAINAUTHENTICATECommand(username, password);
            }
            return new LOGINCommand(username, password);
        }
    }
}

