using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading.Tasks;
using XMPPConnect.Net;
using XMPPConnect.Helpers;
using XMPPConnect.Client;
using XMPPConnect.Data;
using XMPPConnect.Managers;
using XMPPConnect.Strategies;
using XMPPConnect.Loggers;
using NLog;

namespace XMPPConnect
{
    public class XmppClientConnection : IXmppConnection
    {
        private JabberID _jid;
        private ClientSocket _clientSocket;
        private StanzaManager _stanzaManager;
        private int _port;
        private bool _authenticated;
        private bool _connected;
        private string _response;
        private string _request;
        private string _errorMessage;
        private string _password;
        private string _server;
        private StreamWriter _clientServerLogger;
        private DataResolveContext _dataResolveContext;
        private Dictionary<StanzaType, IDataResolveStrategy> _dictDataResolveStrategies;

        public event ObjectHandler OnLogin;
        public event ObjectHandler OnBinded;
        public event ErrorHanlder OnError;
        public event PresenceHandler OnPresence;
        public event MessageHandler OnMessage;

        private MessageGrabber _messageGrabber;

        #region Properties

        public string Server
        {
            get { return _server; }
            private set { _server = value; }
        }
        public int Port
        {
            get { return _port; }
            private set { _port = value; }
        }
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

        public XmppClientConnection(int connectTimeout = 30000, int port = 5222)
        {
            _dictDataResolveStrategies = InitDictionaries();
            _clientSocket = InitSocket(connectTimeout);
            _stanzaManager = new StanzaManager();
            _authenticated = false;
            _connected = false;
            _port = port;
            _messageGrabber = new MessageGrabber(this);
        }

        public XmppClientConnection(JabberID jid, string password) : this()
        {
            _jid = jid;
            _password = password;
            _server = jid.Server;
        }

        public void Login()
        {
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

                _clientSocket.StartReceive();
            }
            catch (Exception ex)
            {
                InvokeOnError(new Error(ex));
            }
        }

        public async Task LoginAsync()
        {
            await Task.Run(() =>
            {
                Login();
            });
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

        private ClientSocket InitSocket(int connectTimeout)
        {
            var socket = new ClientSocket(connectTimeout);
            socket.OnSend += ClientSocketOnSend;
            socket.OnReceive += ClientSocketOnReceive;
            socket.OnConnect += ClientSocketOnConnect;
            socket.OnDisconnect += ClientSocketOnDisconnect;
            socket.OnError += ClientSocketOnError;
            return socket;
        }

        private Dictionary<StanzaType, IDataResolveStrategy> InitDictionaries()
        {
            var dict = new Dictionary<StanzaType, IDataResolveStrategy>();

            dict.Add(StanzaType.Message, new MessageResolver());
            dict.Add(StanzaType.Presence, new PresenceResolver());
            dict.Add(StanzaType.Error, new ErrorResolver());
            dict.Add(StanzaType.None, new NoneResolver());

            return dict;
        }

        #region Client Socket event handlers

        private void ClientSocketOnSend(object sender, byte[] data, int length)
        {
            _request = Encoding.UTF8.GetString(data, 0, length);

            ResolveData(_request);
            //NLogger.Log.Info(this.GetType() + "//" + "Send:" + _request);
        }

        private void ClientSocketOnReceive(object sender, byte[] data, int length)
        {
            Response = Encoding.UTF8.GetString(data);

            ResolveData(Response);
            //NLogger.Log.Info(this.GetType() + "//" + "Response:" + Response);
        }

        private void ResolveData(string xmlData)
        {
            Stanza requestObject = _stanzaManager.GetStanzaObject(xmlData);

            _dataResolveContext = new DataResolveContext(
                _dictDataResolveStrategies[requestObject.Type]);

            _dataResolveContext.Execute(this, requestObject);
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
