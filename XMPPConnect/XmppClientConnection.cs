using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using XMPPConnect.Net;
using XMPPConnect.Helpers;
using XMPPConnect.Client;
using XMPPConnect.Data;
using XMPPConnect.Managers;
using XMPPConnect.Strategies;
using XMPPConnect.Loggers;
using NLog;
using Logger = NLog.Logger;

namespace XMPPConnect
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
        private StreamWriter _clientServerLogger;
        private DataResolveContext _dataResolveContext;
        private Dictionary<StanzaType, IDataResolveStrategy> _dictDataResolveStrategies;

        public event ObjectHandler OnLogin;
        public event ObjectHandler OnBinded;
        public event ErrorHanlder OnError;
        public event PresenceHandler OnPresence;
        public event MessageHandler OnMessage;

        public MessageGrabber _messageGrabber;

        #region Properties
        public string Server { get; set; }
        public int Port { get; set; }
        public bool Authenticated { get { return _authenticated; } }
        public bool Connected { get { return _clientSocket.Connected; } }
        public MessageGrabber MessageGrabber { get { return _messageGrabber; } }
        private string Response
        {
            get
            {
                if (_response == null)
                {
                    return string.Empty;
                }

                return _response;
            }

            set { _response = value; }
        }
        #endregion

        public XmppClientConnection()
        {
            NLogger.InitLogger();
            _dictDataResolveStrategies = new Dictionary<StanzaType, IDataResolveStrategy>();
            InitSocket();
            InitDictionaries();
            _stanzaManager = new StanzaManager();
            _authenticated = false;
            _connected = false;
            _clientServerLogger = new StreamWriter("logClientServer.txt");
            Port = 5222;
            _messageGrabber = new MessageGrabber(this);
        }

        public XmppClientConnection(JabberID jid, string password) : this()
        {
            _jid = jid;
            _password = password;
            Server = jid.Server;
        }

        public void Login()
        {
            IAsyncResult asyncResult = null;
            if (string.IsNullOrEmpty(Server))
            {
                throw new ArgumentException("Server was not assign.");
            }

            try
            {
                _clientSocket.Connect(Server, Port);

                string handshake = _stanzaManager.GetXML(StanzaType.Header);
                string reqAuth = _stanzaManager.GetXML(StanzaType.Digest_auth);
                _clientSocket.Send(handshake);

                _clientSocket.Send(reqAuth);

                WaitChallenge();

                string base64Request = _stanzaManager.GetXML(StanzaType.Base_request, CryptographyHelper.DigestMD5AuthAlgo(_response, _jid, _password));
                _clientSocket.Send(base64Request);

                string saslOn = _stanzaManager.GetXML(StanzaType.Sasl_on);
                _clientSocket.Send(saslOn);

                _clientSocket.Send(handshake);

                string bind = _stanzaManager.GetXML(StanzaType.Bind);
                _clientSocket.Send(bind);

                if (!Response.Contains("error"))
                {
                    _authenticated = true;
                    InvokeOnLogin();
                }

                _clientServerLogger.Close();

                _clientSocket.StartReceive();
                // Test presence and message

                //string presence = _stanzaManager.GetXML(StanzaType.Presence);
                //_clientSocket.BeginSend(presence);

                //Message message = new Message(_jid.ToString(), "katepleh@jabber.ru","Hello");
                //_clientSocket.BeginSend(message.ToString());
            }
            catch (Exception ex)
            {
                InvokeOnError(new Error(ex));
            }
        }

        private void WaitChallenge()
        {
            while (!Response.Contains("challenge"))
            {
                _clientSocket.Receive();
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
            if (msg != null)
            {
                _clientSocket.BeginSend(msg.ToString());
            }
            else
            {
                InvokeOnError(new Error(new NullReferenceException("Message instance is null.")));
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

        private void InitDictionaries()
        {
            _dictDataResolveStrategies.Add(StanzaType.Message, new MessageResolver());
            _dictDataResolveStrategies.Add(StanzaType.Presence, new PresenceResolver());
            _dictDataResolveStrategies.Add(StanzaType.Error, new ErrorResolver());
            _dictDataResolveStrategies.Add(StanzaType.None, new NoneResolver());
        }

        #region Client Socket event handlers

        private void ClientSocketOnSend(object sender, byte[] data, int length)
        {
            _request = Encoding.UTF8.GetString(data, 0, length);

            Stanza requestObject = _stanzaManager.GetStanzaObject(_request);

            _dataResolveContext = new DataResolveContext(
                _dictDataResolveStrategies[requestObject.Type]);

            _dataResolveContext.Execute(this, requestObject);

            //NLogger.Log.Info(this.GetType() + "//" + "Send:" + _request);
        }

        private void ClientSocketOnReceive(object sender, byte[] data, int length)
        {
            Response = Encoding.UTF8.GetString(data);

            //NLogger.Log.Info(this.GetType() + "//" + "Response:" + Response);
        }

        private void ClientSocketOnConnect(object sender)
        {
            //_connected = true;
        }

        private void ClientSocketOnDisconnect(object sender)
        {
            //_connected = false;
            
            _authenticated = false;
        }

        private void ClientSocketOnError(object sender, Error er)
        {
            _errorMessage = er.Message;
           
            InvokeOnError(er);
        }
        #endregion

        #region InvokeEvents
        internal void InvokeOnError(Error er)
        {
            if (OnError != null)
            {
                NLogger.Log.Error(er.Source + "//" + "Exception:" + er.Message);
                OnError(this, er);
            }
        }

        internal void InvokeOnMessage(Message msg)
        {
            if (OnMessage != null)
            {
                OnMessage(this, msg);
            }
        }

        internal void InvokeOnBinded()
        {
            if (OnBinded != null)
            {
                NLogger.Log.Info(this.GetType() + "//" + "OnBinded");
                OnBinded(this);
            }
        }

        internal void InvokeOnLogin()
        {
            if (OnLogin != null)
            {
                NLogger.Log.Info(this.GetType() + "//" + "OnLogin");
                OnLogin(this);
            }
        }

        internal void InvokeOnPresence(Presence presence)
        {
            if (OnPresence != null)
            {
                NLogger.Log.Info(this.GetType() + "//" + "OnPresence");
                OnPresence(this, presence);
            }
        }
        #endregion
    }
}
