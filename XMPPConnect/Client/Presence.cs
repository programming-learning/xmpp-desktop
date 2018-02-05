using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace XMPPConnect.Client
{
    public class Presence : Stanza
    {
        // Протестить шаблоны в отдельном проекте.
        //private readonly string _shTypePattern = "<show>.*<show/>";
        //private readonly string _shTypeDefault = "<show>%TYPE%<show/>";

        public Presence(ShowType type) : base(StanzaType.Presense)
        {
            ShowType = type;
            base._xmlData = base.GetChild(type);
            //base._xmlData = Regex.Replace(base._xmlData, _shTypePattern, _shTypeDefault.Replace(TypeTemplate, type.ToString().ToLower()));
        }

        //public string TypeTemplate
        //{
        //    get { return "%TYPE%"; }
        //}

        public ShowType ShowType { get; set; }
    }

    public enum ShowType
    {
        Default,
        Away,
        Chat
    }
}
