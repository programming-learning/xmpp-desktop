using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XMPPConnect.Desktop.ViewModels;

namespace XMPPConnect.Desktop.Infrastructure.RequestParams
{
    public class SendMessageRequestParams
    {
        private XmppClientConnection _connection;
        private ClientVModel _clientVModel;
        private ConversationVModel _conversationVModel;

        public SendMessageRequestParams(XmppClientConnection connection,ClientVModel clientVModel, ConversationVModel conversationVModel)
        {
            _connection = connection;
            _clientVModel = clientVModel;
            _conversationVModel = conversationVModel;
        }

        public XmppClientConnection Connection
        {
            get { return _connection; }
            set { _connection = value; }
        }

        public ClientVModel Client
        {
            get { return _clientVModel; }
            set { _clientVModel = value; }
        }

        public ConversationVModel Conversation
        {
            get { return _conversationVModel; }
            set { _conversationVModel = value; }
        }
    }
}
