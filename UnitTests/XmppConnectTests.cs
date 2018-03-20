﻿using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XMPPConnect;
using NUnit.VisualStudio.TestAdapter;
using XMPPConnect.Net;
using XMPPConnect.Client;
using System.Threading;
using XMPPConnect.Data;
using XMPPConnect.Managers;

namespace XMPPConnect.Tests
{
    [TestFixture]
    public class XmppConnectTests
    {
        [Test]
        public void TestClientSocketConnect()
        {
            string address = "jabber.ru";
            int port = 5222;

            ClientSocket client = new ClientSocket();
            IAsyncResult result = client.BeginConnect(address, port);
            result.AsyncWaitHandle.WaitOne();

            Assert.IsTrue(client.Connected);
        }

        [Test]
        public void TestClientSocketDisconnect()
        {
            int connectTimeout = 2000;
            string address = "jabber.ru";
            int port = 5222;

            ClientSocket client = new ClientSocket();
            client.BeginConnect(address, port);
            Thread.Sleep(connectTimeout);

            client.Disconnect();
            Assert.IsFalse(client.Connected);
        }

        private string _partnerMessage;
        [Test]
        public void TestMessageGrabber()
        {
            Presence presence = new Presence(ShowType.Show);

            int waitMessage = 20000;
            string jid = "andrewprok@jabber.ru";
            string password = "newbie52";
            string jidPartnerString = "katepleh@jabber.ru";
            string msg = "hello";
            string expected = "katepleh@jabber.ru:hello";

            JabberID jidPartner = new JabberID(jidPartnerString);
            Message message = new Message(jid, jidPartner.Full, msg);

            XmppClientConnection connection = new XmppClientConnection(
                new JabberID(jid), password);
            connection.MessageGrabber.Add(
                jidPartner, new MessageCallback(messageCallback));

            connection.Login();

            connection.Send(presence);
            connection.Send(message);

            Thread.Sleep(waitMessage);

            Assert.AreEqual(expected, _partnerMessage);
        }

        private void messageCallback(object sender, Message message)
        {
            if (message.Body != null)
            {
                _partnerMessage = message.From.Full + ":" + message.Body;
            }
        }

        [Test]
        public void TestConnectionInit()
        {
            XmppClientConnection connection = new XmppClientConnection(
                new XMPPConnect.JabberID("andrewprok@jabber.ru"), "newbie52");
            Assert.AreEqual(connection.Server, "jabber.ru");
            Assert.AreEqual(connection.Port, 5222);
        }

        private static bool authenticated = false;
        [Test]
        public void TestConnectAndAuth()
        {
            string jidString = "andrewprok@jabber.ru";
            string password = "newbie52";


            XmppClientConnection connection = new XmppClientConnection(
                new JabberID(jidString), password);
            connection.OnLogin += OnLogin;
            Thread loginThread = new Thread(connection.Login);
            loginThread.Start();
            while (authenticated != true)
            {

            }

            Assert.IsTrue(connection.Connected);
            Assert.IsTrue(connection.Authenticated);
        }

        private static void OnLogin(object sender)
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

        // Что делает тест?
        [Test]
        public void TestSendPresenceAndMessage()
        {
            Presence presence = new Presence(ShowType.Show);

            JabberID from = new JabberID("andrewprok@jabber.ru");
            JabberID to = new JabberID("katepleh@jabber.ru");
            string msg = "Hello";
            Message message = new Message(from, to, msg);

            string jidString = "andrewprok@jabber.ru";
            string password = "newbie52";

            XmppClientConnection connection = new XmppClientConnection(
                new JabberID(jidString), password);
            connection.Login();

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

            JabberID from = new JabberID("andrewprok@jabber.ru");
            JabberID to = new JabberID("katepleh@jabber.ru");
            string msg = "Hello";
            Message message = new Message(from, to, msg);

            string jidString = "andrewprok@jabber.ru";
            string password = "newbie52";

            XmppClientConnection connection = new XmppClientConnection(
                new JabberID(jidString), password);
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
