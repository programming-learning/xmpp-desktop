using System;
using System.Collections.Generic;
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
    public class SendMessageCommand : ICommand
    {
        private SendMessageRequestParams _requestParams;

        public SendMessageCommand()
        {

        }

        public SendMessageCommand(SendMessageRequestParams requestParams)
        {
            _requestParams = requestParams;
        }

        public bool CanExecute(object parameter)
        {
            Contract.Requires(_requestParams != null && parameter != null);

            if ((parameter as RosterContactVModel) == null)
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
            Contract.Requires(_requestParams != null && parameter != null);
            RosterContactVModel contact = (RosterContactVModel)parameter;
            Execute(contact, _requestParams);
        }

        public void Execute(RosterContactVModel contact, SendMessageRequestParams requestParams)
        {
            XmppClientConnection connection = requestParams.Connection;
            ClientVModel client = requestParams.Client;
            ConversationVModel conversation = contact.Conversation;

            Message message = new Message(
                client.JabberId,
                contact.JabberId,
                conversation.MessageToSend);
            conversation.ChatField += "[" + DateTime.Now.ToLongTimeString() + "]" + "<" + client.JabberId.Username + "> "
                                      + conversation.MessageToSend + Environment.NewLine;
            conversation.MessageToSend = string.Empty;
            connection.Send(message);
        }

        public SendMessageRequestParams ExecuteParams
        {
            set { _requestParams = value; }
        }
    }
}
