using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;

namespace XMPPConnect.Desktop.ViewModels
{
    public class ConnectionStateVModel : BindableBase
    {
        private bool _connected;

        public bool Connected
        {
            get { return _connected; }
            set
            {
                _connected = value;
                RaisePropertyChanged("Connected");
            }
        }
    }
}
