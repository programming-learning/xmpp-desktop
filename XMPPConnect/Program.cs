using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using Microsoft.Language.Xml;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Diagnostics;
using XMPPConnect.Net;
using XMPPConnect.MainClasses;

namespace XMPPConnect
{
    class Program
    {
        static void Main(string[] args)
        {
            int connectTimeout = 10000;
            string jidString = "andrewprok@jabber.ru";
            string password = "newbie52";

            XmppClientConnection connection = new XmppClientConnection(
                new JabberID(jidString), password);
            connection.Login();
            Thread.Sleep(connectTimeout);
            while (true)
            {
                Thread.Sleep(200);
                Console.WriteLine(connection.Authenticated);
            }
            Console.ReadLine();
        }
    }
}
