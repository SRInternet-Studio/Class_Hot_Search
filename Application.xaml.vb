Class Application

    ' Application-level events, such as Startup, Exit, and DispatcherUnhandledException
    ' can be handled in this file.
    Public Shared environMajorVersion As String = String.Empty

    Private Sub Application_Startup(sender As Object, e As StartupEventArgs) Handles Me.Startup
        CheckDotNetVersion()
    End Sub

    Private Function CheckDotNetVersion() As Boolean
        ' 获取当前已安装的 .NET 版本
        Dim version As String = Environment.Version.ToString()
        ' 解析版本号
        Dim majorVersion As Integer = Integer.Parse(version.Split("."c)(0))
        environMajorVersion = majorVersion.ToString
    End Function
End Class
