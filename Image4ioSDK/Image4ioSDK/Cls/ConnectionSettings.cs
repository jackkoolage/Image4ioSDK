using System;
using System.Collections.Generic;
using System.Text;
using Image4ioSDK;

namespace Image4ioSDK
{
    public class ConnectionSettings
    {
        public TimeSpan? TimeOut = null;
        public bool? CloseConnection = true;
        public ProxyConfig Proxy = null;
    }
}
