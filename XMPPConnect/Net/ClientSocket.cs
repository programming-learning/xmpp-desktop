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

namespace XMPPConnect.Net
{
    public class ClientSocket : ISocket
    {
        private TcpClient _tcpClient;
        private const int BUFFER_SIZE = 1024;
        private Timer _connectTimeoutTimer;
        private int _connectTimeout;
        private bool _connectTimedOut;
        private Stream _networkStream;
        private bool _pendingSend;
        private byte[] _readBuffer;
        private Queue _sendQueue;
        private int _port;
        private string _address;

        public event ObjectHandler OnConnect;
        public event ObjectHandler OnDisconnect;
        public event OnSocketDataHandler OnSend;
        public event OnSocketDataHandler OnReceive;
        public event ErrorHanlder OnError;

        public ClientSocket()
        {
            _connectTimeout = 30000;
            _sendQueue = new Queue();
        }

        public void Connect(string address, int port)
        {
            _readBuffer = new byte[BUFFER_SIZE];
            try
            {
                IPAddress serverAddr = Dns.GetHostEntry(address).AddressList[0];
                IPEndPoint endPoint = new IPEndPoint(serverAddr, port);
                _tcpClient = new TcpClient(serverAddr.AddressFamily);

                _connectTimedOut = false;
                TimerCallback callback = new TimerCallback(ConnnectTimeoutTimerDelegate);
                _connectTimeoutTimer = new Timer(callback, null, this.ConnectTimeout, this.ConnectTimeout);
                _tcpClient.BeginConnect(serverAddr, port, new AsyncCallback(EndConnect), null);
            }
            catch (Exception ex)
            {
                InvokeOnError(ex);
            }
        }

        public void Disconnect()
        {
            lock (this)
            {
                _pendingSend = false;
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
                    InvokeOnError(ex);
                }

                InvokeOnDisconnect();
            }
        }

        private void ConnnectTimeoutTimerDelegate(object stateinfo)
        {
            _connectTimeoutTimer.Dispose();
            _connectTimedOut = true;
            _tcpClient.Close();
        }

        private void EndConnect(IAsyncResult ar)
        {
            if (_connectTimedOut)
            {
                InvokeOnError(new Exception("Connect timed out."));
            }
            else
            {
                try
                {
                    _connectTimeoutTimer.Dispose();
                    _tcpClient.EndConnect(ar);
                    // _networkStream - разделяемый ресурс
                    _networkStream = _tcpClient.GetStream();
                    InvokeOnConnect();
                    Receive();
                }
                catch (Exception ex)
                {
                    InvokeOnError(ex);
                }
            }
        }

        private void Receive()
        {
            _networkStream.BeginRead(_readBuffer, 0, BUFFER_SIZE, new AsyncCallback(EndReceive), null);
        }

        private void EndReceive(IAsyncResult ar)
        {
            try
            {
                int length = _networkStream.EndRead(ar);
                if (length > 0)
                {
                    InvokeOnReceive(_readBuffer, length);

                    if (Connected)
                    {
                        Receive();
                    }
                }
                //else
                //{
                //    Disconnect();
                //}
            }
            catch (Exception ex)
            {

            }
        }

        public void Send(byte[] data)
        {
            ClientSocket socket;
            Monitor.Enter(socket = this);
            try
            {
                InvokeOnSend(data, data.Length);
                if (_pendingSend)
                {
                    _sendQueue.Enqueue(data);
                }
                else
                {
                    _pendingSend = true;
                    try
                    {
                        _networkStream.BeginWrite(data, 0, data.Length, new AsyncCallback(EndSend), null);
                    }
                    catch (Exception ex)
                    {
                        this.Disconnect();
                    }
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                Monitor.Exit(socket);
            }
        }

        public void Send(string data)
        {
            Send(Encoding.UTF8.GetBytes(data));
        }

        private void EndSend(IAsyncResult ar)
        {
            ClientSocket socket;
            Monitor.Enter(socket = this);
            try
            {
                _networkStream.EndWrite(ar);
                if (_sendQueue.Count > 0)
                {
                    byte[] buffer = (byte[])_sendQueue.Dequeue();
                    _networkStream.BeginWrite(buffer, 0, buffer.Length, new AsyncCallback(EndSend), null);
                }
                else
                {
                    _pendingSend = false;
                }
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
        private void InvokeOnError(Exception ex)
        {
            if (OnError != null)
            {
                OnError(this, ex);
            }
        }

        private void InvokeOnDisconnect()
        {
            if (OnDisconnect != null)
            {
                OnDisconnect(this);
            }
        }

        private void InvokeOnConnect()
        {
            if (OnConnect != null)
            {
                OnConnect(this);
            }
        }

        private void InvokeOnSend(byte[] data, int length)
        {
            if (OnSend != null)
            {
                OnSend(this, data, length);
            }
        }

        private void InvokeOnReceive(byte[] data, int length)
        {
            if (OnReceive != null)
            {
                OnReceive(this, data, length);
            }
        }
        #endregion

        #region Properties
        public string Address { get { return _address; } set { _address = value; } }
        public bool Connected
        {
            get
            {
                if(this._tcpClient == null)
                {
                    return false;
                }

                return this._tcpClient.Connected;
            }
        }
        public int ConnectTimeout { get { return _connectTimeout; } set { _connectTimeout = value; } }
        public int Port { get { return _port; } set { _port = value; } }
        #endregion
    }
}
