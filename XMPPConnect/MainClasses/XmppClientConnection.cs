using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XMPPConnect.Interfaces;
using XMPPConnect.Managers;
using System.Net.Sockets;
using XMPPConnect.Net;
using XMPPConnect.Helpers;
using XMPPConnect.Client;
using System.Threading;

namespace XMPPConnect.MainClasses
{
    public delegate string SendDelegate(int callDuration, out int threadId);
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
        //private int _waitTime;
        private StreamWriter _clientServerLogger;

        public event ObjectHandler OnLogin;
        public event ObjectHandler OnBinded;
        public event ErrorHanlder OnError;
        public event ObjectHandler OnPresence;
        public event MessageHandler OnMessage;

        public MessageGrabber _messageGrabber;

        public XmppClientConnection()
        {
            InitSocket();
            _stanzaManager = new StanzaManager();
            _authenticated = false;
            _connected = false;
            //_waitTime = 400;
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
                asyncResult = _clientSocket.BeginConnect(Server, Port);
                asyncResult.AsyncWaitHandle.WaitOne();

                //Thread.Sleep(_waitTime);

                string handshake = _stanzaManager.GetXML(StanzaType.Header);
                string reqAuth = _stanzaManager.GetXML(StanzaType.Digest_auth);
                asyncResult = _clientSocket.BeginSend(handshake);
                asyncResult.AsyncWaitHandle.WaitOne();
                //Thread.Sleep(_waitTime);

                asyncResult = _clientSocket.BeginSend(reqAuth);
                asyncResult.AsyncWaitHandle.WaitOne();
                //Thread.Sleep(_waitTime);

                string base64Request = _stanzaManager.GetXML(StanzaType.Base_request, CryptographyHelper.DigestMD5AuthAlgo(_response, _jid, _password));
                asyncResult = _clientSocket.BeginSend(base64Request);
                asyncResult.AsyncWaitHandle.WaitOne();
                //Thread.Sleep(_waitTime);

                string saslOn = _stanzaManager.GetXML(StanzaType.Sasl_on);
                asyncResult = _clientSocket.BeginSend(saslOn);
                asyncResult.AsyncWaitHandle.WaitOne();
                //Thread.Sleep(_waitTime);

                asyncResult = _clientSocket.BeginSend(handshake);
                asyncResult.AsyncWaitHandle.WaitOne();
                //Thread.Sleep(_waitTime);

                string bind = _stanzaManager.GetXML(StanzaType.Bind);
                asyncResult = _clientSocket.BeginSend(bind);
                asyncResult.AsyncWaitHandle.WaitOne();
                //Thread.Sleep(_waitTime);

                if (!Response.Contains("error"))
                {
                    _authenticated = true;
                    InvokeOnLogin();
                }

                // Test presence and message

                //string presence = _stanzaManager.GetXML(StanzaType.Presence);
                //_clientSocket.BeginSend(presence);

                //Message message = new Message(_jid.ToString(), "katepleh@jabber.ru","Hello");
                //_clientSocket.BeginSend(message.ToString());
            }
            catch (Exception ex)
            {
                InvokeOnError(ex);
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
                if (msg is Message)
                {
                    InvokeOnMessage((Message)msg);
                }
                else if (msg is Presence)
                {
                    InvokeOnPresence();
                }

                _clientSocket.BeginSend(msg.ToString());
                Console.WriteLine("BeginSend:" + msg.ToString());
            }
            else
            {
                InvokeOnError(new NullReferenceException("Message instance is null."));
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
            _clientServerLogger.WriteLine("BeginSend:" + _request);
            Console.WriteLine("BeginSend:" + _request);
        }

        private void ClientSocketOnReceive(object sender, byte[] data, int length)
        {
            Response = Encoding.UTF8.GetString(data);
            _clientServerLogger.WriteLine("Response:" + Response);
            Console.WriteLine("Response:" + _response);
            if (Response.Contains("error") || Response.Contains("failure"))
            {
                InvokeOnError(new Exception(Response));
            }
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

        private void ClientSocketOnError(object sender, Exception ex)
        {
            _errorMessage = ex.Message;
            InvokeOnError(new Exception(_errorMessage));
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

    }
}
