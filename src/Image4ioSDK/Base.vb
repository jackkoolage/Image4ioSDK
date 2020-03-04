Imports Newtonsoft.Json

Module Base


    Public Property APIbase As String = "https://api.image4.io/v0.1/"
    Public Property m_TimeOut As System.TimeSpan = Threading.Timeout.InfiniteTimeSpan
    Public Property m_CloseConnection As Boolean = True
    Public Property JSONhandler As New Newtonsoft.Json.JsonSerializerSettings() With {.MissingMemberHandling = Newtonsoft.Json.MissingMemberHandling.Ignore, .NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore}
    Public Property APIkey As String
    Public Property APISecret As String
    Public Property CloudName As String


    Public Class pUri
        Inherits Uri
        Sub New(Action As String)
            MyBase.New(APIbase + Action)
        End Sub
    End Class

    Private _proxy As ProxyConfig
    Public Property m_proxy As ProxyConfig
        Get
            Return If(_proxy, New ProxyConfig)
        End Get
        Set(value As ProxyConfig)
            _proxy = value
        End Set
    End Property

    Public Class HCHandler
        Inherits Net.Http.HttpClientHandler
        Sub New()
            MyBase.New()
            If m_proxy.SetProxy Then
                MaxRequestContentBufferSize = 1 * 1024 * 1024
                Proxy = New Net.WebProxy(String.Format("http://{0}:{1}", m_proxy.ProxyIP, m_proxy.ProxyPort), True, Nothing, New Net.NetworkCredential(m_proxy.ProxyUsername, m_proxy.ProxyPassword))
                UseProxy = m_proxy.SetProxy
            End If
        End Sub
    End Class

    Public Class HttpClient
        Inherits Net.Http.HttpClient
        Sub New(HCHandler As HCHandler)
            MyBase.New(HCHandler)
            MyBase.DefaultRequestHeaders.Authorization = New Net.Http.Headers.AuthenticationHeaderValue("Basic", System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(APIkey + ":" + APISecret)))
            MyBase.DefaultRequestHeaders.UserAgent.ParseAdd("Image4ioSDK")
            DefaultRequestHeaders.ConnectionClose = m_CloseConnection
            Timeout = m_TimeOut
        End Sub
        Sub New(progressHandler As Net.Http.Handlers.ProgressMessageHandler)
            MyBase.New(progressHandler)
            MyBase.DefaultRequestHeaders.Authorization = New Net.Http.Headers.AuthenticationHeaderValue("Basic", System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(APIkey + ":" + APISecret)))
            MyBase.DefaultRequestHeaders.UserAgent.ParseAdd("Image4ioSDK")
            DefaultRequestHeaders.ConnectionClose = m_CloseConnection
            Timeout = m_TimeOut
        End Sub
    End Class

    Function ShowError(result As String)
        Dim errorInfo = Newtonsoft.Json.JsonConvert.DeserializeObject(Of JSON.JSON_Error)(result, JSONhandler)
        Throw ExceptionCls.CreateException(errorInfo._ErrorMessage, errorInfo._ErrorCode)
    End Function

    <Runtime.CompilerServices.Extension()>
    Public Function Slash(Path As String) As String
        Return String.Concat("/", Path.TrimStart("/"c))
    End Function
    ''' <summary>
    ''' FilenameValidation
    ''' </summary>
    <Runtime.CompilerServices.Extension()>
    Public Sub Validation(Filename As String)
        If IO.Path.GetFileNameWithoutExtension(Filename.Trim("/")).Length < 3 Then Throw ExceptionCls.CreateException("Filename Lengt must be between 3 and 20 characters.", 101)
    End Sub
End Module
