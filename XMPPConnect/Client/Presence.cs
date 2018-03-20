using System;
using XMPPConnect.Data;

namespace XMPPConnect.Client
{
    public class Presence : Stanza
    {
        public ShowType ShowType { get; set; }

        public Presence(ShowType type) : base()
        {
            ShowType = type;
            base._type = StanzaType.Presence;
            base.XmlData = base.GetChild(type);
        }
    }

    public enum ShowType
    {
        Show,
        Away,
        Chat
    }
}
