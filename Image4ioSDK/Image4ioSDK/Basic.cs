using System;
using System.Threading;
using Newtonsoft.Json;

namespace Image4ioSDK
{
    public static class Basic
    {
        public static string APIbase = "https://api.image4.io/v0.1/";
        public static TimeSpan m_TimeOut = Timeout.InfiniteTimeSpan;
        public static bool m_CloseConnection = true;
        public static JsonSerializerSettings JSONhandler = new JsonSerializerSettings() { MissingMemberHandling = MissingMemberHandling.Ignore, NullValueHandling = NullValueHandling.Ignore };
        public static string APIkey { get; set; }
        public static string APISecret { get; set; }
        public static string CloudName { get; set; }


        public class pUri : Uri
        {
            public pUri(string Action) : base(APIbase + Action) { }
        }

        private static ProxyConfig _proxy;
        public static ProxyConfig m_proxy
        {
            get
            {
                return _proxy ?? new ProxyConfig();
            }
            set
            {
                _proxy = value;
            }
        }

        public class HCHandler : System.Net.Http.HttpClientHandler
        {
            public HCHandler()
                : base()
            {
                if (m_proxy.SetProxy)
                {
                    base.MaxRequestContentBufferSize = 1 * 1024 * 1024;
                    base.Proxy = new System.Net.WebProxy(string.Format("http://{0}:{1}", m_proxy.ProxyIP, m_proxy.ProxyPort), true, null, new System.Net.NetworkCredential(m_proxy.ProxyUsername, m_proxy.ProxyPassword));
                    base.UseProxy = m_proxy.SetProxy;
                }
            }
        }

        public class HtpClient : System.Net.Http.HttpClient
        {
            public HtpClient(HCHandler HCHandler)
                : base(HCHandler)
            {
                base.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(APIkey + ":" + APISecret)));
                base.DefaultRequestHeaders.UserAgent.ParseAdd("Image4ioSDK");
                base.DefaultRequestHeaders.ConnectionClose = m_CloseConnection;
                Timeout = m_TimeOut;
            }

            public HtpClient(System.Net.Http.Handlers.ProgressMessageHandler progressHandler)
                : base(progressHandler)
            {
                base.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(APIkey + ":" + APISecret)));
                base.DefaultRequestHeaders.UserAgent.ParseAdd("Image4ioSDK");
                base.DefaultRequestHeaders.ConnectionClose = m_CloseConnection;
                Timeout = m_TimeOut;
            }
        }

        public static Image4ioException ShowError(string result)
        {
            var errorInfo = JsonConvert.DeserializeObject<JSON.JSON_Error>(result, JSONhandler);
            return new Image4ioException(errorInfo._ErrorMessage, errorInfo._ErrorCode);
        }


        public static string Slash(this string Path)
        {
            return string.Concat("/", Path.TrimStart('/'));
        }

        public static void Validation(this string Filename)
        {
            if (System.IO.Path.GetFileNameWithoutExtension(Filename.Trim('/')).Length < 3) throw new Image4ioException("Filename Lengt must be between 3 and 20 characters.", 101);
        }






    }
}
