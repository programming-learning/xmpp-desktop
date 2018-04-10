using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using XMPPConnect.Loggers;

namespace UnitTests
{
    [SetUpFixture]
    public class SetUp
    {
        [SetUp]
        public void InitLogs()
        {
            //NLogger.InitLogger();
        }
    }
}
