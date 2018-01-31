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

        public PacketGrabber(IXmppConnection conn)
        {
            _connection = conn;
        }
    }
}
