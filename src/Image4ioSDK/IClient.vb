Imports Image4ioSDK
Imports Image4ioSDK.JSON

Public Interface IClient


    ReadOnly Property Transformation(ImageMetadata As JSON_ImageMetadata) As ITransformation


    Function ImageMetadata(ImagePath As String) As Task(Of JSON.JSON_ImageMetadata)
    Function Upload(FileToUpload As Object, UploadType As utilitiez.UploadTypes, DestinationFolderPath As String, FileName As String, UseFilename As Boolean, OverwriteIfExists As Boolean, Optional ReportCls As IProgress(Of ReportStatus) = Nothing, Optional token As Threading.CancellationToken = Nothing) As Task(Of JSON_Uploadedfile)
    Function UploadMultiable(FilesToUpload As List(Of OClient.MultiableUpload), UploadType As utilitiez.UploadTypes, DestinationFolderPath As String, UseFilename As Boolean, OverwriteIfExists As Boolean, Optional ReportCls As IProgress(Of ReportStatus) = Nothing, Optional token As Threading.CancellationToken = Nothing) As Task(Of JSON_MultiUpload)
    Function UploadRemotely(ImgUrl As String, DestinationFolderPath As String) As Task(Of JSON_Uploadedfile)

    Function ImageCopy(ImagePath As String, DestinationFolderPath As String) As Task(Of JSON_Uploadedfile)
    Function ImageMove(ImagePath As String, DestinationFolderPath As String) As Task(Of JSON_Uploadedfile)
    Function ImageDelete(ImagePath As String) As Task(Of JSON_Uploadedfile)

    Function FolderDelete(FolderPath As String) As Task(Of JSON_Uploadedfile)
    Function FolderCreate(FolderName As String, DestinationFolderPath As String) As Task(Of JSON_Uploadedfile)
    Function FolderList(FolderPath As String) As Task(Of JSON_FolderList)

End Interface
