Imports Newtonsoft.Json
Imports System.ComponentModel

Namespace JSON


    Public Enum ResultsStatusEnum
        Uploaded
        fetched
        Copied
        Moved
        Deleted
        Created
        AlreadyExist
        Overwrited
        [Error]
    End Enum

    Public Class JSON_Error
        <JsonProperty("message")> Public Property _ErrorMessage As String
        <JsonProperty("statusCode")> Public Property _ErrorCode As Integer
    End Class


#Region "JSON_ImageMetadata"
    Public Class JSON_ImageMetadata
        'https://stackoverflow.com/posts/43715009/revisions
        <JsonProperty("userGivenName")>
        Public Property Filename As String
        <JsonProperty("orginal_name")>
        Private WriteOnly Property Filename2 As String
            Set(ByVal value As String)
                Filename = value
            End Set
        End Property

        Public Property folder As String ''list only, not in single metadata
        <JsonProperty("name")> Public Property Path As String
        Public Property size As Integer
        Public Property format As String
        Public Property width As Integer
        Public Property height As Integer
        Public Property createdAtUTC As Date
        Public Property updatedAtUTC As Date

        Public ReadOnly Property DirectUrl As String
            Get
                Return String.Format("https://cdn.image4.io/{0}/f_auto{1}", CloudName, Path)
            End Get
        End Property
    End Class
#End Region

#Region "JSON_Upload"
    Public Class JSON_MultiUpload
        Public Property uploadedFiles As List(Of JSON_Uploadedfile)
    End Class
    Public Class JSON_Uploadedfile
        Public Property orginal_name As String
        Public Property name As String
        Public Property status As ResultsStatusEnum
    End Class
#End Region

#Region "JSON_FolderList"
    Public Class JSON_FolderList
        Public Property folders As List(Of JSON_FolderMetadata)
        Public Property files As List(Of JSON_ImageMetadata)
    End Class
    Public Class JSON_FolderMetadata
        Public Property name As String
        Public Property parent As String
    End Class
#End Region



End Namespace

