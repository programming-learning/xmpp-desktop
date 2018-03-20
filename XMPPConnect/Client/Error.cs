using System;
using XMPPConnect.Data;

namespace XMPPConnect.Client
{
    public class Error : Stanza
    {
        private string _message;
        private string _source;

        public string Message
        {
            get { return _message; }
        }

        public string Source
        {
            get { return _source; }
        }

        public Error(Exception ex)
        {
            _message = ex.Message;
            _source = ex.Source;
        }
    }
}
