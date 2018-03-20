using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XMPPConnect.Helpers;

namespace XMPPConnect.Data
{
    public class Stanza
    {
        private string _xmlData;
        private JSONNode _node;
        private StreamReader _streamReader;
        private readonly string _dataPath;
        protected StanzaType _type;

        protected string XmlData
        {
            set { _xmlData = value; }
            get { return _xmlData; }
        }

        public StanzaType Type
        {
            get { return _type; }
        }

        public Stanza()
        {
            _dataPath = "JSONs\\xmlRequests.json";
            _streamReader = new StreamReader(_dataPath);
            _node = JSON.Parse(_streamReader.GetFileContent());
            _type = StanzaType.None;
        }

        public Stanza(StanzaType type) : this()
        {
            _type = type;
            _xmlData = _node[type.ToString().ToLower()];
        }

        // Для вытаскивания json-подтипов
        protected string GetChild(Enum type)
        {
            return _node[_type.ToString().ToLower()][type.ToString().ToLower()];
        }

        public override string ToString()
        {
            return _xmlData;
        }
    }

    public enum StanzaType
    {
        Header,
        Message,
        Digest_auth,
        Base_request,
        Sasl_on,
        Bind,
        Presence,
        Error,
        None
    }
}
