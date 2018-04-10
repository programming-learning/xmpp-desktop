using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XMPPConnect.Client;

namespace XMPPConnect
{
    public interface IXmppConnection
    {
        bool Authenticated { get; }

        void Login();
        void Logout();

        event ObjectHandler OnLogin;
        event ObjectHandler OnBinded;
        event ErrorHanlder OnError;
        event PresenceHandler OnPresence;
        event MessageHandler OnMessage;
    }
}
