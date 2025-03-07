Public Class CurrentEvents
    Public Shared Event ThemeChangedEvent(message As String)

    Public Shared Sub RaiseThemeChangedEvent(message As String)
        RaiseEvent ThemeChangedEvent(message)
    End Sub
End Class