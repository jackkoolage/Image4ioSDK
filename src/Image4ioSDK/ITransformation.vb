Public Interface ITransformation
    Function ToWebP(Quality As Integer, Optional Width As Integer? = Nothing, Optional Height As Integer? = Nothing) As String
    Function Resize(Quality As Integer, Optional Width As Integer? = Nothing, Optional Height As Integer? = Nothing) As String
    Function Compress(Quality As Integer) As String

End Interface
