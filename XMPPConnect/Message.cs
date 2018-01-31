using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace XMPPConnect
{
    public class Message : Stanza
    {
        private Regex _fromPattern;
        private Regex _toPattern;
        private Regex _bodyPattern;
        private string _from;
        private string _to;
        private string _body;
        public Message() : base()
        {
            _fromPattern = new Regex("");
            _toPattern = new Regex("");
            _bodyPattern = new Regex("");
            this.SetType(StanzaType.Message);
        }

        public 
    }
}
