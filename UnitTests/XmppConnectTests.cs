using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XMPPConnect;
using NUnit.VisualStudio.TestAdapter;
using XMPPConnect.MainClasses;
using XMPPConnect.Net;
using XMPPConnect.Client;
using System.Threading;

namespace XMPPConnect.Tests
{
    [TestFixture]
    public class XmppConnectTests
    {
        [Test]
        public void TestClientSocketConnect()
        {
            int connectTimeout = 2000;
            string address = "jabber.ru";
            int port = 5222;

            ClientSocket client = new ClientSocket();
            client.Connect(address, port);
            Thread.Sleep(connectTimeout);

            Assert.IsTrue(client.Connected);
        }

        [Test]
        public void TestConnectionInit()
        {
            XmppClientConnection connection = new XmppClientConnection(
                new XMPPConnect.JabberID("andrewprok@jabber.ru"), "newbie52");
            Assert.AreEqual(connection.Server, "jabber.ru");
            Assert.AreEqual(connection.Port, 5222);
        }

        [Test]
        public void TestConnectAndAuth()
        {
            int wait = 5000;
            string jidString = "andrewprok@jabber.ru";
            string password = "newbie52";

            XmppClientConnection connection = new XmppClientConnection(
                new JabberID(jidString), password);
            connection.Login();
            Thread.Sleep(wait);

            Assert.IsTrue(connection.Connected);
            Assert.IsTrue(connection.Authenticated);
        }

        [Test]
        public void TestInitMessageWay_1()
        {
            JabberID from = new JabberID("andrewprok@jabber.ru");
            JabberID to = new JabberID("katepleh@jabber.ru");
            string msg = "Hello";

            Message message = new Message(from, to, msg);

            Assert.AreEqual(msg, message.Body);
            Assert.AreEqual(from.Full, message.From.Full);
            Assert.AreEqual(to.Full, message.To.Full);
        }

        [Test]
        public void TestInitMessageWay_2()
        {
            string endPointResponse = "<message type='chat' to='andrewprok@jabber.ru' id='id' from='katepleh@jabber.ru'><body>Oh, hello</body></message>";

            Message message = new Message(endPointResponse);

            Assert.AreEqual("Oh, hello", message.Body);
            Assert.AreEqual("katepleh@jabber.ru", message.From.Full);
            Assert.AreEqual("andrewprok@jabber.ru", message.To.Full);
        }

        [Test]
        public void TestInitMessageWay_3()
        {
            JabberID from = new JabberID("andrewprok@jabber.ru");
            JabberID to = new JabberID("katepleh@jabber.ru");
            string msg = "Hello";

            StanzaManager stanzaManager = new StanzaManager();
            Message message = new Message(stanzaManager.GetXML(StanzaType.Message, msg, to.Full, from.Full));

            Assert.AreEqual(msg, message.Body);
            Assert.AreEqual(from.Full, message.From.Full);
            Assert.AreEqual(to.Full, message.To.Full);
        }

        [Test]
        public void TestInitPresence()
        {
            Presence presence = new Presence(ShowType.Show);

            Assert.AreEqual("<presence><show></show></presence>", presence.ToString());
        }

        [Test]
        public void TestSendPresenceAndMessage()
        {
            Presence presence = new Presence(ShowType.Show);

            JabberID from = new JabberID("andrewprok@jabber.ru");
            JabberID to = new JabberID("katepleh@jabber.ru");
            string msg = "Hello";
            Message message = new Message(from, to, msg);

            int wait = 5000;
            string jidString = "andrewprok@jabber.ru";
            string password = "newbie52";

            XmppClientConnection connection = new XmppClientConnection(
                new JabberID(jidString), password);
            connection.Login();
            Thread.Sleep(wait);

            connection.Send(presence);
            connection.Send(message);
            Thread.Sleep(500);
        }
    }
}
