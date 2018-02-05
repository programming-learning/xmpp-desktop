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
        private MessageCallback _messageCB;
        public MessageGrabber(XmppClientConnection connection) : base(connection)
        {
            connection.OnMessage += new MessageHandler(_connectionOnMessage);
        }

        public void Add(JabberID jid, MessageCallback callback)
        {
            lock(_interlocutors)
            {
                if(_interlocutors.ContainsKey(jid.ToString()))
                {
                    return;
                }
            }

            _messageCB += callback;
            
            lock(_interlocutors)
            {
                _interlocutors.Add(jid.ToString(), null);
            }
        }

        private void _connectionOnMessage(object sender, Message msg)
        {
            if(msg != null)
            {
                lock(_interlocutors)
                {
                    IDictionaryEnumerator tableEnum = _interlocutors.GetEnumerator();
                    while(tableEnum.MoveNext())
                    {
                        if(msg.From == new JabberID((string)tableEnum.Key))
                        {
                            _messageCB?.Invoke(this, msg);
                        }
                    }
                }
            }
        }
    }
}
