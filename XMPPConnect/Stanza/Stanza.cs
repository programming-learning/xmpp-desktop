using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XMPPConnect.Helpers;

namespace XMPPConnect
{
    public class Stanza
    {
        protected string _xmlData;
        private JSONNode _node;
        private StreamReader _streamReader;
        private const string _dataPath = "JSONs\\xmlRequests.json";
        protected StanzaType _type;

        public Stanza()
        {
            _streamReader = new StreamReader(_dataPath);
            _node = JSON.Parse(_streamReader.GetFileContent());
        }

        public Stanza(StanzaType type) : this()
        {
            // Потом добавить Additional Type. Из Presence добавить PresenceType.
            _type = type;
            _xmlData = _node[type.ToString().ToLower()];
        }

        // Для вытаскивания под-json objects
        protected string GetChild(Enum type)
        {
            return _node[_type.ToString().ToLower()][type.ToString().ToLower()];
        }

        public override string ToString()
        {
            return _xmlData;
        }

        public static string DataTemplate
        {
            get
            {
                return "%DATA%";
            }
        }

        public static string TagToTemplate
        {
            get
            {
                return "%TO%";
            }
        }

        public static string TagFromTemplate
        {
            get
            {
                return "%FROM%";
            }
        }

        public bool ContainsDataTemplate
        {
            get
            {
                return _xmlData.Contains(DataTemplate);
            }
        }

        public bool ContainsTagToTemplate
        {
            get
            {
                return _xmlData.Contains(TagToTemplate);
            }
        }

        public bool ContainsTagFromTemplate
        {
            get
            {
                return _xmlData.Contains(TagFromTemplate);
            }
        }

        public StanzaType Type
        {
            get
            {
                return _type;
            }
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
        Presence
    }
}
