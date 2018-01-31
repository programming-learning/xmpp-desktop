using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMPPConnect
{
    public class StanzaManager
    {
        private readonly string _dataPath; 
        private Stanza _stanza;
        
        public StanzaManager(StanzaType type)
        {
            _dataPath = "JSONs\\xmlRequests.json";
            _stanza = new Stanza(_dataPath);
        }

        //public string GetHandshakeXML()
        //{
        //    _stanza.SetType(StanzaType.Handshake);
        //    return _stanza.ToString();
        //}

        //public string GetDigestAuthXML()
        //{
        //    _stanza.SetType(StanzaType.Digest_auth);
        //    return _stanza.ToString();
        //}

        //public string GetBase64RequestXML(string baseRequest)
        //{
        //    _stanza.SetType(StanzaType.Base_request);
        //    string baseXML = _stanza.ToString().Replace("%DATA%", baseRequest);
        //    return baseXML;
        //}

        //public string GetSaslXML()
        //{
        //    _stanza.SetType(StanzaType.Sasl_on);
        //    return _stanza.ToString();
        //}

        public string GetXML(StanzaType type, string data = null, string to = null, string from = null)
        {
            string resultXML = string.Empty;
            _stanza.SetType(type);
            return _stanza.ToString().Replace(Stanza.DataTemplate, data).
                Replace(Stanza.TagToTemplate, to).
                Replace(Stanza.TagFromTemplate, from);

            //if (string.IsNullOrEmpty(data))
            //{
            //    resultXML = _stanza.ToString();
            //}
            //else if(!string.IsNullOrEmpty(data) && !string.IsNullOrEmpty(to) && !string.IsNullOrEmpty(from))
            //{
            //    resultXML = _stanza.ToString().Replace("%DATA%", data).Replace("%TO", to).Replace("%FROM%", from);
            //}
            //else if(!string.IsNullOrEmpty(data) && string.IsNullOrEmpty(to) && string.IsNullOrEmpty(from))
            //{

            //}
        }

        public string ParseChallenge(string xml)
        {
            string ch = string.Empty;
            return ch;
        }
    }
}
