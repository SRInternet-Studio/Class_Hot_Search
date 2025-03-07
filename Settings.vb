Imports System.IO
Imports Newtonsoft.Json
Public Class AppSettings
    Public Property Title As String = "班级热搜"
    Public Property BackgroundDark As String = "Normal"
    Public Property BackgroundLight As String = "Normal"
    Public Property HeadImageDark As String = "Normal"
    Public Property HeadImageLight As String = "Normal"
    Public Property NoChangeHot As Boolean = False
    Public Property RecordPosition As Boolean = True
    Public Property AutoDarkMode As Boolean = True
End Class

Public Class Settings
    Public Shared settings As AppSettings
    Public Shared AppPath = AppContext.BaseDirectory()

    Public Shared Sub Read()
        Dim filePath As String = AppPath & "\hot_settings.Sr"

        If File.Exists(filePath) Then
            Try
                Dim jsonString As String = File.ReadAllText(filePath)
                settings = JsonConvert.DeserializeObject(Of AppSettings)(jsonString)
            Catch ex As Exception
                settings = New AppSettings() ' 创建默认设置
            End Try
        Else
            settings = New AppSettings() ' 创建默认设置
        End If
    End Sub
End Class
