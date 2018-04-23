using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XMPPConnect.Desktop.Infrastructure.Commands;
using XMPPConnect.Desktop.ViewModels;

namespace XMPPConnect.Desktop.Infrastructure.RequestParams
{
    public class AuthorizeClientRequestParams
    {
        private XmppClientConnection _connection;
        private ReadOnlyObservableCollection<RosterContactVModel> _contacts;
        private AuthorizationVModel _authorizationVModel;
        private ClientVModel _clientVModel;
        //private ConversationVModel _conversationVModel;
        private ConnectionStateVModel _connectionStateVModel;
        private SendMessageCommand _sendMessageCommand;

        public AuthorizeClientRequestParams(XmppClientConnection connection, ReadOnlyObservableCollection<RosterContactVModel> contacts,
            SendMessageCommand sendMessageCommand, AuthorizationVModel authorizationVModel,
            ClientVModel clientVModel, /*ConversationVModel conversationVModel,*/ ConnectionStateVModel connectionStateVModel)
        {
            _connection = connection;
            _sendMessageCommand = sendMessageCommand;
            _authorizationVModel = authorizationVModel;
            _clientVModel = clientVModel;
            //_conversationVModel = conversationVModel;
            _connectionStateVModel = connectionStateVModel;
            _contacts = contacts;
        }

        public XmppClientConnection Connection
        {
            get { return _connection; }
            set { _connection = value; }
        }

        public AuthorizationVModel AuthorizeCredentials
        {
            get { return _authorizationVModel; }
            set { _authorizationVModel = value; }
        }

        public ClientVModel Client
        {
            get { return _clientVModel; }
            set { _clientVModel = value; }
        }

        //public ConversationVModel Conversation
        //{
        //    get { return _conversationVModel; }
        //    set { _conversationVModel = value; }
        //}

        public ConnectionStateVModel ConnectionState
        {
            get { return _connectionStateVModel; }
            set { _connectionStateVModel = value; }
        }

        public ReadOnlyObservableCollection<RosterContactVModel> Contacts
        {
            get { return _contacts; }
            set { _contacts = value; }
        }

        public SendMessageCommand SendMessageCommand
        {
            get { return _sendMessageCommand; }
            set { _sendMessageCommand = value; }
        }
    }
}
