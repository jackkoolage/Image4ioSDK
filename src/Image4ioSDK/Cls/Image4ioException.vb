Imports System

Public Class Image4ioException
    Inherits Exception

    Public Sub New(ByVal errorMessage As String, ByVal errorCode As String)
        MyBase.New(errorMessage)
    End Sub
End Class


Public Class ExceptionCls
    Public Shared Function CreateException(errorMesage As String, errorCode As String) As Image4ioException
        Return New Image4ioException(errorMesage, errorCode)
    End Function
End Class

