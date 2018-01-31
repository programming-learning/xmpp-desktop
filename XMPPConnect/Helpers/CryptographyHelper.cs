using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Language.Xml;
using System.Text.RegularExpressions;
using System.Security.Cryptography;

namespace XMPPConnect.Helpers
{
    public static class CryptographyHelper
    {
        private const string _nc = "00000001";
        private const string _qop = "auth";
        private const string _realm = "jabber.ru";
        private const string _digestUri = "xmpp/jabber.ru";

        public static string DigestMD5AuthAlgo(string xml, JabberID jid, string password)
        {
            XmlDocumentSyntax root = Parser.ParseText(xml);

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

            byte[] uniqDataB = new byte[1024];
            uniqDataB = Convert.FromBase64String(base64Info);

            string uniqData = Encoding.Default.GetString(uniqDataB);
            Regex reg = new Regex("nonce=\"[0-9]*\"");
            Match m = reg.Match(uniqData);
            string nonce = string.Empty;
            if (m.Success)
            {
                nonce = m.Groups[0].Value;
                nonce = nonce.Replace("nonce=\"", "").Replace("\"", "");
            }

            string cNonce = GetUniqCNonce();
            string clientHash = GenerateClientHash(jid.Username, password, nonce, cNonce);
            string value = GetAuthenticationString(jid.Username, password, nonce, cNonce, clientHash);

            string baseResponse = Convert.ToBase64String(Encoding.UTF8.GetBytes(value));

            return baseResponse;
        }

        private static string GenerateClientHash(string username, string password, string nonce, string cNonce)
        {
            byte[] h1 = H(Encoding.UTF8.GetBytes(string.Format($"{username}:{_realm}:{password}")));
            byte[] h2 = Encoding.UTF8.GetBytes($":{nonce}:{cNonce}");

            byte[] A1 = Merge(h1, h2);
            byte[] A2 = Encoding.UTF8.GetBytes(string.Format($"AUTHENTICATE:{_digestUri}"));

            byte[] responseValue = HEX(KD(HEX(H(A1)), Merge(Encoding.UTF8.GetBytes($"{nonce}:{_nc}:{cNonce}:{_qop}:"), HEX(H(A2)))
                ));

            byte[] h3 = Merge(h1, h2);
            return Encoding.UTF8.GetString(responseValue);
        }

        private static string GetUniqCNonce()
        {
            string result = string.Empty;

            //result += username.GetHashCode();
            //foreach (var c in username)
            //{
            //    result += (char)((int)c);
            //}

            //result += password.GetHashCode();

            RandomNumberGenerator generator = RandomNumberGenerator.Create();
            byte[] data = new byte[1024];
            generator.GetBytes(data);

            return HexEncoding.ToString(data).ToLower();
        }

        private static string GetAuthenticationString(string username, string password, string nonce, string cNonce, string response)
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

        private static byte[] H(byte[] s)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            return md5.ComputeHash(s);
        }

        private static byte[] KD(byte[] k, byte[] s)
        {
            byte[] colon = Encoding.UTF8.GetBytes(":");
            byte[] ks = Merge(k, colon, s);
            return H(ks);
        }

        private static byte[] Merge(byte[] f, byte[] s)
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

        private static byte[] Merge(byte[] f, byte[] s, byte[] t)
        {
            byte[] res = new byte[f.Length + s.Length + t.Length];
            res = Merge(Merge(f, s), t);

            return res;
        }

        private static byte[] HEX(byte[] n)
        {
            string test = HexEncoding.ToString(n).ToLower();
            byte[] b = Encoding.UTF8.GetBytes(HexEncoding.ToString(n).ToLower());
            return b;
        }

        private static string GetMD5Hash(MD5 md5Hash, string input)
        {
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
            StringBuilder sBuilder = new StringBuilder();

            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            return sBuilder.ToString();
        }
    }
}
