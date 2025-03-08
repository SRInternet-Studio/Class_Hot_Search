Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.IO
Imports MdXaml
Imports Microsoft.Win32
Imports Newtonsoft.Json

Public Class Window2

    Public Event WindowClosed As EventHandler
    Public Property IsOpen As Boolean = False
    Public updateResult As UpdateCheckResult
    Public Shared AppPath = AppContext.BaseDirectory()

    Private Sub Window2_Closing(sender As Object, e As ComponentModel.CancelEventArgs) Handles Me.Closing

        If String.IsNullOrWhiteSpace(TitleBox.Text) Then
            e.Cancel = True
            MsgBox("标题必须为有效字符组成，不能空格、换行符或直接为空。", vbExclamation, "错误")
            Return
        End If

        Settings.settings.Title = TitleBox.Text
        Settings.settings.NoChangeHot = NoChangeHot.IsChecked
        Settings.settings.RecordPosition = RecordPosition.IsChecked
        Settings.settings.AutoDarkMode = AutoDarkChanger.IsChecked

        Dim jsonString As String = JsonConvert.SerializeObject(Settings.settings, Formatting.Indented) ' 使用缩进提高可读性
        File.WriteAllText(AppPath & "\hot_settings.Sr", jsonString)

        If Not RecordPosition.IsChecked And File.Exists(Path.Combine(AppPath, "Window.ini")) Then
            File.Delete(Path.Combine(AppPath, "Window.ini"))
        End If
    End Sub

    Private Sub Window2_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        IsOpen = True
        Try
            If Settings.settings.BackgroundLight <> "Normal" Then
                LighBackImg.Source = LoadImageToMEM.LoadImageFromPath(Path.Combine(AppPath, "resource/HotSearch.png"))
            End If
            If Settings.settings.BackgroundDark <> "Normal" Then
                DarkBackImg.Source = LoadImageToMEM.LoadImageFromPath(Path.Combine(AppPath, "resource/HotSearch.Dark.png"))
            End If
            If Settings.settings.HeadImageLight <> "Normal" Then
                LighHeadImg.Source = LoadImageToMEM.LoadImageFromPath(Path.Combine(AppPath, "resource/HotSearchHead.png"))
            End If
            If Settings.settings.HeadImageDark <> "Normal" Then
                DarkHeadImg.Source = LoadImageToMEM.LoadImageFromPath(Path.Combine(AppPath, "resource/HotSearchHead.Dark.png"))
            End If

            NoChangeHot.IsChecked = Settings.settings.NoChangeHot
            RecordPosition.IsChecked = Settings.settings.RecordPosition
            AutoDarkChanger.IsChecked = Settings.settings.AutoDarkMode
            TitleBox.Text = Settings.settings.Title
            UpdateUUID.Text = UpdateChecker.informationalVersion
        Catch ex As Exception
            MessageBox.Show("发生错误，设置文件可能已经损坏: " & vbCrLf & ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error)
        End Try
    End Sub

    Private Async Sub CompletedTip()
        SnackbarOne.IsActive = True
        Await Task.Delay(3000)
        SnackbarOne.IsActive = False
    End Sub

    Private Sub HeadCustom_Click(sender As Object, e As RoutedEventArgs) Handles HeadCustom.Click
        If Not Directory.Exists(Path.Combine(AppPath, "resource")) Then
            Directory.CreateDirectory(Path.Combine(AppPath, "resource"))
        End If
        Dim outputFilePath As String = Path.Combine(AppPath, "resource/HotSearchHead.png")
        If openImage(outputFilePath, "选取头图封面", False) Then
            Settings.settings.HeadImageLight = outputFilePath
            LighHeadImg.Source = LoadImageToMEM.LoadImageFromPath(Settings.settings.HeadImageLight)
            CompletedTip()
        End If

    End Sub

    Private Sub BackgroungCustom_Click(sender As Object, e As RoutedEventArgs) Handles BackgroungCustom.Click
        If Not Directory.Exists(Path.Combine(AppPath, "resource")) Then
            Directory.CreateDirectory(Path.Combine(AppPath, "resource"))
        End If
        Dim outputFilePath As String = Path.Combine(AppPath, "resource/HotSearch.png")
        If openImage(outputFilePath, "选取背景图片", True) Then
            Settings.settings.BackgroundLight = outputFilePath
            LighBackImg.Source = LoadImageToMEM.LoadImageFromPath(Settings.settings.BackgroundLight)
            CompletedTip()
        End If

    End Sub

    Private Sub BackgroungCustomDark_Click(sender As Object, e As RoutedEventArgs) Handles BackgroungCustomDark.Click
        If Not Directory.Exists(Path.Combine(AppPath, "resource")) Then
            Directory.CreateDirectory(Path.Combine(AppPath, "resource"))
        End If
        Dim outputFilePath As String = Path.Combine(AppPath, "resource/HotSearch.Dark.png")
        If openImage(outputFilePath, "选取背景图片（深色模式）", True) Then
            Settings.settings.BackgroundDark = outputFilePath
            DarkBackImg.Source = LoadImageToMEM.LoadImageFromPath(Settings.settings.BackgroundDark)
            CompletedTip()
        End If

    End Sub

    Private Sub HeadCustomDark_Click(sender As Object, e As RoutedEventArgs) Handles HeadCustomDark.Click
        If Not Directory.Exists(Path.Combine(AppPath, "resource")) Then
            Directory.CreateDirectory(Path.Combine(AppPath, "resource"))
        End If
        Dim outputFilePath As String = Path.Combine(AppPath, "resource/HotSearchHead.Dark.png")
        If openImage(outputFilePath, "选取头图封面（深色模式）", False) Then
            Settings.settings.HeadImageDark = outputFilePath
            DarkHeadImg.Source = LoadImageToMEM.LoadImageFromPath(Settings.settings.HeadImageDark)
            CompletedTip()
        End If

    End Sub

    Private Sub BackgroungNormal_Click(sender As Object, e As RoutedEventArgs) Handles BackgroungNormal.Click
        Settings.settings.BackgroundLight = "Normal"
        Settings.settings.BackgroundDark = "Normal"
        LighBackImg.Source = New BitmapImage(New Uri("pack://application:,,,/HotSearch.png"))
        DarkBackImg.Source = New BitmapImage(New Uri("pack://application:,,,/HotSearch.Dark.png"))
        CompletedTip()
    End Sub

    Private Sub HeadNormal_Click(sender As Object, e As RoutedEventArgs) Handles HeadNormal.Click
        Settings.settings.HeadImageLight = "Normal"
        Settings.settings.HeadImageDark = "Normal"
        LighHeadImg.Source = New BitmapImage(New Uri("pack://application:,,,/HotSearchHead.png"))
        DarkHeadImg.Source = New BitmapImage(New Uri("pack://application:,,,/HotSearchHead.Dark.png"))
        CompletedTip()
    End Sub

    Private Sub CheckUpdates_Click(sender As Object, e As RoutedEventArgs) Handles CheckUpdates.Click
        Dim startInfo As New ProcessStartInfo()
        startInfo.FileName = "https://github.com/SRInternet-Studio/Class_Hot_Search/releases"
        startInfo.UseShellExecute = True ' 确保这行代码存在

        Process.Start(startInfo)
    End Sub

    Private Function openImage(outputFilePath As String, t As String, IsBackImage As Boolean) As Boolean
        Dim openFileDialog As New OpenFileDialog()
        openFileDialog.Filter = "可控图片格式|*.jpg;*.jpeg;*.png;*.bmp"
        openFileDialog.Title = t

        If openFileDialog.ShowDialog() = True Then
            Dim selectedFilePath As String = openFileDialog.FileName
            ' 处理图像
            Dim image As Bitmap = Nothing

            Try

                Try
                    image = New System.Drawing.Bitmap(selectedFilePath)
                Catch ex As Exception
                    MessageBox.Show("图片不受支持或已损坏，或图片格式与实际扩展名不符。请更换图片后重试。 ", "班级热搜排行榜 - 内存安全阀", MessageBoxButton.OK, MessageBoxImage.Error)
                    Return False
                    Exit Function
                End Try

                If IsBackImage Then
                    ' 检查分辨率
                    If image.Width > 1080 OrElse image.Height > 1920 Then
                        ' 如果分辨率超过1080P，执行压缩
                        Dim compressedImage As Bitmap = CompressImage(image, 1080, 1920)
                        compressedImage.Save(outputFilePath, ImageFormat.Png)
                        compressedImage.Dispose()
                        'MessageBox.Show("图片已超出 1080P，已经被压缩并保存。", "完成", MessageBoxButton.OK)
                    Else
                        ' 如果分辨率不超过1080P，直接复制文件
                        File.Copy(selectedFilePath, outputFilePath, True)
                        'MessageBox.Show("图片已复制到根目录。", "完成", MessageBoxButton.OK)
                    End If
                Else
                    ' 检查分辨率
                    If image.Width > 1920 OrElse image.Height > 1080 Then
                        ' 如果分辨率超过1080P，执行压缩
                        Dim compressedImage As Bitmap = CompressImage(image, 1920, 1080)
                        If compressedImage IsNot Nothing Then
                            compressedImage.Save(outputFilePath, ImageFormat.Png)
                        Else
                            MsgBox("图像过大，大于 1080p ，请调整图片大小后重试。", vbExclamation, "班级热搜排行榜 - 内存安全阀")
                        End If

                        compressedImage.Dispose()
                        'MessageBox.Show("图片已超出 1080P，已经被压缩并保存。", "完成", MessageBoxButton.OK)
                    Else
                        ' 如果分辨率不超过1080P，直接复制文件
                        File.Copy(selectedFilePath, outputFilePath, True)
                        'MessageBox.Show("图片已复制到根目录。", "完成", MessageBoxButton.OK)
                    End If
                End If
                Return True
            Catch ex As Exception
                MessageBox.Show("发生错误：" & ex.ToString & $"{vbCrLf}   at {selectedFilePath} {vbCrLf}更换图片可能有助于解决此问题。", "班级热搜排行榜 - 内存安全阀 - 错误", MessageBoxButton.OK, MessageBoxImage.Error)
            Finally
                ' 确保资源得到释放
                If image IsNot Nothing Then
                    image.Dispose()
                End If

            End Try
        End If
        Return False
    End Function

    Private Function CompressImage(originalImage As Bitmap, maxWidth As Integer, maxHeight As Integer) As Bitmap
        ' 检查参数有效性
        If maxWidth <= 0 OrElse maxHeight <= 0 Then
            Throw New ArgumentException("maxWidth 和 maxHeight 必须大于 0。")
        End If

        ' 计算缩放比例
        Dim ratioX As Double = CDbl(maxWidth) / originalImage.Width
        Dim ratioY As Double = CDbl(maxHeight) / originalImage.Height
        Dim ratio As Double = Math.Min(ratioX, ratioY)

        ' 计算新的宽高
        Dim newWidth As Integer = CInt(originalImage.Width * ratio)
        Dim newHeight As Integer = CInt(originalImage.Height * ratio)

        ' 创建新的图像
        Dim compressedImage As Bitmap = Nothing ' 初始化为 Nothing
        Try
            compressedImage = New Bitmap(newWidth, newHeight)
            Using g As Graphics = Graphics.FromImage(compressedImage)
                g.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
                g.DrawImage(originalImage, 0, 0, newWidth, newHeight)
            End Using
            Return compressedImage
        Catch ex As Exception
            ' 处理 Bitmap 创建失败的情况
            Debug.WriteLine("压缩图像时出错: " & ex.Message)
            If compressedImage IsNot Nothing Then
                compressedImage.Dispose() ' 确保在异常情况下释放
            End If
            Return Nothing
        End Try
    End Function

    Private Sub Window2_Closed(sender As Object, e As EventArgs) Handles Me.Closed
        IsOpen = False
        RaiseEvent WindowClosed(Me, EventArgs.Empty)
    End Sub

    Private Async Sub CUpdate_Click(sender As Object, e As RoutedEventArgs) Handles CUpdate.Click
        DUpdate.Visibility = Visibility.Collapsed
        UpdateProgress.Visibility = Visibility.Visible
        UpdateVersionName.Text = "正在瞅一眼更新……"
        UpdateInformation.Document.Blocks.Clear()
        updateResult = Await UpdateChecker.GetLatestReleaseInfoAsync()

        Dim new_text
        If UpdateChecker.ExceptionMessage <> String.Empty Then
            new_text = New FlowDocument()
            Dim para As New Paragraph()
            para.Inlines.Add(New Run(UpdateChecker.ExceptionMessage))
            new_text.Blocks.Add(para)
        Else
            Dim texts As String
            If updateResult.LatestVersion Is Nothing Then
                texts = "没有更新。"
            Else
                texts = updateResult.LatestVersion.Body
            End If

            Dim engine As New Markdown()
            Try
                new_text = engine.Transform(texts)
            Catch ex As Exception
                new_text = New FlowDocument()
                Dim para As New Paragraph()
                para.Inlines.Add(New Run(texts))
                new_text.Blocks.Add(para)
            End Try
        End If
        UpdateProgress.Visibility = Visibility.Hidden
        UpdateInformation.Document = new_text

        If updateResult.IsNewVersionAvailable Then
            UpdateVersionName.Text = $"瞅到更新：{updateResult.CurrentVersion} -> {updateResult.LatestVersion.Name}"
            Select Case updateResult.DownloadType
                Case DownloadType.DirectDownload
                    DUpdate.Visibility = Visibility.Visible
                Case DownloadType.ReleasePage
                    DUpdate.Visibility = Visibility.Visible
            End Select
        Else
            UpdateVersionName.Text = $"没有更新"
        End If
    End Sub

    Private Sub DUpdate_Click(sender As Object, e As RoutedEventArgs) Handles DUpdate.Click
        Select Case updateResult.DownloadType
            Case DownloadType.DirectDownload
                Dim startInfo As New ProcessStartInfo()
                startInfo.FileName = updateResult.DownloadUrl
                startInfo.UseShellExecute = True ' 确保这行代码存在

                Process.Start(startInfo)
            Case DownloadType.ReleasePage
                Dim startInfo As New ProcessStartInfo()
                startInfo.FileName = updateResult.DownloadUrl
                startInfo.UseShellExecute = True ' 确保这行代码存在

                Process.Start(startInfo)
        End Select
    End Sub

    Private Sub PageFounder_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles PageFounder.SelectionChanged
        Select Case PageFounder.SelectedIndex
            Case 0
                Normal.Visibility = Visibility.Visible
                Personality.Visibility = Visibility.Collapsed
                Updating.Visibility = Visibility.Collapsed
            Case 1
                Normal.Visibility = Visibility.Collapsed
                Personality.Visibility = Visibility.Visible
                Updating.Visibility = Visibility.Collapsed
            Case 2
                Normal.Visibility = Visibility.Collapsed
                Personality.Visibility = Visibility.Collapsed
                Updating.Visibility = Visibility.Visible
        End Select
    End Sub
End Class
