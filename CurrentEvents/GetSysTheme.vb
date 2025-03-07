Imports MaterialDesignThemes.Wpf
Imports MaterialDesignThemes.Wpf.Theme
Imports Microsoft.Win32
Module GetSysTheme
    Public Function GetSysTheme() As String
        Try
            Select Case GetSystemTheme()
                Case BaseTheme.Dark
                    Return "Dark"
                Case BaseTheme.Light
                    Return "Light"
                Case Else
                    Return GetSysThemeByReg()
            End Select
        Catch ex As Exception
            Return GetSysThemeByReg()
        End Try
    End Function

    Public Function GetSysThemeByReg() As String
        Try

            Dim subKey As String = "Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"
            Dim key As Object = Registry.CurrentUser.OpenSubKey(subKey).GetValue("AppsUseLightTheme")

            If key IsNot Nothing AndAlso key.ToString() = "0" Then
                Return "Dark"
            Else
                Return "Light"
            End If

        Catch ex As Exception
            ' 无法读取注册表
            Debug.WriteLine("无法获取系统主题: " & ex.Message)
            Return "Light"
        End Try
    End Function
End Module
