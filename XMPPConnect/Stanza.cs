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
        private string _xmlData;
        private JSONNode _node;
        private StreamReader _streamReader;
        private string _dataPath;
        private StanzaType _type;

        public Stanza()
        {
            
        }

        public void SetXMLTemplatesFile(string dataPath)
        {
            _dataPath = dataPath;
            _streamReader = new StreamReader(_dataPath);
            _node = JSON.Parse(_streamReader.GetFileContent());
        }

        public void SetType(StanzaType type)
        {
            _type = type;
            _xmlData = _node[type.ToString().ToLower()];
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

        public bool ContainsData
        {
            get
            {
                return _xmlData.Contains(DataTemplate);
            }
        }

        public bool ContainsTagTo
        {
            get
            {
                return _xmlData.Contains(TagToTemplate);
            }
        }

        public bool ContainsTagFrom
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
        Handshake,
        Message,
        Digest_auth,
        Base_request,
        Sasl_on,
        Bind,
        Presense
    }
}
