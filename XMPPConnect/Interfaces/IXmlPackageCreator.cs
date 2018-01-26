using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XMPPConnect.Interfaces
{
    interface IXmlPackageCreator
    {
        string CreatePackage(XmlPackageTemplate template);
    }
}
