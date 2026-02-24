Public NotInheritable Class AppSession2
    Private Sub New()
    End Sub

    Public Shared Property IsLoggedIn As Boolean = False
    Public Shared Property UserId As Integer = 0
    Public Shared Property Username As String = ""
    Public Shared Property Role As String = "" ' "admin" or "user"

    Public Shared ReadOnly Property IsAdmin As Boolean
        Get
            Return String.Equals(Role, "admin", StringComparison.OrdinalIgnoreCase)
        End Get
    End Property

    Public Shared Sub Logout()
        IsLoggedIn = False
        UserId = 0
        Username = ""
        Role = ""
    End Sub
End Class
