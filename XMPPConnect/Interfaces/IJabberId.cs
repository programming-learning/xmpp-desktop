using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMPPConnect.Interfaces
{
    public interface IJabberId
    {
        string Username { get; set; }
        string Server { get; set; }
    }
}
