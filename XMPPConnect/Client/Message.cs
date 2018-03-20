using System;
using System.Text.RegularExpressions;
using XMPPConnect.Data;
using XMPPConnect.Providers;

namespace XMPPConnect.Client
{
    public class Message : Stanza // What is the purpose of inheritance? What fields do you use?
    {
        //private JsonDataProvider _dataProvider;
        private Regex _fromPattern;
        private Regex _toPattern;
        private Regex _bodyPattern;
        private JabberID _from;
        private JabberID _to;
        private string _body;
        //private string _data;

#region Properties
        public static string DataTemplate
        {
            get { return "%DATA%"; }
        }

        public static string TagToTemplate
        {
            get { return "%TO%"; }
        }

        public static string TagFromTemplate
        {
            get { return "%FROM%"; }
        }

        public JabberID From
        {
            get { return _from; }
        }

        public JabberID To
        {
            get { return _to; }
        }

        public string Body
        {
            get { return _body; }
        }
#endregion 

        public Message() : base(StanzaType.Message)
        {
            //_data = _dataProvider.GetXmlString(StanzaType.Message);
            _toPattern = new Regex("to='.*' xml:lang=");
            _fromPattern = new Regex("from='.*' to");
            _bodyPattern = new Regex("<body>.*</body>");
        }

        public Message(string from, string to, string msg) : this()
        {
            _from = new JabberID(from);
            _to = new JabberID(to);
            _body = msg;

            XmlData = XmlData.Replace(DataTemplate, msg).
                Replace(TagToTemplate, to).
                Replace(TagFromTemplate, from);
        }

        public Message(string xml) : this()
        {
            Match from = _fromPattern.Match(xml);
            Match to = _toPattern.Match(xml);
            Match body = _bodyPattern.Match(xml);
            if (from.Success && to.Success && body.Success)
            {
                _from = new JabberID(from.Groups[0].Value.Replace("from='", "").Replace(" to", ""));
                _to = new JabberID(to.Groups[0].Value.Replace("to='", "").Replace(" xml:lang=", ""));
                _body = body.Groups[0].Value.Replace("<body>", "").Replace("</body>", "");
            }

            XmlData = XmlData.Replace(DataTemplate, _body).
                Replace(TagToTemplate, _to.Full).
                Replace(TagFromTemplate, _from.Full);
        }

        public Message(JabberID from, JabberID to, string msg) : this(from.Full, to.Full, msg) { }       
    }
}
