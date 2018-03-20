using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XMPPConnect.Client;

namespace XMPPConnect.Strategies
{
    internal class PresenceResolver : IDataResolveStrategy
    {
        public void InvokeOnData(object sender, object dataObject)
        {
            Presence presence = (Presence) dataObject;
            XmppClientConnection connection = (XmppClientConnection) sender;

            connection.InvokeOnPresence(presence);
        }
    }
}
