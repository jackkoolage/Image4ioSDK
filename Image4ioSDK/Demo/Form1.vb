Public Class Form1
    Private Async Sub Button1_ClickAsync(sender As Object, e As EventArgs) Handles Button1.Click
        Dim Clint As Image4ioSDK.IClient = New Image4ioSDK.OClient("cloud_name", "api_key", "api_secret", New Image4ioSDK.ConnectionSettings With {.CloseConnection = True, .TimeOut = TimeSpan.FromMinutes(80), .Proxy = New Image4ioSDK.ProxyConfig With {.SetProxy = True, .ProxyIP = "127.0.0.1", .ProxyPort = 80, .ProxyUsername = "user", .ProxyPassword = "123456"}})

        Await Clint.FolderCreate("folder_name", "folder_path")
        Await Clint.FolderDelete("folder_name")
        Await Clint.FolderList("folder_path")
        Await Clint.ImageCopy("image_path", "folder_path")
        Await Clint.ImageDelete("image_path")
        Await Clint.ImageMetadata("image_path")
        Await Clint.ImageMove("image_path", "folder_path")

        Dim CancelToken As New Threading.CancellationTokenSource()
        Dim _ReportCls As New Progress(Of Image4ioSDK.ReportStatus)(Sub(r) Console.WriteLine($"{r.BytesTransferred}/{r.TotalBytes}" + r.ProgressPercentage + If(r.TextStatus, "Downloading...")))
        Await Clint.Upload("C:\Down\mypic.jpg", Image4ioSDK.Utilitiez.UploadTypes.FilePath, "folder_path", "mypic.jpg", True, False, _ReportCls, CancelToken.Token)
        Dim multiFiles = New List(Of Image4ioSDK.OClient.MultiableUpload)
        multiFiles.Add(New Image4ioSDK.OClient.MultiableUpload With {.FileToUpload = "C:\Down\mypic1.jpg", .FileName = "mypic1.jpg"})
        multiFiles.Add(New Image4ioSDK.OClient.MultiableUpload With {.FileToUpload = "C:\Down\mypic2.jpg", .FileName = "mypic2.jpg"})
        Await Clint.UploadMultiable(multiFiles, Image4ioSDK.Utilitiez.UploadTypes.FilePath, "folder_path", True, False, _ReportCls, CancelToken.Token)
        Await Clint.UploadRemotely("https://domain.com/mypic.jpg", "folder_path")
        Await Clint.Download("image_path", "C:\Down", "mypic.jpg", _ReportCls, CancelToken.Token)

        '' Transformation
        Dim meta = Await Clint.ImageMetadata("tzt/40d78f11-2b11-4169-aa73-b953573666cd.jpg")
        Clint.Transformation(meta).Compress(75)
        Clint.Transformation(meta).Resize(75, 800, 600)
        Clint.Transformation(meta).ToWebP(75, 800, 600)
    End Sub
End Class
