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
        private string _validatePattern;

        public JabberID(string jid)
        {
            _validatePattern = "@";
            Init(jid);
        }

        private void Init(string jid)
        {
            if(ValidateJid(jid))
            {
                string[] jidDataArr = jid.Split('@');
                _user = jidDataArr[0];
                _server = jidDataArr[1];
                _jid = jid;
            }
            else
            {
                throw new ArgumentException("Illegal username.");
            }
        }

        private bool ValidateJid(string jid)
        {
            int amount = new Regex(_validatePattern).Matches(jid).Count;
            if (amount > 1)
            {
                return false;
            }

            return true;
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
            }
            set { _jid = value; }
        }
    }
}
