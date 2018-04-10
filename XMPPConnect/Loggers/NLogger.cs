using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
namespace XMPPConnect.Loggers
{
    public static class NLogger
    {
        private static NLog.Logger _log;

        public static ILogger Log
        {
            get
            {
                if(_log == null)
                {
                    _log = NLog.LogManager.GetCurrentClassLogger();
                }

                return _log;
            }
        }
    }
}
