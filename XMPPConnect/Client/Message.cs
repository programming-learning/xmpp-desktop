using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace XMPPConnect.Client
{
    public class Message : Stanza
    {
        private Regex _fromPattern;
        private Regex _toPattern;
        private Regex _bodyPattern;
        private JabberID _from;
        private JabberID _to;
        private string _body;

        public Message() : base(StanzaType.Message)
        {
            _toPattern = new Regex("to='.*' id");
            _fromPattern = new Regex("from='.*'>");
            _bodyPattern = new Regex("<body>.*</body>");
        }

        public Message(string from, string to, string msg) : this()
        {
            _from = new JabberID(from);
            _to = new JabberID(to);
            _body = msg;

            this._xmlData = this.ToString().Replace(Stanza.DataTemplate, msg).
                Replace(Stanza.TagToTemplate, to).
                Replace(Stanza.TagFromTemplate, from);
        }

        public Message(JabberID from, JabberID to, string msg) : this(from.Full, to.Full, msg) { }       

        public Message(string xml) : this()
        {
            Match from = _fromPattern.Match(xml);
            Match to = _toPattern.Match(xml);
            Match body = _bodyPattern.Match(xml);
            if(from.Success && to.Success && body.Success)
            {
                _from = new JabberID(from.Groups[0].Value.Replace("from='", "").Replace("'>", ""));
                _to = new JabberID(to.Groups[0].Value.Replace("to='", "").Replace("' id", ""));
                _body = body.Groups[0].Value.Replace("<body>", "").Replace("</body>", "");
            }

            this._xmlData = this.ToString().Replace(Stanza.DataTemplate, _body).
                Replace(Stanza.TagToTemplate, _to.Full).
                Replace(Stanza.TagFromTemplate, _from.Full);
        }

        public JabberID From
        {
            get
            {
                return _from;
            }
        }

        public JabberID To
        {
            get
            {
                return _to;
            }
        }

        public string Body
        {
            get
            {
                return _body;
            }
        }
    }
}
