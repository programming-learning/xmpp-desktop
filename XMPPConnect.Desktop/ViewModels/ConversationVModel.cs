using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FirstFloor.ModernUI.Presentation;
using Prism.Mvvm;
using XMPPConnect.Client;

namespace XMPPConnect.Desktop.ViewModels
{
    public class ConversationVModel : BindableBase
    {
        private string _chatField;
        private string _messageToSend;
        private Message _messageToReceive;
        private bool _partnerIsChosen;
        private string _partnerJid;
        private MessageGrabber _messageGrabber;
        private XmppClientConnection _connection;

        public XmppClientConnection Connection
        {
            get { return _connection; }
            set { _connection = value; }
        }

        public string ChatField {
            get { return _chatField; }
            set
            {
                _chatField = value;
                RaisePropertyChanged("ChatField");
            }
        }

        public string PartnerJid
        {
            get { return _partnerJid; }
            set
            {
                _partnerJid = value;
                RaisePropertyChanged("PartnerJid");
                if (Connection != null && MessageGrabber == null)
                {
                    _messageGrabber = new MessageGrabber(Connection);
                    _messageGrabber.Add(new JabberID(PartnerJid), OnMessage);
                }              
            }
        }

        public string MessageToSend
        {
            get { return _messageToSend; }
            set
            {
                _messageToSend = value;
                RaisePropertyChanged("MessageToSend");
            }
        }

        public Message MessageToReceive
        {
            get { return _messageToReceive; }
            set
            {
                _messageToReceive = value;
                RaisePropertyChanged("MessageToReceive");
            }
        }

        public bool PartnerIsChosen
        {
            get { return _partnerIsChosen; }
            set
            {
                _partnerIsChosen = value;
                RaisePropertyChanged("PartnerIsChosen");
            }
        }

        public MessageGrabber MessageGrabber
        {
            get { return _messageGrabber; }
        }

        private void OnMessage(object sender, Message msg)
        {
            MessageToReceive = msg;
            ChatField += "[" + DateTime.Now.ToLongTimeString() + "]" + "<" + msg.From.Username + "> " + msg.Body + Environment.NewLine;
        }
    }
}
