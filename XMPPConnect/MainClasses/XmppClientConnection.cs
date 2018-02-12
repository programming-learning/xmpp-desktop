using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XMPPConnect.Interfaces;
using XMPPConnect.Managers;
using System.Net.Sockets;
using XMPPConnect.Net;
using XMPPConnect.Helpers;
using XMPPConnect.Client;

namespace XMPPConnect.MainClasses
{
    public class XmppClientConnection : IXmppConnection
    {
        private JabberID _jid;
        private ClientSocket _clientSocket;
        private StanzaManager _stanzaManager;
        private bool _authenticated;
        private bool _connected;
        private string _response;
        private string _request;
        private string _errorMessage;
        private string _password;

        public event ObjectHandler OnLogin;
        public event ObjectHandler OnBinded;
        public event ErrorHanlder OnError;
        public event ObjectHandler OnPresence;
        public event MessageHandler OnMessage;

        public XmppClientConnection()
        {
            InitSocket();
            _authenticated = false;
            _connected = false;
            Port = 5222;
        }

        public XmppClientConnection(JabberID jid, string password) : this()
        {
            _jid = jid;
            _password = password;
            Server = jid.Server;
        }

        public void Login()
        {
            if (string.IsNullOrEmpty(Server))
            {
                throw new ArgumentException("Server was not assign");
            }

            try
            {
                _clientSocket.Connect(Server, Port);
                string handshake = _stanzaManager.GetXML(StanzaType.Header);
                string reqAuth = _stanzaManager.GetXML(StanzaType.Digest_auth);
                _clientSocket.Send(handshake);
                _clientSocket.Send(reqAuth);
                _clientSocket.Send(CryptographyHelper.DigestMD5AuthAlgo(_response, _jid, _password));
                string saslOn = _stanzaManager.GetXML(StanzaType.Sasl_on);
                _clientSocket.Send(saslOn);
                _clientSocket.Send(handshake);
                string bind = _stanzaManager.GetXML(StanzaType.Bind);
                _clientSocket.Send(bind);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public void Logout()
        {
            if (_clientSocket.Connected)
            {
                _clientSocket.Disconnect();
            }
            else
            {
                throw new SocketException();
            } 
        }

        public void Send(Stanza msg)
        {
            if(msg != null)
            {
                if(msg is Message)
                {
                    InvokeOnMessage((Message)msg);
                }
                else if(msg is Presence)
                {
                    InvokeOnPresence();
                }
                
                _clientSocket.Send(msg.ToString());
            }
            else
            {
                InvokeOnError(new NullReferenceException("Message instance is null."));
            }
        }

        private void ClientSocketOnRecieve(object sender, byte[] data, int length)
        {
            _response = Encoding.UTF8.GetString(data);
            if (_response.Contains("error"))
            {
                InvokeOnError(new Exception(_response));
            }
        }

        private void InitSocket()
        {
            _clientSocket = new ClientSocket();
            _clientSocket.OnSend += ClientSocketOnSend;
            _clientSocket.OnReceive += ClientSocketOnReceive;
            _clientSocket.OnConnect += ClientSocketOnConnect;
            _clientSocket.OnDisconnect += ClientSocketOnDisconnect;
            _clientSocket.OnError += ClientSocketOnError;
        }

#region Client Socket event handlers

        private void ClientSocketOnSend(object sender, byte[] data, int length)
        {
            _request = Encoding.UTF8.GetString(data, 0, length);
        }

        private void ClientSocketOnReceive(object sender, byte[] data, int length)
        {
            _response = Encoding.UTF8.GetString(data, 0, length);
        }

        private void ClientSocketOnConnect(object sender)
        {
            _connected = true;
        }

        private void ClientSocketOnDisconnect(object sender)
        {
            _connected = false;
        }

        private void ClientSocketOnError(object sender, Exception ex)
        {
            _errorMessage = ex.Message;
        }

        #endregion

        #region InvokeEvents
        private void InvokeOnError(Exception ex)
        {
            if (OnError != null)
            {
                OnError(this, ex);
            }
        }

        private void InvokeOnMessage(Message msg)
        {
            if (OnMessage != null)
            {
                OnMessage(this, msg);
            }
        }

        private void InvokeOnBinded()
        {
            if (OnBinded != null)
            {
                OnBinded(this);
            }
        }

        private void InvokeOnLogin()
        {
            if (OnLogin != null)
            {
                OnLogin(this);
            }
        }

        private void InvokeOnPresence()
        {
            if (OnPresence != null)
            {
                OnPresence(this);
            }
        }
        #endregion

        #region Properties
        public string Server { get; set; }
        public int Port { get; set; }
        public bool Authenticated
        {
            get
            {
                return _authenticated;
            }
        }
        #endregion

    }
}
