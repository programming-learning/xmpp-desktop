using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XMPPConnect.Client;

namespace XMPPConnect.Interfaces
{
    public interface IXmppConnection
    {
        void Login();
        void Logout();
        string Server { get; set; }
        int Port { get; set; }
        bool Authenticated { get; }

        event ObjectHandler OnLogin;
        event ObjectHandler OnBinded;
        event ErrorHanlder OnError;
        event ObjectHandler OnPresence;
        event MessageHandler OnMessage;
    }
}
