using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Image4ioSDK
{
   public class ProxyConfig
    {
            public bool SetProxy {get;set;}
    public string ProxyIP {get;set;}
    public int ProxyPort {get;set;}
    public string ProxyUsername { get; set; }
    public string ProxyPassword { get; set; }
    }
}
