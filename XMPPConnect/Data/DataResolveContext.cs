using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XMPPConnect.Strategies;

namespace XMPPConnect.Data
{
    internal class DataResolveContext : IContext
    {
        private readonly IDataResolveStrategy _resolveStrategy;
        public DataResolveContext(IDataResolveStrategy strategy)
        {
            _resolveStrategy = strategy;
        }

        public void Execute(object sender, object data)
        {
            _resolveStrategy.InvokeOnData(sender, data);
        }
    }
}
