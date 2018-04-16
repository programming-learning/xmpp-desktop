using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XMPPConnect;

namespace XMPPConnect
{
    public abstract class PacketGrabber
    {
        protected IXmppConnection _connection;
        protected Hashtable _data;

        public PacketGrabber(IXmppConnection conn)
        {
            _connection = conn;
            _data = new Hashtable();
        }

        public void Clear()
        {
            _data.Clear();
        }

        public void Remove(JabberID jid)
        {
            _data.Remove(jid.ToString());
        }
    }
}
