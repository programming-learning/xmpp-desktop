﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XMPPConnect.Desktop.ViewModels;

namespace XMPPConnect.Desktop.Infrastructure.RequestParams
{
    public class SendMessageRequestParams
    {
        private XmppClientConnection _connection;
        private ClientVModel _clientVModel;

        public SendMessageRequestParams(XmppClientConnection connection,ClientVModel clientVModel)
        {
            _connection = connection;
            _clientVModel = clientVModel;
        }

        public XmppClientConnection Connection
        {
            get { return _connection; }
            set { _connection = value; }
        }

        public ClientVModel Client
        {
            get { return _clientVModel; }
            set { _clientVModel = value; }
        }
    }
}
