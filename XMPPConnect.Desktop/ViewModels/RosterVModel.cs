using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;

namespace XMPPConnect.Desktop.ViewModels
{
    public class RosterVModel : BindableBase
    {
        private readonly ObservableCollection<RosterContactVModel> _contacts;

        public readonly ReadOnlyObservableCollection<RosterContactVModel> UserContacts;

        public RosterVModel()
        {
            _contacts = new ObservableCollection<RosterContactVModel>();
            UserContacts = new ReadOnlyObservableCollection<RosterContactVModel>(_contacts);
        }

        public void AddContact(string jid)
        {
            RosterContactVModel contact = new RosterContactVModel
            {
                JabberId = new JabberID(jid),
                Conversation = new ConversationVModel()
            };
            _contacts.Add(contact);
        }

        public void RemoveContact(int index)
        {
            if (index >= 0 && index < _contacts.Count)
            {
                _contacts.RemoveAt(index);
            }
        }
    }
}
