using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMPPConnect.Helpers
{
    public static class StreamReaderExtensionMethods
    {
        public static string GetFileContent(this StreamReader reader)
        {
            string result = reader.ReadToEnd();
            reader.Close();
            return result;
        }
    }
}
