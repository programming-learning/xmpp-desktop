using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XMPPConnect.Client;

namespace XMPPConnect.Strategies
{
    internal interface IDataResolveStrategy
    {
        void InvokeOnData(object sender, object dataObject);
    }
}
