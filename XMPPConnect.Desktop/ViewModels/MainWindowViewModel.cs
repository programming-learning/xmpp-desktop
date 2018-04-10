using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Prism.Mvvm;
using Prism.Commands;
using XMPPConnect.Client;
using XMPPConnect.Desktop.Models;
using XMPPConnect.Desktop.Views;

namespace XMPPConnect.Desktop.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private Models.Client _currentClient;
        private LoginForm _loginForm;
        private RosterList _rosterList;
        private Conversation _currentConversation;

        private DelegateCommand _loginCommand;
        private DelegateCommand<Window> _openWindowCommand;
        private DelegateCommand<string> _addContactCommand;
        private DelegateCommand<int?> _removeContactCommand;
        private DelegateCommand<int?> _sendMessageCommand;

        private XmppClientConnection _connection;

        //public JabberID CurrentUsername => _currentClient.JabberId;
        public ReadOnlyObservableCollection<string> Contacts => _rosterList.UserContacts;

        public LoginForm LoginForm
        {
            get { return _loginForm; }
        }

        public Models.Client CurrentClient
        {
            get { return _currentClient; }
            set
            {
                if (_currentClient == null)
                {
                    _currentClient = value;
                }
            }
        }

        public ConnectionState ConnectionState { get; set; }

        public Conversation CurrentConversation
        {
            get { return _currentConversation; }
            set
            {
                if (_currentConversation == null)
                {
                    _currentConversation = value;
                }
            }
        }

        public MainWindowViewModel()
        {
            _loginForm = new LoginForm();
            _rosterList = new RosterList();
            //TODO: delete then
            _rosterList.AddContact("katepleh@jabber.ru");
            _currentConversation = new Conversation { PartnerIsChosen = false };
            _currentClient = new Models.Client();
            ConnectionState = new ConnectionState { Connected = false };

            _currentClient.PropertyChanged += (s, e) => { RaisePropertyChanged(e.PropertyName); };
            _rosterList.PropertyChanged += (s, e) => { RaisePropertyChanged(e.PropertyName); };
            _currentConversation.PropertyChanged += (s, e) => { RaisePropertyChanged(e.PropertyName); };
        }

        public DelegateCommand<string> AddContactCommand
        {
            get
            {
                return _addContactCommand ??
                       (_addContactCommand = new DelegateCommand<string>(str => { _rosterList.AddContact(str); }));
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
                               _rosterList.RemoveContact(ind.Value);
                           }
                       }));
            }
        }

        public DelegateCommand LoginCommand
        {
            get
            {
                return _loginCommand ??
                       (_loginCommand = new DelegateCommand(async () =>
                       {
                           if (_currentClient == null)
                           {
                               _currentClient = new Models.Client();
                           }

                           JabberID id = new JabberID(_loginForm.Jid);
                           _connection = new XmppClientConnection(id, _loginForm.Password);

                           await _connection.LoginAsync();
                           if (_connection.Connected && _connection.Authenticated)
                           {
                               MessageBox.Show("Вы успешно авторизованы.");
                               CurrentClient.JabberId = id;
                               CurrentClient.Password = _loginForm.Password;
                               ConnectionState.Connected = true;
                               CurrentConversation.Connection = _connection;
                           }
                           else
                           {
                               MessageBox.Show("Произошла ошибка, попробуйте еще раз");
                           }

                           _connection.Send(new Presence(ShowType.Show));
                       }));
            }
        }

        public DelegateCommand<int?> SendMessageCommand
        {
            get
            {
                return _sendMessageCommand ??
                       (_sendMessageCommand = new DelegateCommand<int?>((partnerIndex) =>
                       {
                           if (partnerIndex.Value >= 0)
                           {
                               CurrentConversation.PartnerJid = Contacts.ElementAt(partnerIndex.Value);
                               Message message = new Message(
                                   CurrentClient.JabberId,
                                   new JabberID(CurrentConversation.PartnerJid),
                                   CurrentConversation.MessageToSend);
                               CurrentConversation.ChatField += "[" + DateTime.Now.ToLongTimeString() + "]" + "<" + CurrentClient.JabberId.Username + "> " + CurrentConversation.MessageToSend + Environment.NewLine;
                               CurrentConversation.MessageToSend = string.Empty;
                               _connection.Send(message);
                           }
                           else
                           {
                               MessageBox.Show("Вы не выбрали собеседника.");
                           }
                       }));
            }
        }

        public DelegateCommand<Window> OpenAuthorizationWindowCommand
        {
            get
            {
                return _openWindowCommand ??
                       (_openWindowCommand = new DelegateCommand<Window>((parentWindow) =>
                       {
                           Window authWindow = new AuthorizationWindow();
                           authWindow.Owner = parentWindow;
                           authWindow.DataContext = parentWindow.DataContext;
                           authWindow.Show();
                       }));
            }
        }
    }
}
