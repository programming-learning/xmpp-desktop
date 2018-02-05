using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XMPPConnect.Interfaces;

namespace XMPPConnect
{
    public abstract class PacketGrabber
    {
        protected IXmppConnection _connection;
        protected Hashtable _interlocutors;

        public PacketGrabber(IXmppConnection conn)
        {
            _connection = conn;
            _interlocutors = new Hashtable();
        }

        public void Clear()
        {
            _interlocutors.Clear();
        }

        public void Remove(JabberID jid)
        {
            _interlocutors.Remove(jid.ToString());
        }
    }
}
