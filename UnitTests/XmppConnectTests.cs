using NUnit.Framework;
using System;
using XMPPConnect.Net;
using XMPPConnect.Client;
using System.Threading;
using System.Configuration;
using System.Threading.Tasks;
using NLog;
using XMPPConnect.Loggers;

namespace XMPPConnect.Tests
{
    [TestFixture]
    public class XmppConnectTests
    {
        private bool authenticated = false;
        private string partnerMessage = null;

        private string myJid = ConfigurationManager.AppSettings["TestMyJabberId"];
        private string password = ConfigurationManager.AppSettings["TestMyPassword"];
        private string address = ConfigurationManager.AppSettings["TestServerAddress"];
        private int port = int.Parse(ConfigurationManager.AppSettings["TestServerPort"]);
        private string partnerJid = ConfigurationManager.AppSettings["TestPartnerId"];

        [Test]
        public void TestClientSocketConnect()
        {
            ClientSocket client = new ClientSocket();
            IAsyncResult result = client.BeginConnect(address, port);
            result.AsyncWaitHandle.WaitOne();

            Assert.IsTrue(client.Connected);
        }

        [Test]
        public void TestClientSocketDisconnect()
        { 
            ClientSocket client = new ClientSocket();
            client.BeginConnect(address, port);

            client.Disconnect();
            Assert.IsFalse(client.Connected);
        }

        [Test]
        public void TestMessageGrabber()
        {
            Presence presence = new Presence(ShowType.Show);

            int waitMessage = 20000;
            string msg = "hello";
            string expected = partnerJid + ":hello";

            JabberID jidPartner = new JabberID(partnerJid);
            Message message = new Message(myJid, jidPartner.Full, msg);

            XmppClientConnection connection = new XmppClientConnection(
                new JabberID(myJid), password);
            connection.MessageGrabber.Add(
                jidPartner, new MessageCallback(messageCallback));

            connection.Login();

            connection.Send(presence);
            connection.Send(message);

            while (partnerMessage == null)
            {
                Thread.Sleep(2000);
            }

            Assert.AreEqual(expected, partnerMessage);
        }

        private void messageCallback(object sender, Message message)
        {
            if (message.Body != null)
            {
                partnerMessage = message.From.Full + ":" + message.Body;
            }
        }

        [Test]
        public void TestConnectionInit()
        {
            XmppClientConnection connection = new XmppClientConnection(
                new JabberID(myJid), password);
            Assert.AreEqual(connection.Server, address);
            Assert.AreEqual(connection.Port, port);
        }

        [Test]
        public void TestConnectAndAuth()
        {

            XmppClientConnection connection = new XmppClientConnection(
                new JabberID(myJid ), password);
            connection.OnLogin += OnLogin;
            Thread loginThread = new Thread(connection.Login);
            loginThread.Start();
            while (authenticated != true)
            {

            }

            Assert.IsTrue(connection.Connected);
            Assert.IsTrue(connection.Authenticated);
        }

        private void OnLogin(object sender)
        {
            authenticated = true;
        }

        //[Test]
        //public void TestInitMessageWay_1()
        //{
        //    JabberID from = new JabberID("andrewprok@jabber.ru");
        //    JabberID to = new JabberID("katepleh@jabber.ru");
        //    string msg = "Hello";

        //    Message message = new Message(from, to, msg);

        //    Assert.AreEqual(msg, message.Body);
        //    Assert.AreEqual(from.Full, message.From.Full);
        //    Assert.AreEqual(to.Full, message.To.Full);
        //}

        //[Test]
        //public void TestInitMessageWay_2()
        //{
        //    string endPointResponse = "<message type='chat' to='andrewprok@jabber.ru' id='id' from='katepleh@jabber.ru'><body>Oh, hello</body></message>";

        //    Message message = new Message(endPointResponse);

        //    Assert.AreEqual("Oh, hello", message.Body);
        //    Assert.AreEqual("katepleh@jabber.ru", message.From.Full);
        //    Assert.AreEqual("andrewprok@jabber.ru", message.To.Full);
        //}

        //[Test]
        //public void TestInitMessageWay_3()
        //{
        //    JabberID from = new JabberID("andrewprok@jabber.ru");
        //    JabberID to = new JabberID("katepleh@jabber.ru");
        //    string msg = "Hello";

        //    StanzaManager stanzaManager = new StanzaManager();
        //    Message message = new Message(stanzaManager.GetXML(StanzaType.Message, msg, to.Full, from.Full));

        //    Assert.AreEqual(msg, message.Body);
        //    Assert.AreEqual(from.Full, message.From.Full);
        //    Assert.AreEqual(to.Full, message.To.Full);
        //}

        [Test]
        public void TestInitPresence()
        {
            Presence presence = new Presence(ShowType.Show);

            Assert.AreEqual("<presence><show></show></presence>", presence.ToString());
        }

        [Test]
        public void TestSendPresenceAndMessage()
        {
            //Logger logger = LogManager.GetLogger("foo");
            ILogger log = NLog.LogManager.GetLogger("foo");
            log.Info("Program started");
            Presence presence = new Presence(ShowType.Show);

            JabberID from = new JabberID(myJid);
            JabberID to = new JabberID(partnerJid);
            string msg = "Hello";
            Message message = new Message(from, to, msg);

            XmppClientConnection connection = new XmppClientConnection(
                new JabberID(myJid), password);
            connection.Login();

            if (!connection.Connected || !connection.Authenticated)
            {
                throw new Exception("Not authenticate");
            }

            Assert.DoesNotThrow(() => { connection.Send(presence); });
            Assert.DoesNotThrow(() => { connection.Send(message); });

            //NLogger.ShutdownLogger();
        }

        [Test]
        public async Task TestSendPresenceAndMessageAsync()
        {
            Presence presence = new Presence(ShowType.Show);

            JabberID from = new JabberID(myJid);
            JabberID to = new JabberID(partnerJid);
            string msg = "Hello";
            Message message = new Message(from, to, msg);

            XmppClientConnection connection = new XmppClientConnection(
                new JabberID(myJid), password);
            await connection.LoginAsync();

            if (!connection.Connected || !connection.Authenticated)
            {
                throw new Exception("Not authenticate");
            }

            Assert.DoesNotThrow(() => { connection.Send(presence); });
            Assert.DoesNotThrow(() => { connection.Send(message); });
        }

        [Test]
        public void TestSendManyMessages()
        {
            Presence presence = new Presence(ShowType.Show);

            JabberID from = new JabberID(myJid);
            JabberID to = new JabberID(partnerJid);
            string msg = "Hello";
            Message message = new Message(from, to, msg);

            XmppClientConnection connection = new XmppClientConnection(
                new JabberID(myJid), password);
            connection.Login();
            connection.Send(presence);

            for (int i = 0; i < 500; i++)
            {
                Assert.DoesNotThrow(() => { connection.Send(new Message(from, to, i.ToString())); });
                Thread.Sleep(100);
            }            
        }
    }
}
