using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMPPConnect.Strategies
{
    internal class NoneResolver : IDataResolveStrategy
    {
        public void InvokeOnData(object sender, object dataObject){}
    }
}
