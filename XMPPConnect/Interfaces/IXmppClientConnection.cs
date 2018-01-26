using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMPPConnect.Managers
{
    public interface IXmppClientConnection
    {
        void Open();
        void Connect();
        string Server { get; set; }
        int Port { get; set; }
        bool Authenticated { get; }
    }
}
