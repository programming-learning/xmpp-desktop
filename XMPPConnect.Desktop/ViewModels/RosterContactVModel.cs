using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;

namespace XMPPConnect.Desktop.ViewModels
{
    public class RosterContactVModel : BindableBase
    {
        private JabberID _jabberId;
        private ConversationVModel _conversation;
        private bool _isChosen;

        public ConversationVModel Conversation
        {
            get { return _conversation; }
            set
            {
                _conversation = value;
                RaisePropertyChanged("Conversation");
            }
        }

        public JabberID JabberId
        {
            get { return _jabberId; }
            set
            {
                _jabberId = value;
                RaisePropertyChanged("JabberId");
            }
        }
    }
}
