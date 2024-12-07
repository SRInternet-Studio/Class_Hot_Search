Imports System.IO
Public Class Settings
    Public Shared settings() As String
    Public Shared AppPath = AppContext.BaseDirectory()

    Public Shared Sub Read()
        Dim UserSetting = File.ReadAllText(AppPath & "\hot_settings.Sr")
        settings = UserSetting.Split({Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries)
    End Sub
End Class
