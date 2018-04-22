using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;

namespace XMPPConnect.Desktop.ViewModels
{
    public class ConnectionVModel : BindableBase
    {
        private XmppClientConnection _connection;

        public XmppClientConnection ClientConnection
        {
            get { return _connection; }
            set
            {
                _connection = value;
                RaisePropertyChanged("ClientConnection");
            }
        }

        public bool Connected
        {
            get { return _connection.Connected; }
        }
    }
}
