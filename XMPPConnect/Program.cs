using System;
using System.Threading;
using XMPPConnect.MainClasses;
using XMPPConnect.Client;

namespace XMPPConnect
{
    class Program
    {
        private static string _partnerMessage;
        static void Main(string[] args)
        {
            Presence presence = new Presence(ShowType.Show);

            int waitAuth = 5000;
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
            connection.Login();
            Thread.Sleep(waitAuth);

            connection.Send(presence);
            connection.Send(message);

            connection.MessageGrabber.Add(
                jidPartner, new MessageCallback(messageCallback));

            Thread.Sleep(waitMessage);

            Console.ReadLine();
        }

        private static void messageCallback(object sender, Message message)
        {
            if (message.Body != null)
            {
                _partnerMessage = message.From.Full + ":" + message.Body;
            }
        }
    }
}
