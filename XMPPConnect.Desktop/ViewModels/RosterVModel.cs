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
        private readonly ObservableCollection<string> _contacts;

        public readonly ReadOnlyObservableCollection<string> UserContacts;

        public RosterVModel()
        {
            _contacts = new ObservableCollection<string>();
            UserContacts = new ReadOnlyObservableCollection<string>(_contacts);
        }

        public void AddContact(string jid)
        {
            _contacts.Add(jid);
            //RaisePropertyChanged("UserContacts");
        }

        public void RemoveContact(int index)
        {
            if (index >= 0 && index < _contacts.Count)
            {
                _contacts.RemoveAt(index);
                //RaisePropertyChanged("UserContacts");
            }
            //RaisePropertyChanged("Sum");
        }
    }
}
