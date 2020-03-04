Public Class TransformationClient
    Implements ITransformation


    Private Property ImageMetadata As JSON.JSON_ImageMetadata

    Sub New(ImageMetadata As JSON.JSON_ImageMetadata)
        Me.ImageMetadata = ImageMetadata
    End Sub


#Region "ToWebP"
    Public Function _ToWebP(Quality As Integer, Optional Width As Integer? = Nothing, Optional Height As Integer? = Nothing) As String Implements ITransformation.ToWebP
        Dim sb As New Text.StringBuilder
        sb.AppendFormat("f_webp")
        sb.AppendFormat(",q_{0}", Quality)
        If Width.HasValue Then sb.Append(",w_").Append(Width.Value)
        If Height.HasValue Then sb.Append(",h_").Append(Height.Value)

        Return String.Format("https://cdn.image4.io/{0}/{1}{2}", CloudName, sb.ToString(), ImageMetadata.Path)
    End Function
#End Region

#Region "Resize"
    Public Function _Resize(Quality As Integer, Optional Width As Integer? = Nothing, Optional Height As Integer? = Nothing) As String Implements ITransformation.Resize
        Dim sb As New Text.StringBuilder
        sb.AppendFormat("q_{0}", Quality)
        If Width.HasValue Then sb.Append(",w_").Append(Width.Value)
        If Height.HasValue Then sb.Append(",h_").Append(Height.Value)

        Return String.Format("https://cdn.image4.io/{0}/{1}{2}", CloudName, sb.ToString(), ImageMetadata.Path)
    End Function
#End Region

#Region "Compress"
    Public Function _Compress(Quality As Integer) As String Implements ITransformation.Compress
        Return _Resize(Quality)
    End Function
#End Region


End Class
