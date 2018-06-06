namespace Skycap.Net.Smtp.ServerActions
{
    using Skycap.Net.Common.Connections;
    using Skycap.Net.Smtp;
    using System;
    using System.Net;
    using System.Net.NetworkInformation;

    public class EHLOCommand : ISmtpAction
    {
        protected const string CommandPattern = "EHLO {0}";

        public static string GetFQDN()
        {
            string domainName = "";//TODO:IPGlobalProperties.GetIPGlobalProperties().DomainName;
            string hostName = "127.0.0.1";//TODO:Dns.GetHostName();
            string text = "";
            if (!hostName.Contains(domainName))
            {
                text = hostName + "." + domainName;
            }
            else
            {
                text = hostName;
            }
            if (MailMessageRFCEncoder.IsAsciiCharOnly(text))
            {
                return text;
            }
            char[] chArray = text.ToCharArray();
            for (int i = 0; i < chArray.Length; i++)
            {
                if (chArray[i] > '\x007f')
                {
                    chArray[i] = 'X';
                }
            }
            return new string(chArray);
        }

        public virtual SmtpResponse Interact(IConnection connection)
        {
            string textLine = string.Format("EHLO {0}", GetFQDN());
            connection.SendLine(textLine);
            return new SmtpResponse(connection);
        }
    }
}

