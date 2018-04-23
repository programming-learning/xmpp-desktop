using System;
using System.Collections.ObjectModel;
using System.Linq;
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
        private XmppClientConnection _connection;

        private ClientVModel _currentClientVModel;
        private AuthorizationVModel _authorizationVModel;
        private RosterVModel _rosterVModel;
        private RosterContactVModel _currentPartner;

        private AuthorizeClientCommand _authorizeClientCommand;
        private SendMessageCommand _sendMessageCommand;
        private DelegateCommand<AuthorizationVModel> _openWindowCommand;
        private DelegateCommand<string> _addContactCommand;
        private DelegateCommand<int?> _removeContactCommand;

        #region Other VModels

        public ReadOnlyObservableCollection<RosterContactVModel> Contacts => _rosterVModel.UserContacts;

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

        public RosterContactVModel CurrentPartner
        {
            get { return _currentPartner; }
            set
            {
                _currentPartner = value;
                RaisePropertyChanged("CurrentPartner");
            }
        }

        #endregion

        public XmppClientVModel()
        {
            _sendMessageCommand = new SendMessageCommand();
            _connection = new XmppClientConnection();
            _authorizationVModel = new AuthorizationVModel();
            _rosterVModel = new RosterVModel();
            _currentPartner = new RosterContactVModel();
            //TODO: delete then
            _rosterVModel.AddContact("katepleh@jabber.ru");
            _rosterVModel.AddContact("egorbir@jabber.ru");
            //

            _currentClientVModel = new ClientVModel();
            ConnectionStateVModel = new ConnectionStateVModel { Connected = false };
        }

        #region Commands
        public DelegateCommand<string> AddContactCommand
        {
            get
            {
                return _addContactCommand ??
                       (_addContactCommand = new DelegateCommand<string>(str =>
                       {
                           if (!_rosterVModel.UserContacts.Any(c => c.JabberId.Full == str))
                           {
                               _rosterVModel.AddContact(str);
                           }
                           else
                           {
                               MessageBox.Show("Контакт с таким именем уже существует.");
                           }
                       }));
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
                    new AuthorizeClientRequestParams(_connection, _rosterVModel.UserContacts, _sendMessageCommand, _authorizationVModel,
                        _currentClientVModel, ConnectionStateVModel);

                return _authorizeClientCommand ??
                       (_authorizeClientCommand = new AuthorizeClientCommand(clientRequestParams));
            }
        }

        public SendMessageCommand SendMessageCommand
        {
            get { return _sendMessageCommand; }
        }

        public DelegateCommand<AuthorizationVModel> OpenAuthorizationWindowCommand
        {
            get
            {
                return _openWindowCommand ??
                       (_openWindowCommand = new DelegateCommand<AuthorizationVModel>((authorizationVModel) =>
                       {
                           Window authWindow = new AuthorizationWindow();

                           authWindow.DataContext = new
                           {
                               AuthorizationForm = authorizationVModel,
                               AuthorizeCommand = AuthorizeClientCommand
                           };
                           authWindow.Show();
                       }));
            }
        }

        #endregion
    }
}
