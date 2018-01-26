using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMPPConnect.Net
{
    public interface ISocket
    {
        void Connect(string address, int port);
        void Disconnect();
        void Send(byte[] data);
        void Send(string str);


        string Address { get; set; }
        bool Connected { get; }
        int ConnectTimeout { get; set; }
        int Port { get; set; }
        event ObjectHandler OnConnect;
        event ObjectHandler OnDisconnect;
        event ErrorHanlder OnError;
        event OnSocketDataHandler OnReceive;
        event OnSocketDataHandler OnSend;
    }
}
