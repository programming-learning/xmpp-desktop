using System;
using agsXMPP;
using agsXMPP.protocol.client;
using agsXMPP.Collections;
using System.Threading;

namespace XMPP_client
{
    class Program
    {
        static private bool s_wait;
        static void Main(string[] args)
        {
            Console.WriteLine("Authentication...");
            Console.WriteLine();

            Console.Write("JID>> ");
            string login = Console.ReadLine();

            Console.Write("Password>> ");
            string password = Console.ReadLine();

            Jid jidClient = new Jid(login);

            XmppClientConnection xmppClientConnetion = new XmppClientConnection
            {
                ConnectServer = jidClient.Server,
                Server = jidClient.Server,
                Port = 5222,
            };

            XmppClientConnection c = new XmppClientConnection();

            try
            {
                xmppClientConnetion.Open(jidClient.User, password);
                xmppClientConnetion.OnLogin += new ObjectHandler(OnLogin);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Console.Write("Wait");
            int waitTimer = 0;
            s_wait = true;
            while (s_wait)
            {
                if (waitTimer == 20)
                {
                    s_wait = false;
                }

                waitTimer++;
                Thread.Sleep(100);
            }

            Console.WriteLine();

            if (xmppClientConnetion.Authenticated == true)
            {
                Console.WriteLine("Success auth");
            }
            else
            {
                Console.WriteLine("Failure auth");
            }

            Presence presence = new Presence(ShowType.chat, "Online");

            presence.Type = PresenceType.invisible;
            xmppClientConnetion.Send(presence);


            xmppClientConnetion.OnPresence += OnPresence;
            //xmppClientConnetion.

            Console.Write("Write your partner jid>> ");
            string jidPartnerStr = Console.ReadLine();
            Jid jidPartner = new Jid(jidPartnerStr);

            Console.WriteLine("Chat starts here ->");

            xmppClientConnetion.MessageGrabber.Add(
                jidPartner,
                new BareJidComparer(),
                new MessageCB(MessageCallback),
                null);

            string receivedMessage;
            bool exit = false;
            while (!exit)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                receivedMessage = Console.ReadLine();

                if (receivedMessage == "exit")
                {
                    exit = true;
                }
                else
                {
                    xmppClientConnetion.Send(new Message(new Jid(jidPartner), MessageType.chat, receivedMessage));
                }
            }

            xmppClientConnetion.Close();
        }

        static private void OnLogin(object sender)
        {
            s_wait = false;
        }

        static private void OnPresence(object sender, Presence pres) { }

        static void MessageCallback(object sender, Message message, object data)
        {
            if (message.Body != null)
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("[" + DateTime.Now.ToLongTimeString() + "]" + " <{0}> {1}", message.From.User, message.Body);
                Console.ForegroundColor = ConsoleColor.Yellow;
            }
        }
    }
}
