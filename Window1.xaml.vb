Imports MaterialDesignThemes.Wpf

Public Class Window1

    Public Property IsOpen As Boolean = False
    Private ReadOnly paletteHelper As New PaletteHelper()

    Private Sub bilibili_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles bilibili.MouseLeftButtonDown
        Dim startInfo As New ProcessStartInfo()
        startInfo.FileName = "https://space.bilibili.com/1969160969"
        startInfo.UseShellExecute = True ' 确保这行代码存在

        Process.Start(startInfo)
    End Sub

    Private Sub tiktok_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles tiktok.MouseLeftButtonDown
        Dim startInfo As New ProcessStartInfo()
        startInfo.FileName = "https://www.douyin.com/user/MS4wLjABAAAATzdjtBBrLLCn69TtPMeseuEUzztbNZzw-9f13adrfiM?relation=0&vid=7387043479261678902"
        startInfo.UseShellExecute = True ' 确保这行代码存在

        Process.Start(startInfo)
    End Sub

    Private Sub sr_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles sr.MouseLeftButtonDown
        Dim startInfo As New ProcessStartInfo()
        startInfo.FileName = "https://qm.qq.com/q/8sVnGBdeJW"
        startInfo.UseShellExecute = True ' 确保这行代码存在

        Process.Start(startInfo)
    End Sub

    Private Sub sr2_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles sr2.MouseLeftButtonDown
        Dim startInfo As New ProcessStartInfo()
        startInfo.FileName = "https://qm.qq.com/q/8sVnGBdeJW"
        startInfo.UseShellExecute = True ' 确保这行代码存在

        Process.Start(startInfo)
    End Sub

    Private Sub github_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles github.MouseLeftButtonDown

    End Sub

    Private Sub github_Click(sender As Object, e As RoutedEventArgs) Handles github.Click
        Dim startInfo As New ProcessStartInfo()
        startInfo.FileName = "https://github.com/SRInternet-Studio/Class_Hot_Search"
        startInfo.UseShellExecute = True ' 确保这行代码存在

        Process.Start(startInfo)
    End Sub

    Private Sub Window1_Closed(sender As Object, e As EventArgs) Handles Me.Closed
        IsOpen = False
    End Sub

    Private Sub Window1_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        IsOpen = True

        AddHandler CurrentEvents.ThemeChangedEvent, AddressOf OnThemeChanged
        OnThemeChanged()

        ProductionInfoText.Text = $"版本：{UpdateChecker.versionParts}。基于 Material Design 3 设计, .NET {Application.environMajorVersion} 平台构建。"
    End Sub

    Private Sub OnThemeChanged()
        Dim AppPath = AppContext.BaseDirectory()
        If paletteHelper.GetTheme().GetBaseTheme() = BaseTheme.Dark Then
            tiktok.Source = New BitmapImage(New Uri(IO.Path.Combine(AppPath, "assets/Tiktok_round_dark.png")))
            bilibili.Source = New BitmapImage(New Uri(IO.Path.Combine(AppPath, "assets/bilibili_round_dark.png")))
        Else
            tiktok.Source = New BitmapImage(New Uri(IO.Path.Combine(AppPath, "assets/Tiktok_round.png")))
            bilibili.Source = New BitmapImage(New Uri(IO.Path.Combine(AppPath, "assets/bilibili_round.png")))
        End If
    End Sub
End Class
