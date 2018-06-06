namespace Skycap.Net.Pop3.Command
{
    using Skycap.Net.Common.Connections;
    using Skycap.Net.Pop3;
    using System;

    public class STLS : Pop3Command
    {
        public STLS()
        {
            base.name = "STLS";
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
                connection.SwitchToSslChannel();
            }
            return response;
        }
    }
}

