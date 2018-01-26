using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XMPPConnect.Interfaces;
using XMPPConnect.Managers;
using System.Net.Sockets;
using XMPPConnect.Net;

namespace XMPPConnect.MainClasses
{
    public class XmppClientConnection : IXmppClientConnection
    {
        private IJabberId _jid;
        private ClientSocket _clientSocket;
        private bool _authenticated;

        public event ObjectHandler OnLogin;
        public event ObjectHandler OnBinded;
        public event ErrorHanlder OnError;
        public event ErrorHanlder OnAuthError;
        public event ObjectHandler OnPresence;
        public event ObjectHandler OnMessage;

        public XmppClientConnection()
        {
            _authenticated = false;
            Port = 5222;
        } 

        public XmppClientConnection(IJabberId jid, string password)
        {
            _jid = jid;
            Server = jid.Server;
        }

        public void Open()
        {
            if(string.IsNullOrEmpty(Server))
            {
                throw new Exception("Server was not assign");
            }

            _clientSocket = new ClientSocket();
            try
            {
                _clientSocket.Connect(Server, Port);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public void Connect()
        {

        }

        public string Server { get; set; }
        public int Port { get; set; }
        public bool Authenticated
        {
            get
            {
                return _authenticated;
            }
        }
    }
}
