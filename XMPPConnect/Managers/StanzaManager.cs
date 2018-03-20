using Microsoft.Language.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XMPPConnect.Client;
using XMPPConnect.Data;

namespace XMPPConnect.Managers
{
    internal class StanzaManager
    {
        public StanzaManager()
        {
            
        }

        public string GetXML(StanzaType type, string data = null, string to = null, string from = null)
        {
            var stanza = new Stanza(type);
            return stanza.ToString().Replace(Message.DataTemplate, data).
                Replace(Message.TagToTemplate, to).
                Replace(Message.TagFromTemplate, from);
        }

        public Stanza GetStanzaObject(string xmlString)
        {
            Stanza result = new Stanza();
            if (xmlString.Contains("message"))
            {
                result = new Message(xmlString);
            }
            else if (xmlString.Contains("presence"))
            {
                result = new Presence(ShowType.Show);
            }
            else if (xmlString.Contains("error") || xmlString.Contains("failure"))
            {
                result = new Error(new Exception(xmlString));
            }

            return result;
        }

        internal static string ParseChallenge(string xml)
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
