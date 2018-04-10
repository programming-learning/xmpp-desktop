using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;
using XMPPConnect;

namespace XMPPConnect.Desktop.Models
{
    public class Client : BindableBase
    {
        private JabberID _jabberId;

        public JabberID JabberId
        {
            get { return _jabberId; }
            set
            {
                _jabberId = value;
                RaisePropertyChanged("JabberId");
            }
        }

        public string Password { get; set; }
    }
}
