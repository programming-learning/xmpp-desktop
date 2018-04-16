using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Collections;
using System.Net;
using System.Runtime.InteropServices;
using XMPPConnect.Client;
using XMPPConnect.Loggers;

namespace XMPPConnect.Net
{
    public class ClientSocket : ISocket
    {
        private TcpClient _tcpClient;
        private const int BUFFER_SIZE = 1024;
        private Timer _connectTimeoutTimer;
        private int _connectTimeout;
        private bool _connectTimedOut;
        private Stream _networkStream; // shared for many purposes
        private byte[] _readBuffer;
        private Queue _sendQueue;
        private int _port;
        private string _address;
        private IAsyncResult _asyncResult;

        public event ObjectHandler OnConnect;
        public event ObjectHandler OnDisconnect;
        public event OnSocketDataHandler OnSend;
        public event OnSocketDataHandler OnReceive;
        public event ErrorHanlder OnError;

        #region Properties
        // свойства обычно лежат вверху
        public string Address { get { return _address; } set { _address = value; } }
        public bool Connected
        {
            get
            {
                if (this._tcpClient == null)
                {
                    return false;
                }

                return this._tcpClient.Connected;
            }
        }
        public int ConnectTimeout { get { return _connectTimeout; } set { _connectTimeout = value; } }
        public int Port { get { return _port; } set { _port = value; } }
        #endregion

        public ClientSocket(int connectTimeout = 30000)
        {
            _connectTimeout = connectTimeout;
            _sendQueue = new Queue();
        }

        public IAsyncResult BeginConnect(string address, int port)
        {
            IAsyncResult result = null;
            _readBuffer = new byte[BUFFER_SIZE];
            try
            {
                IPAddress serverAddr = Dns.GetHostEntry(address).AddressList[0];
                _tcpClient = new TcpClient(serverAddr.AddressFamily);

                _connectTimedOut = false;
                TimerCallback callback = ConnectTimeoutTimerDelegate;
                _connectTimeoutTimer = new Timer(callback, null, this.ConnectTimeout, this.ConnectTimeout);

                result = _tcpClient.BeginConnect(serverAddr, port, EndConnect, null);
                //result.AsyncWaitHandle.WaitOne();

                BeginReceive(null);
            }
            catch (Exception ex)
            {
                InvokeOnError(new Error(ex));
            }

            return result;
        }

        public void Connect(string address, int port)
        {
            _readBuffer = new byte[BUFFER_SIZE];
            try
            {
                IPAddress serverAddr = Dns.GetHostEntry(address).AddressList[0];
                _tcpClient = new TcpClient(serverAddr.AddressFamily);

                _connectTimedOut = false;
                TimerCallback callback = ConnectTimeoutTimerDelegate;
                _connectTimeoutTimer = new Timer(callback, null, this.ConnectTimeout, this.ConnectTimeout);

                _tcpClient.Connect(serverAddr, port);
                _connectTimeoutTimer.Dispose();
                _networkStream = _tcpClient.GetStream();
                InvokeOnConnect();

            }
            catch (Exception ex)
            {
                InvokeOnError(new Error(ex));
            }
        }

        public void Send(string data)
        {
            Send(Encoding.UTF8.GetBytes(data));
        }

        public void Send(byte[] data)
        {
            lock (this)
            {
                _networkStream.Write(data, 0, data.Length);
            }

            InvokeOnSend(data, data.Length);
            Receive();
        }

        public void Receive()
        {
            _networkStream.Read(_readBuffer, 0, BUFFER_SIZE);          
            InvokeOnReceive(_readBuffer, _readBuffer.Length);
        }

        public void StartReceive()
        {
            BeginReceive(null);
        }

        public void Disconnect()
        {
            lock (this)
            {
                _sendQueue.Clear();
            }

            if (_tcpClient != null)
            {
                try
                {
                    _tcpClient.Close();
                }
                catch (Exception ex)
                {
                    InvokeOnError(new Error(ex));
                }

                InvokeOnDisconnect();
            }
        }

        private void ConnectTimeoutTimerDelegate(object stateinfo)
        {
            _connectTimeoutTimer.Dispose();
            _connectTimedOut = true;
            _tcpClient.Close();
            InvokeOnError(new Error(new Exception("Сonnect timed out.")));
        }

        private void EndConnect(IAsyncResult ar)
        {
            if(!_connectTimedOut)
            {
                try
                {
                    _connectTimeoutTimer.Dispose();
                    _tcpClient.EndConnect(ar);
                    // _networkStream - разделяемый ресурс
                    _networkStream = _tcpClient.GetStream();
                    ar.AsyncWaitHandle.Close();
                    InvokeOnConnect();
                }
                catch (Exception ex)
                {
                    InvokeOnError(new Error(ex));
                }
            }
        }

        private void BeginReceive(IAsyncResult ar)
        {
            if (ar != null)
            {
                try
                {
                    int length = _networkStream.EndRead(ar);
                    if (length > 0)
                    {
                        InvokeOnReceive(_readBuffer, length);

                        if (Connected)
                        {
                            lock (this)
                            {
                                _networkStream.BeginRead(_readBuffer, 0, BUFFER_SIZE, BeginReceive, null);
                            }                           
                        }
                    }
                    //else
                    //{
                    //    Disconnect();
                    //}
                }
                catch (Exception ex)
                {
                    InvokeOnError(new Error(ex));
                }
            }
            else
            {
                if (Connected)
                {
                    lock (this)
                    {
                        _networkStream.BeginRead(_readBuffer, 0, BUFFER_SIZE, BeginReceive, null);
                    }
                }
            }
        }

        public IAsyncResult BeginSend(byte[] data)
        {
            IAsyncResult result = null;
            ClientSocket socket;
            Monitor.Enter(socket = this);
            try
            {
                _sendQueue.Enqueue(data);
                while (_sendQueue.Count != 0)
                {
                    byte[] buffer = (byte[])_sendQueue.Dequeue();
                    lock (this)
                    {
                        result = _networkStream.BeginWrite(buffer, 0, buffer.Length, EndSend, null);
                    }
                    
                    InvokeOnSend(data, data.Length);
                }             
            }
            catch (Exception ex)
            {
                InvokeOnError(new Error(ex));
            }
            finally
            {
                Monitor.Exit(socket);
            }

            return result;
        }

        public IAsyncResult BeginSend(string data)
        {
            return BeginSend(Encoding.UTF8.GetBytes(data));
        }

        private void EndSend(IAsyncResult ar)
        {
            ClientSocket socket;
            Monitor.Enter(socket = this);
            try
            {
                lock (this)
                {
                    _networkStream.EndWrite(ar);
                }
                ar.AsyncWaitHandle.Close();
            }
            catch (Exception ex)
            {
                Disconnect();
            }
            finally
            {
                Monitor.Exit(socket);
            }
        }

        #region InvokeEvents
        private void InvokeOnError(Error er)
        {
            if (OnError != null)
            {
                NLogger.Log.Error(er.Source + "//" + "Exception:" + er.Message);
                OnError(this, er);
            }
        }

        private void InvokeOnDisconnect()
        {
            if (OnDisconnect != null)
            {
                NLogger.Log.Info(this.GetType() + "//" + "OnDisconnect");
                OnDisconnect(this);
            }
        }

        private void InvokeOnConnect()
        {
            if (OnConnect != null)
            {
                NLogger.Log.Info(this.GetType() + "//" + "OnConnect");
                OnConnect(this);
            }
        }

        private void InvokeOnSend(byte[] data, int length)
        {
            if (OnSend != null)
            {
                NLogger.Log.Info(this.GetType() + "//" + "Send:" + Encoding.UTF8.GetString(data));
                OnSend(this, data, length);
            }
        }

        private void InvokeOnReceive(byte[] data, int length)
        {
            if (OnReceive != null)
            {
                NLogger.Log.Info(this.GetType() + "//" + "Receive:" + Encoding.UTF8.GetString(data));
                OnReceive(this, data, length);
            }
        }
        #endregion
    }
}
