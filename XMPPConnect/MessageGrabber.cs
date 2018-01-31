using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XMPPConnect.MainClasses;

namespace XMPPConnect
{
    public class MessageGrabber : PacketGrabber
    {
        private MessageCallback messageCB;
        public MessageGrabber(XmppClientConnection connection) : base(connection)
        {
            connection.OnMessage += new MessageHandler(_connectionOnMessage);
        }

        private void _connectionOnMessage(object sender, string msg)
        {
            if(!string.IsNullOrEmpty(msg))
            {
               
            }
        }
    }
}
