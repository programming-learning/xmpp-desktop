using Microsoft.Language.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMPPConnect
{
    public class StanzaManager
    {
        private Stanza _stanza;
        
        public StanzaManager(/*StanzaType type*/)
        {
            //_stanza = new Stanza(type);
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
            _stanza = new Stanza(type);
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

        public static string ParseChallenge(string xml)
        {
            XmlDocumentSyntax root = Parser.ParseText(xml);

            string base64Info = string.Empty;
            foreach (IXmlElement node in root.Elements)
            {
                if (node.Name == "challenge")
                {
                    base64Info = node.Value;
                }
            }

            byte[] uniqDataB = new byte[1024];
            uniqDataB = Convert.FromBase64String(base64Info);

            return Encoding.Default.GetString(uniqDataB);
        }
    }
}
