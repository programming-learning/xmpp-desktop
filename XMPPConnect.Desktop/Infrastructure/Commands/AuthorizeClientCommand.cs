using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using XMPPConnect.Client;
using XMPPConnect.Desktop.Infrastructure.RequestParams;
using XMPPConnect.Desktop.ViewModels;

namespace XMPPConnect.Desktop.Infrastructure.Commands
{
    public class AuthorizeClientCommand : ICommand
    {
        private AuthorizeClientRequestParams _requestParams;

        public AuthorizeClientCommand(AuthorizeClientRequestParams requestParams)
        {
            _requestParams = requestParams;
        }

        public bool CanExecute(object parameter)
        {
            Contract.Requires(_requestParams != null);

            if (string.IsNullOrEmpty(_requestParams.AuthorizeCredentials.Jid) ||
                string.IsNullOrEmpty(_requestParams.AuthorizeCredentials.Password))
            {
                return false;
            }

            return true;
        }

        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }

        public void Execute(object parameter)
        {
            Contract.Requires(_requestParams != null);
            Execute(_requestParams);
        }

        public async Task Execute(AuthorizeClientRequestParams requestParams)
        {
            Contract.Requires(requestParams != null);
            if (!JabberID.ValidateJid(_requestParams.AuthorizeCredentials.Jid))
            {
                MessageBox.Show("Введен некорректный идентификатор пользователя.");
                return;
            }

            XmppClientConnection connection = requestParams.Connection;
            ClientVModel client = requestParams.Client;
            //ConversationVModel conversation = requestParams.Conversation;
            ConnectionStateVModel connectionState = requestParams.ConnectionState;
            AuthorizationVModel credentials = requestParams.AuthorizeCredentials;
            ReadOnlyObservableCollection<RosterContactVModel> contacts = requestParams.Contacts;

            JabberID id = new JabberID(credentials.Jid);
            connection = new XmppClientConnection(id, credentials.Password);

            await connection.LoginAsync();
            if (connection.Connected && connection.Authenticated)
            {
                MessageBox.Show("Вы успешно авторизованы.");
                client.JabberId = id;
                client.Password = credentials.Password;
                connectionState.Connected = true;
                
                //TODO: Всем контактам передать ссылку на текущее соединение
                //conversation.Connection = connection;
                foreach (var contact in contacts)
                {
                    contact.Conversation.InitMessageGrabber(connection, contact.JabberId);
                    //contact.Conversation.Connection = connection;
                    //contact.Conversation.MessageGrabber = new MessageGrabber(connection)
                    //    .Add(new JabberID(PartnerJid), OnMessage);
                }
            }
            else
            {
                MessageBox.Show("Произошла ошибка, попробуйте еще раз");
                return;
            }

            SendMessageCommand sendMessageCommand = _requestParams.SendMessageCommand;
            SendMessageRequestParams parameters = new SendMessageRequestParams(connection, client);
            sendMessageCommand.ExecuteParams = parameters;

            connection.Send(new Presence(ShowType.Show));
        }
    }
}
