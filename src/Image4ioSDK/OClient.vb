
Imports Newtonsoft.Json
Imports Image4ioSDK.JSON
Imports Image4ioSDK.utilitiez

Public Class OClient
    Implements IClient



    Public Sub New(_CloudName As String, _APIkey As String, _APISecret As String, Optional Settings As ConnectionSettings = Nothing)
        Net.ServicePointManager.Expect100Continue = True : Net.ServicePointManager.SecurityProtocol = Net.SecurityProtocolType.Tls Or Net.SecurityProtocolType.Tls11 Or Net.SecurityProtocolType.Tls12 Or Net.SecurityProtocolType.Ssl3
        If Settings Is Nothing Then
            m_proxy = Nothing
        Else
            m_proxy = Settings.Proxy
            m_CloseConnection = If(Settings.CloseConnection, True)
            m_TimeOut = If(Settings.TimeOut = Nothing, TimeSpan.FromMinutes(60), Settings.TimeOut)
        End If
        APIkey = _APIkey
        APISecret = _APISecret
        CloudName = _CloudName
    End Sub


    Public ReadOnly Property Transformation(ImageMetadata As JSON.JSON_ImageMetadata) As ITransformation Implements IClient.Transformation
        Get
            Return New TransformationClient(ImageMetadata)
        End Get
    End Property



#Region "ImageMetadata"
    Public Async Function _ImageMetadata(ImagePath As String) As Task(Of JSON_ImageMetadata) Implements IClient.ImageMetadata
        Using localHttpClient As New HttpClient(New HCHandler)
            Using response As Net.Http.HttpResponseMessage = Await localHttpClient.GetAsync(New pUri("get?" + String.Format("name={0}", ImagePath.Slash)), Net.Http.HttpCompletionOption.ResponseContentRead).ConfigureAwait(False)
                Dim result As String = Await response.Content.ReadAsStringAsync()

                If response.StatusCode = Net.HttpStatusCode.OK Then
                    Return JsonConvert.DeserializeObject(Of JSON_ImageMetadata)(result, JSONhandler)
                Else
                    ShowError(result)
                End If
            End Using
        End Using
    End Function
#End Region

#Region "UploadFile"
    Public Async Function _UploadLocal(FileToUpload As Object, UploadType As UploadTypes, DestinationFolderPath As String, FileName As String, UseFilename As Boolean, OverwriteIfExists As Boolean, Optional ReportCls As IProgress(Of ReportStatus) = Nothing, Optional token As Threading.CancellationToken = Nothing) As Task(Of JSON_Uploadedfile) Implements IClient.Upload
        'FileName.Validation()
        If ReportCls Is Nothing Then ReportCls = New Progress(Of ReportStatus)
        ReportCls.Report(New ReportStatus With {.Finished = False, .TextStatus = "Initializing..."})
        Try
            Dim progressHandler As New Net.Http.Handlers.ProgressMessageHandler(New HCHandler)
            AddHandler progressHandler.HttpSendProgress, (Function(sender, e)
                                                              ReportCls.Report(New ReportStatus With {.ProgressPercentage = e.ProgressPercentage, .BytesTransferred = e.BytesTransferred, .TotalBytes = If(e.TotalBytes Is Nothing, 0, e.TotalBytes), .TextStatus = "Uploading..."})
                                                          End Function)
            Dim localHttpClient As New HttpClient(progressHandler)
            Dim HtpReqMessage As New Net.Http.HttpRequestMessage(Net.Http.HttpMethod.Post, New pUri("upload?" + String.Format("path={0}", DestinationFolderPath.Trim("/"))))
            Dim MultipartsformData As New Net.Http.MultipartFormDataContent()
            Dim streamContent As Net.Http.HttpContent
            Select Case UploadType
                Case utilitiez.UploadTypes.FilePath
                    streamContent = New Net.Http.StreamContent(New IO.FileStream(FileToUpload, IO.FileMode.Open, IO.FileAccess.Read))
                Case utilitiez.UploadTypes.Stream
                    streamContent = New Net.Http.StreamContent(CType(FileToUpload, IO.Stream))
                Case utilitiez.UploadTypes.BytesArry
                    streamContent = New Net.Http.StreamContent(New IO.MemoryStream(CType(FileToUpload, Byte())))
            End Select

            MultipartsformData.Add(New Net.Http.StringContent(UseFilename), "use_filename")
            MultipartsformData.Add(New Net.Http.StringContent(OverwriteIfExists), "overwrite")
            MultipartsformData.Add(streamContent, "Image", FileName.Trim("/"))
            HtpReqMessage.Content = MultipartsformData
            '''''''''''''''''will write the whole content to H.D WHEN download completed'''''''''''''''''''''''''''''
            Using ResPonse As Net.Http.HttpResponseMessage = Await localHttpClient.SendAsync(HtpReqMessage, Net.Http.HttpCompletionOption.ResponseContentRead, token).ConfigureAwait(False)
                Dim result As String = Await ResPonse.Content.ReadAsStringAsync()

                token.ThrowIfCancellationRequested()
                If ResPonse.StatusCode = Net.HttpStatusCode.OK Then
                    ReportCls.Report(New ReportStatus With {.Finished = True, .TextStatus = String.Format("[{0}] Uploaded successfully", FileName)})
                    Return JsonConvert.DeserializeObject(Of JSON_Uploadedfile)(Linq.JObject.Parse(result).SelectToken("uploadedFiles[*]").ToString, JSONhandler)
                Else
                    ReportCls.Report(New ReportStatus With {.Finished = True, .TextStatus = String.Format("The request returned with HTTP status code {0}", Linq.JObject.Parse(result).SelectToken("message").ToString)})
                End If
            End Using
        Catch ex As Exception
            ReportCls.Report(New ReportStatus With {.Finished = True})
            If ex.Message.ToString.ToLower.Contains("a task was canceled") Then
                ReportCls.Report(New ReportStatus With {.TextStatus = ex.Message})
            Else
                Throw ExceptionCls.CreateException(ex.Message, ex.Message)
            End If
        End Try
    End Function
#End Region

#Region "UploadMultiableFiles"
    Structure MultiableUpload
        Property FileToUpload As Object
        Property FileName As String
    End Structure
    Public Async Function _UploadLocalMultiable(FilesToUpload As List(Of MultiableUpload), UploadType As UploadTypes, DestinationFolderPath As String, UseFilename As Boolean, OverwriteIfExists As Boolean, Optional ReportCls As IProgress(Of ReportStatus) = Nothing, Optional token As Threading.CancellationToken = Nothing) As Task(Of JSON_MultiUpload) Implements IClient.UploadMultiable
        If ReportCls Is Nothing Then ReportCls = New Progress(Of ReportStatus)
        ReportCls.Report(New ReportStatus With {.Finished = False, .TextStatus = "Initializing..."})
        Try
            Dim progressHandler As New Net.Http.Handlers.ProgressMessageHandler(New HCHandler)
            AddHandler progressHandler.HttpSendProgress, (Function(sender, e)
                                                              ReportCls.Report(New ReportStatus With {.ProgressPercentage = e.ProgressPercentage, .BytesTransferred = e.BytesTransferred, .TotalBytes = If(e.TotalBytes Is Nothing, 0, e.TotalBytes), .TextStatus = "Uploading..."})
                                                          End Function)
            Dim localHttpClient As New HttpClient(progressHandler)
            Dim HtpReqMessage As New Net.Http.HttpRequestMessage(Net.Http.HttpMethod.Post, New pUri("upload?" + String.Format("path={0}", DestinationFolderPath.Trim("/"))))
            Dim MultipartsformData As New Net.Http.MultipartFormDataContent()

            For Each Fle In FilesToUpload
                'Fle.FileName.Validation()
                Dim streamContent As Net.Http.HttpContent
                Select Case UploadType
                    Case utilitiez.UploadTypes.FilePath
                        streamContent = New Net.Http.StreamContent(New IO.FileStream(Fle.FileToUpload, IO.FileMode.Open, IO.FileAccess.Read))
                    Case utilitiez.UploadTypes.Stream
                        streamContent = New Net.Http.StreamContent(CType(Fle.FileToUpload, IO.Stream))
                    Case utilitiez.UploadTypes.BytesArry
                        streamContent = New Net.Http.StreamContent(New IO.MemoryStream(CType(Fle.FileToUpload, Byte())))
                End Select
                MultipartsformData.Add(streamContent, Fle.FileName.Trim("/"), Fle.FileName.Trim("/"))
            Next

            MultipartsformData.Add(New Net.Http.StringContent(UseFilename), "use_filename")
            MultipartsformData.Add(New Net.Http.StringContent(OverwriteIfExists), "overwrite")
            HtpReqMessage.Content = MultipartsformData
            '''''''''''''''''will write the whole content to H.D WHEN download completed'''''''''''''''''''''''''''''
            Using ResPonse As Net.Http.HttpResponseMessage = Await localHttpClient.SendAsync(HtpReqMessage, Net.Http.HttpCompletionOption.ResponseContentRead, token).ConfigureAwait(False)
                Dim result As String = Await ResPonse.Content.ReadAsStringAsync()

                token.ThrowIfCancellationRequested()
                If ResPonse.StatusCode = Net.HttpStatusCode.OK Then
                    ReportCls.Report(New ReportStatus With {.Finished = True, .TextStatus = String.Format("[{0}] Files Uploaded successfully", FilesToUpload.Count)})
                    Return JsonConvert.DeserializeObject(Of JSON_MultiUpload)(result, JSONhandler)
                Else
                    ReportCls.Report(New ReportStatus With {.Finished = True, .TextStatus = String.Format("The request returned with HTTP status code {0}", Linq.JObject.Parse(result).SelectToken("message").ToString)})
                End If
            End Using
        Catch ex As Exception
            ReportCls.Report(New ReportStatus With {.Finished = True})
            If ex.Message.ToString.ToLower.Contains("a task was canceled") Then
                ReportCls.Report(New ReportStatus With {.TextStatus = ex.Message})
            Else
                Throw ExceptionCls.CreateException(ex.Message, ex.Message)
            End If
        End Try
    End Function
#End Region

#Region "UploadRemotely"
    Public Async Function _UploadRemotely(ImgUrl As String, DestinationFolderPath As String) As Task(Of JSON_Uploadedfile) Implements IClient.UploadRemotely
        Using localHttpClient As New HttpClient(New HCHandler)
            Using response As Net.Http.HttpResponseMessage = Await localHttpClient.PostAsync(New pUri("fetch?" + String.Format("from={0}&target_path={1}", ImgUrl, DestinationFolderPath.Trim("/"))), Nothing).ConfigureAwait(False)
                Dim result As String = Await response.Content.ReadAsStringAsync()

                If response.StatusCode = Net.HttpStatusCode.OK Then
                    Return JsonConvert.DeserializeObject(Of JSON_Uploadedfile)(Linq.JObject.Parse(result).SelectToken("fetchedFile").ToString, JSONhandler)
                Else
                    ShowError(result)
                End If
            End Using
        End Using
    End Function
#End Region

#Region "CopyImage"
    Public Async Function _CopyImage(ImagePath As String, DestinationFolderPath As String) As Task(Of JSON_Uploadedfile) Implements IClient.ImageCopy
        Using localHttpClient As New HttpClient(New HCHandler)
            Using response As Net.Http.HttpResponseMessage = Await localHttpClient.PutAsync(New pUri("copy?" + String.Format("source={0}&Target_path={1}", ImagePath.Slash, DestinationFolderPath.Trim("/"))), Nothing).ConfigureAwait(False)
                Dim result As String = Await response.Content.ReadAsStringAsync()

                If response.StatusCode = Net.HttpStatusCode.OK Then
                    Return JsonConvert.DeserializeObject(Of JSON_Uploadedfile)(Linq.JObject.Parse(result).SelectToken("copiedFile").ToString, JSONhandler)
                Else
                    ShowError(result)
                End If
            End Using
        End Using
    End Function
#End Region

#Region "MoveImage"
    Public Async Function _MoveImage(ImagePath As String, DestinationFolderPath As String) As Task(Of JSON_Uploadedfile) Implements IClient.ImageMove
        Using localHttpClient As New HttpClient(New HCHandler)
            Using response As Net.Http.HttpResponseMessage = Await localHttpClient.PutAsync(New pUri("move?" + String.Format("source={0}&Target_path={1}", ImagePath.Slash, DestinationFolderPath.Trim("/"))), Nothing).ConfigureAwait(False)
                Dim result As String = Await response.Content.ReadAsStringAsync()

                If response.StatusCode = Net.HttpStatusCode.OK Then
                    Return JsonConvert.DeserializeObject(Of JSON_Uploadedfile)(Linq.JObject.Parse(result).SelectToken("movedFile").ToString, JSONhandler)
                Else
                    ShowError(result)
                End If
            End Using
        End Using
    End Function
#End Region

#Region "DeleteImage"
    Public Async Function _DeleteImage(ImagePath As String) As Task(Of JSON_Uploadedfile) Implements IClient.ImageDelete
        Using localHttpClient As New HttpClient(New HCHandler)
            Using response As Net.Http.HttpResponseMessage = Await localHttpClient.DeleteAsync(New pUri("deletefile?" + String.Format("name={0}", ImagePath.Slash))).ConfigureAwait(False)
                Dim result As String = Await response.Content.ReadAsStringAsync()

                If response.StatusCode = Net.HttpStatusCode.OK Then
                    Return JsonConvert.DeserializeObject(Of JSON_Uploadedfile)(Linq.JObject.Parse(result).SelectToken("deletedFile").ToString, JSONhandler)
                Else
                    ShowError(result)
                End If
            End Using
        End Using
    End Function
#End Region

#Region "DeleteFolder"
    Public Async Function _DeleteFolder(FolderPath As String) As Task(Of JSON_Uploadedfile) Implements IClient.FolderDelete
        Using localHttpClient As New HttpClient(New HCHandler)
            Using response As Net.Http.HttpResponseMessage = Await localHttpClient.DeleteAsync(New pUri("deletefolder?" + String.Format("path={0}", FolderPath.Trim("/")))).ConfigureAwait(False)
                Dim result As String = Await response.Content.ReadAsStringAsync()

                If response.StatusCode = Net.HttpStatusCode.OK Then
                    Return JsonConvert.DeserializeObject(Of JSON_Uploadedfile)(Linq.JObject.Parse(result).SelectToken("deletedFolder").ToString, JSONhandler)
                Else
                    ShowError(result)
                End If
            End Using
        End Using
    End Function
#End Region

#Region "CreateFolder"
    Public Async Function _CreateFolder(FolderName As String, DestinationFolderPath As String) As Task(Of JSON_Uploadedfile) Implements IClient.FolderCreate
        Dim folderPath = String.Concat(DestinationFolderPath.Trim("/"), "/", FolderName)

        Using localHttpClient As New HttpClient(New HCHandler)
            Using response As Net.Http.HttpResponseMessage = Await localHttpClient.PostAsync(New pUri("CreateFolder?" + String.Format("path={0}", folderPath)), Nothing).ConfigureAwait(False)
                Dim result As String = Await response.Content.ReadAsStringAsync()

                If response.StatusCode = Net.HttpStatusCode.OK Then
                    Return JsonConvert.DeserializeObject(Of JSON_Uploadedfile)(Linq.JObject.Parse(result).SelectToken("createdFolder").ToString, JSONhandler)
                Else
                    ShowError(result)
                End If
            End Using
        End Using
    End Function
#End Region

#Region "ListFolder"
    Public Async Function _ListFolder(FolderPath As String) As Task(Of JSON_FolderList) Implements IClient.FolderList
        Using localHttpClient As New HttpClient(New HCHandler)
            Using response As Net.Http.HttpResponseMessage = Await localHttpClient.GetAsync(New pUri("listfolder?" + String.Format("path={0}", FolderPath.Trim("/"))), Net.Http.HttpCompletionOption.ResponseContentRead).ConfigureAwait(False)
                Dim result As String = Await response.Content.ReadAsStringAsync()

                If response.StatusCode = Net.HttpStatusCode.OK Then
                    Return JsonConvert.DeserializeObject(Of JSON_FolderList)(result, JSONhandler)
                Else
                    ShowError(result)
                End If
            End Using
        End Using
    End Function
#End Region



End Class
