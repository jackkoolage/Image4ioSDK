## Image4ioSDK

`Download:`[https://github.com/jackkoolage/Image4ioSDK/releases](https://github.com/jackkoolage/Image4ioSDK/releases)<br>
`Help:`[https://github.com/jackkoolage/Image4ioSDK/wiki](https://github.com/jackkoolage/Image4ioSDK/wiki)<br>
`NuGet:`
[![NuGet](https://img.shields.io/nuget/v/DeQmaTech.Image4ioSDK.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/DeQmaTech.Image4ioSDK)<br>


# Features:
* Assemblies for .NET 4.5.2 and .NET Standard 2.0 and .NET Core 2.1
* Just one external reference (Newtonsoft.Json)
* Easy installation using NuGet
* Upload/Download tracking support
* Proxy Support
* Upload/Download cancellation support


# List of functions:
**Management**
1. ImageMetadata
1. ImageCopy
1. ImageMove
1. ImageDelete
1. FolderDelete
1. FolderCreate
1. FolderList
1. Upload
1. UploadMultiable
1. UploadRemotely

**Transformation**
1. ToWebP
1. Resize
1. Compress

# CodeMap:
![codemap](https://i.postimg.cc/FK1nJndz/io-codemap.png)

# Code simple:
```vb
''set proxy and connection options
Dim con As New Image4ioSDK.ConnectionSettings With {.CloseConnection = True, .TimeOut = TimeSpan.FromMinutes(30), .Proxy = New Image4ioSDK.ProxyConfig With {.SetProxy = True, .ProxyIP = "127.0.0.1", .ProxyPort = 8888, .ProxyUsername = "user", .ProxyPassword = "pass"}}
''set api client
Dim CLNT As Image4ioSDK.IClient = New Image4ioSDK.OClient("cloudname", "apikey", "secretkey", con)

''general
CLNT.FolderCreate("folder name", "folder path")
CLNT.FolderDelete("folder path")
CLNT.FolderList("folder path")
CLNT.ImageCopy("image path", "folder path")
CLNT.ImageDelete("image path")
CLNT.ImageMetadata("image path")
CLNT.ImageMove("image path", "folder path")
Dim cts As New Threading.CancellationTokenSource()
Dim _ReportCls As New Progress(Of Image4ioSDK.ReportStatus)(Sub(ReportClass As Image4ioSDK.ReportStatus) Console.WriteLine(String.Format("{0} - {1}% - {2}", String.Format("{0}/{1}", (ReportClass.BytesTransferred), (ReportClass.TotalBytes)), CInt(ReportClass.ProgressPercentage), ReportClass.TextStatus)))
CLNT.Upload("c:\\VIDO.jpg", Image4ioSDK.utilitiez.UploadTypes.FilePath, "", "VIDO.jpg", True, True, _ReportCls, cts.Token)
CLNT.UploadMultiable(New List(Of Image4ioSDK.OClient.MultiableUpload) From {New Image4ioSDK.OClient.MultiableUpload With {.FileToUpload = "c:\\VIDO.jpg", .FileName = "VIDO.jpg"}}, Image4ioSDK.utilitiez.UploadTypes.FilePath, "", True, False, _ReportCls, cts.Token)
CLNT.UploadRemotely("https://domain.com/watch.png", "")

''Transformation
CLNT.Transformation(New Image4ioSDK.JSON.JSON_ImageMetadata).Compress(75)
CLNT.Transformation(New Image4ioSDK.JSON.JSON_ImageMetadata).Resize(75, 1024, 768)
CLNT.Transformation(New Image4ioSDK.JSON.JSON_ImageMetadata).ToWebP(75, 1024, 768)
```
