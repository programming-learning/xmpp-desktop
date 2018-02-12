using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using Microsoft.Language.Xml;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Diagnostics;

namespace Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            XmlDocument xmlDoc = new XmlDocument();
            Debug.Assert(HereJabber("andrewprok@jabber.ru", "newbie52") == true);
            Debug.Assert(HereJabber("andrewprok@jabber.ru", "newbe51") == false);
        }

        static StreamWriter logger = new StreamWriter("log.txt");
        static bool HereJabber(string jid, string password)
        {
            string server = jid.Split('@')[1];
            string username = jid.Split('@')[0];

            IPAddress address = Dns.GetHostEntry("jabber.ru").AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(address, 5222);
            Socket clientSocket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                clientSocket.Connect(endPoint);

                // Первое рукопожатие
                string message = "<?xml version='1.0' encoding='UTF-8'?><stream:stream to='jabber.ru' xmlns='jabber:client' xmlns:stream='http://etherx.jabber.org/streams' xml:l='ru' version='1.0'>";
                string response = SendMessageToServer(clientSocket, message, logger, 0);

                // Запрос на аутентификацию
                message = "<auth xmlns='urn:ietf:params:xml:ns:xmpp-sasl' mechanism='DIGEST-MD5'/>";
                response = SendMessageToServer(clientSocket, message, logger, 2000);

                XmlDocumentSyntax root = Parser.ParseText(response);

                string base64Info = string.Empty;
                foreach (IXmlElement node in root.Elements)
                {
                    Console.WriteLine(node.Name);
                    Console.WriteLine(node.Value);
                    if (node.Name == "challenge")
                    {
                        base64Info = node.Value;
                    }
                }

                byte[] impDataByte = new byte[1024];
                impDataByte = Convert.FromBase64String(base64Info);

                string impData = Encoding.Default.GetString(impDataByte);
                Regex reg = new Regex("nonce=\"[0-9]*\"");
                Match m = reg.Match(impData);
                string nonce = string.Empty;
                if (m.Success)
                {
                    nonce = m.Groups[0].Value;
                    nonce = nonce.Replace("nonce=\"", "").Replace("\"", "");
                }

                string cNonce = GetUniqCNonce(username, password);
                string clientHash = GenerateClientHash(username, password, nonce, cNonce);
                string value = GetAuthenticationString(username, password, nonce, cNonce, clientHash);

                string baseResponse = Convert.ToBase64String(Encoding.UTF8.GetBytes(value));

                //XmlTextWriter writer = new XmlTextWriter("XMLDocuments\\Base64Response.xml", null);
                //writer.Formatting = Formatting.Indented;
                //writer.WriteStartDocument();
                //writer.WriteStartElement("response");
                //writer.WriteAttributeString("xmlns", "urn:ietf:params:xml:ns:xmpp-sasl");
                //writer.WriteString(baseResponse);
                //writer.WriteEndElement();
                //writer.WriteEndDocument();
                //writer.Flush();
                //writer.Close();

                //XmlDocument baseDoc = new XmlDocument();
                //baseDoc.Load("XMLDocuments\\Base64Response.xml");

                // Ответ base64 строкой
                message = "<response xmlns=\"urn:ietf:params:xml:ns:xmpp-sasl\">" + baseResponse + "</response>";
                response = SendMessageToServer(clientSocket, message, logger, 0);

                if(response.Contains("not-authorized"))
                {
                    return false;
                }

                // SASL
                message = "<response xmlns=\"urn:ietf:params:xml:ns:xmpp-sasl\"/>";
                response = SendMessageToServer(clientSocket, message, logger, 0);

                // Повторное рукопожатие
                message = "<?xml version='1.0' encoding='UTF-8'?><stream:stream to='jabber.ru' xmlns='jabber:client' xmlns:stream='http://etherx.jabber.org/streams' xml:l='ru' version='1.0'>";
                response = SendMessageToServer(clientSocket, message, logger, 0);

                Regex idReg = new Regex("id='[0-9]*'");
                m = idReg.Match(response);
                string id = string.Empty;
                if (m.Success)
                {
                    id = m.Groups[0].Value;
                    id = id.Replace("id=\"", "").Replace("\"", "");
                }

                // Бинд
                message = "<iq type='set' id='bund_2'><bind xmlns='urn:ietf:params:xml:ns:xmpp-bind'><resource></resource></bind></iq>";
                response = SendMessageToServer(clientSocket, message, logger, 1000);

                logger.Close();
                logger.Dispose();

                if (!response.Contains("error"))
                {
                    return true;
                }

                return false;

                // Присутствие
                //message = "<presence><show></show></presence>";
                //response = SendMessageToServer(clientSocket, message, logger, 0);

                // Тестовое сообщение
                //message = "<message type=\"chat\" to=\"katepleh@jabber.ru\" id=\"id\" from=\"andrewprok@jabber.ru\"><body>Привет</body></message>";
                //response = SendMessageToServer(clientSocket, message, logger, 0);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        static string SendMessageToServer(Socket clientSocket, string message, StreamWriter logger, int timeout)
        {
            byte[] bytes = new byte[1024];
            logger.WriteLine("C:" + message + Environment.NewLine);
            byte[] msg = Encoding.UTF8.GetBytes(message);
            int bytesSent = clientSocket.Send(msg);
            Thread.Sleep(timeout);
            int bytesResp = clientSocket.Receive(bytes);
            string response = Encoding.UTF8.GetString(bytes, 0, bytesResp);
            logger.WriteLine("S:" + response + Environment.NewLine);

            Console.WriteLine(response);
            return response;
        }

        static string GenerateClientHash(string username, string password, string nonce, string cNonce)
        {
            const string nc = "00000001";
            const string qop = "auth";
            const string realm = "jabber.ru";
            const string digestUri = "xmpp/jabber.ru";

            byte[] h1 = H(Encoding.UTF8.GetBytes(string.Format($"{username}:{realm}:{password}")));
            byte[] h2 = Encoding.UTF8.GetBytes($":{nonce}:{cNonce}");

            byte[] A1 = Merge(h1, h2);
            byte[] A2 = Encoding.UTF8.GetBytes(string.Format($"AUTHENTICATE:{digestUri}"));

            byte[] responseValue = HEX(KD(HEX(H(A1)), Merge(Encoding.UTF8.GetBytes($"{nonce}:{nc}:{cNonce}:{qop}:"), HEX(H(A2)))
                ));

            byte[] h3 = Merge(h1, h2);
            return Encoding.UTF8.GetString(responseValue);
        }

        static byte[] H(byte[] s)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            return md5.ComputeHash(s);
        }

        static byte[] KD(byte[] k, byte[] s)
        {
            byte[] colon = Encoding.UTF8.GetBytes(":");
            byte[] ks = Merge(k, colon, s);
            return H(ks);
        }

        static byte[] Merge(byte[] f, byte[] s)
        {
            byte[] res = new byte[f.Length + s.Length];

            int i = 0;
            for (; i < f.Length; i++)
            {
                res[i] = f[i];
            }

            for (int j = 0; i < f.Length + s.Length; i++)
            {
                res[i] = s[j];
                j++;
            }

            return res;
        }
        static byte[] Merge(byte[] f, byte[] s, byte[] t)
        {
            byte[] res = new byte[f.Length + s.Length + t.Length];
            res = Merge(Merge(f, s), t);

            return res;
        }

        static byte[] HEX(byte[] n)
        {
            string test = HexEncoding.ToString(n).ToLower();
            byte[] b = Encoding.UTF8.GetBytes(HexEncoding.ToString(n).ToLower());
            return b;
        }

        static string GetMD5Hash(MD5 md5Hash, string input)
        {
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
            StringBuilder sBuilder = new StringBuilder();

            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            return sBuilder.ToString();
        }

        static string GetAuthenticationString(string username, string password, string nonce, string cNonce, string response)
        {
            StringBuilder res = new StringBuilder();
            res.Append("username=\"" + username + "\"," +
                       "realm=\"" + "jabber.ru" + "\"," +
                       "nonce=\"" + nonce + "\"," +
                       "cnonce=\"" + cNonce + "\"," +
                       "nc=\"" + "00000001" + "\"," +
                       "qop=\"" + "auth" + "\"," +
                       "digest-uri=\"" + "xmpp/jabber.ru" + "\"," +
                       "charset=\"" + "utf-8" + "\"," +
                       "response=\"" + response + "\"");
            return res.ToString();
        }

        static string GetUniqCNonce(string username, string password)
        {
            string result = string.Empty;

            result += username.GetHashCode();
            foreach (var c in username)
            {
                result += (char)((int)c);
            }

            result += password.GetHashCode();
            //foreach (var c in password)
            //{
            //    result += (int)c;
            //}

            return result;
        }
    }
}
