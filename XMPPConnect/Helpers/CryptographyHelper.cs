using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Language.Xml;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using XMPPConnect.Managers;

namespace XMPPConnect.Helpers
{
    public static class CryptographyHelper
    {
        private static string _nc = "00000001";
        private static string _qop = "auth";

        public static string DigestMD5AuthAlgo(string xml, JabberID jid, string password)
        {
            var realm = jid.Server;
            var digestUri = "xmpp/" + jid.Server;
            string uniqData = StanzaManager.ParseChallenge(xml);
            Regex reg = new Regex("nonce=\"[0-9]*\"");
            Match m = reg.Match(uniqData);
            string nonce = string.Empty;
            if (m.Success)
            {
                nonce = m.Groups[0].Value;
                nonce = nonce.Replace("nonce=\"", "").Replace("\"", "");
            }

            string cNonce = Environment.MachineName; //GetUniqCNonce();
            string clientHash = GenerateClientHash(jid.Username, password, nonce, cNonce, realm, digestUri);
            string value = GetAuthenticationString(jid.Username, password, nonce, cNonce, clientHash, realm, digestUri);

            string baseResponse = Convert.ToBase64String(Encoding.UTF8.GetBytes(value));

            return baseResponse;
        }

        private static string GenerateClientHash(string username, string password, string nonce, string cNonce, string realm, string digestUri)
        {
            byte[] h1 = H(Encoding.UTF8.GetBytes(string.Format($"{username}:{realm}:{password}")));
            byte[] h2 = Encoding.UTF8.GetBytes($":{nonce}:{cNonce}");

            byte[] A1 = Merge(h1, h2);
            byte[] A2 = Encoding.UTF8.GetBytes(string.Format($"AUTHENTICATE:{digestUri}"));

            byte[] responseValue = HEX(KD(HEX(H(A1)), Merge(Encoding.UTF8.GetBytes($"{nonce}:{_nc}:{cNonce}:{_qop}:"), HEX(H(A2)))
                ));

            byte[] h3 = Merge(h1, h2);
            return Encoding.UTF8.GetString(responseValue);
        }

        private static string GetUniqCNonce()
        {
            string result = string.Empty;

            RandomNumberGenerator generator = RandomNumberGenerator.Create();
            byte[] data = new byte[8];
            generator.GetBytes(data);

            return HexEncoding.ToString(data).ToLower();
        }

        private static string GetAuthenticationString(string username, string password, string nonce, string cNonce, string response, string realm, string digestUri)
        {
            StringBuilder res = new StringBuilder();
            res.Append("username=\"" + username + "\"," +
                       "realm=\"" + realm + "\"," +
                       "nonce=\"" + nonce + "\"," +
                       "cnonce=\"" + cNonce + "\"," +
                       "nc=\"" + "00000001" + "\"," +
                       "qop=\"" + "auth" + "\"," +
                       "digest-uri=\"" + digestUri + "\"," +
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
