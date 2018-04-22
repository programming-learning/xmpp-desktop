using System.Collections.ObjectModel;
using System.Windows;
using Prism.Mvvm;
using Prism.Commands;
using XMPPConnect.Client;
using XMPPConnect.Desktop.Infrastructure.Commands;
using XMPPConnect.Desktop.Infrastructure.RequestParams;
using XMPPConnect.Desktop.Views;

namespace XMPPConnect.Desktop.ViewModels
{
    public class XmppClientVModel : BindableBase
    {
        private ClientVModel _currentClientVModel;
        private AuthorizationVModel _authorizationVModel;
        private RosterVModel _rosterVModel;
        private ConversationVModel _currentConversationVModel;
        private ConnectionVModel _connectionVModel;

        private AuthorizeClientCommand _authorizeClientCommand;
        private SendMessageCommand _sendMessageCommand;
        private DelegateCommand<AuthorizationVModel> _openWindowCommand;
        private DelegateCommand<string> _addContactCommand;
        private DelegateCommand<int?> _removeContactCommand;

        private XmppClientConnection _connection;
        //public XmppClientConnection ClientConnection
        //{
        //    get { return _connectionVModel.ClientConnection; }
        //    set { _connectionVModel.ClientConnection = value; }
        //}

        public ReadOnlyObservableCollection<string> Contacts => _rosterVModel.UserContacts;

        public AuthorizationVModel AuthorizationVModel
        {
            get { return _authorizationVModel; }
        }

        public ClientVModel CurrentClient
        {
            get { return _currentClientVModel; }
            set
            {
                if (_currentClientVModel == null)
                {
                    _currentClientVModel = value;
                }
            }
        }

        public ConnectionStateVModel ConnectionStateVModel { get; set; }

        public ConversationVModel CurrentConversationVModel
        {
            get { return _currentConversationVModel; }
            set
            {
                if (_currentConversationVModel == null)
                {
                    _currentConversationVModel = value;
                }
            }
        }

        public XmppClientVModel()
        {
            _sendMessageCommand = new SendMessageCommand();
            _connection = new XmppClientConnection();
            _authorizationVModel = new AuthorizationVModel();
            _rosterVModel = new RosterVModel();
            _connectionVModel = new ConnectionVModel();
            //TODO: delete then
            _rosterVModel.AddContact("katepleh@jabber.ru");
            //
            _currentConversationVModel = new ConversationVModel { PartnerIsChosen = false };
            _currentClientVModel = new ClientVModel();
            ConnectionStateVModel = new ConnectionStateVModel { Connected = false };

            //_currentClientVModel.PropertyChanged += (s, e) => { RaisePropertyChanged(e.PropertyName); };
            //_rosterVModel.PropertyChanged += (s, e) => { RaisePropertyChanged(e.PropertyName); };
            //_currentConversationVModel.PropertyChanged += (s, e) => { RaisePropertyChanged(e.PropertyName); };
        }

        public DelegateCommand<string> AddContactCommand
        {
            get
            {
                return _addContactCommand ??
                       (_addContactCommand = new DelegateCommand<string>(str => { _rosterVModel.AddContact(str); }));
            }
        }

        public DelegateCommand<int?> RemoveContactCommand
        {
            get
            {
                return _removeContactCommand ??
                       (_removeContactCommand = new DelegateCommand<int?>(ind =>
                       {
                           if (ind.HasValue)
                           {
                               _rosterVModel.RemoveContact(ind.Value);
                           }
                       }));
            }
        }

        public AuthorizeClientCommand AuthorizeClientCommand
        {
            get
            {
                AuthorizeClientRequestParams clientRequestParams =
                    new AuthorizeClientRequestParams(_connection, _sendMessageCommand, _authorizationVModel,
                        _currentClientVModel, _currentConversationVModel, ConnectionStateVModel);

                return _authorizeClientCommand ??
                       (_authorizeClientCommand = new AuthorizeClientCommand(clientRequestParams));
            }
        }

        public SendMessageCommand SendMessageCommand
        {
            get
            {
                //if (_sendMessageCommand == null)
                //{
                //    SendMessageRequestParams requestParams =
                //        new SendMessageRequestParams(_connection, _currentClientVModel, _currentConversationVModel);
                //    _sendMessageCommand = new SendMessageCommand(requestParams);
                //}

                return _sendMessageCommand;
            }
        }

        public DelegateCommand<AuthorizationVModel> OpenAuthorizationWindowCommand
        {
            get
            {
                return _openWindowCommand ??
                       (_openWindowCommand = new DelegateCommand<AuthorizationVModel>((authorizationVModel) =>
                       {
                           Window authWindow = new AuthorizationWindow();

                           //authWindow.Owner = parentWindow;
                           authWindow.DataContext = new
                           {
                               AuthorizationForm = authorizationVModel,
                               AuthorizeCommand = AuthorizeClientCommand
                           }; //new AuthorizationVModel() { Current = connectionViewModel };
                           authWindow.Show();
                       }));
            }
        }
    }
}
