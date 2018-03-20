using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XMPPConnect.Client;

namespace XMPPConnect.Strategies
{
    internal class MessageResolver : IDataResolveStrategy
    {
        public void InvokeOnData(object sender, object dataObject)
        {
            Message message = (Message)dataObject;
            XmppClientConnection connection = (XmppClientConnection)sender;

            connection.InvokeOnMessage(message);
        }
    }
}
