using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XMPPConnect.Client;

namespace XMPPConnect.Strategies
{
    class ErrorResolver :IDataResolveStrategy
    {
        public void InvokeOnData(object sender, object dataObject)
        {
            Error error = (Error)dataObject;
            XmppClientConnection connection = (XmppClientConnection)sender;

            connection.InvokeOnError(error);
        }
    }
}
