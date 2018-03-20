using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XMPPConnect.Client;
using XMPPConnect.Data;
using XMPPConnect.Helpers;

namespace XMPPConnect.Providers
{
    internal class JsonDataProvider
    {
        private JSONNode _node;
        private StreamReader _streamReader;
        private readonly string _dataPath;

        public JsonDataProvider()
        {
            _dataPath = "JSONs\\xmlRequests.json";
            _streamReader = new StreamReader(_dataPath);
            _node = JSON.Parse(_streamReader.GetFileContent());
        }

        public string GetXmlString(StanzaType parent, string data = null, string to = null, string from = null)
        {
            return _node[parent.ToString().ToLower()].ToString().Replace(Message.DataTemplate, data)
                .Replace(Message.TagToTemplate, to).Replace(Message.TagFromTemplate, from);
        }

        public string GetXmlString(StanzaType parent, StanzaType child, string data = null, string to = null,
            string from = null)
        {
            return _node[parent.ToString().ToLower()][child.ToString().ToLower()].ToString()
                .Replace(Message.DataTemplate, data).Replace(Message.TagToTemplate, to)
                .Replace(Message.TagFromTemplate, from);
        }

        public string GetXML(StanzaType type, string data = null, string to = null, string from = null)
        {
            var stanza = new Stanza(type);
            return stanza.ToString().Replace(Message.DataTemplate, data).Replace(Message.TagToTemplate, to)
                .Replace(Message.TagFromTemplate, from);
        }
    }
}
