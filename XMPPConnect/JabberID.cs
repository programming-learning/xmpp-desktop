using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace XMPPConnect
{
    public class JabberID
    {
        private string _jid;
        private string _user;
        private string _server;
        private string _nonce;
        private const string _validatePattern = "@";

        public JabberID(string jid)
        {
            Init(jid);
        }

        private void Init(string jid)
        {
            if(ValidateJid(jid))
            {
                string[] jidDataArr = jid.Split('@');
                _user = jidDataArr[0];
                if (jidDataArr[1].Contains("/"))
                {
                    _server = jidDataArr[1].Split('/')[0];
                    _nonce = jidDataArr[1].Split('/')[1];
                }
                else
                {
                    _server = jidDataArr[1];
                    _nonce = string.Empty;
                }

                _jid = jid;
            }
            else
            {
                throw new ArgumentException("Illegal username.");
            }
        }

        public static bool ValidateJid(string jid)
        {
            int amount = new Regex(_validatePattern).Matches(jid).Count;
            if (amount == 1)
            {
                return true;
            }

            return false;
        }

        public override string ToString()
        {
            return Full;
        }

        public string Username
        {
            get { return _user; }
            set { _user = value; }
        }

        public string Server
        {
            get { return _server; }
            set { _server = value; }
        }

        public string Full
        {
            get
            {
                return _jid = _user + "@" + _server;
                //if (string.IsNullOrEmpty(_nonce))
                //{
                //    return _jid = _user + "@" + _server;
                //}
                //else
                //{
                //    return _jid = _user + "@" + _server + "/" + _nonce;
                //}
            }
            set
            {
                Init(value);      
            }
        }
    }
}
