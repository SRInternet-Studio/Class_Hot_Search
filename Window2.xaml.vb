Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.IO
Imports Microsoft.Win32

Public Class Window2

    Public Event WindowClosed As EventHandler
    Public Property IsOpen As Boolean = False

    Public Shared AppPath = AppContext.BaseDirectory()
    Dim head_image As String
    Dim back_image As String

    Private Sub Window2_Closing(sender As Object, e As ComponentModel.CancelEventArgs) Handles Me.Closing

        If String.IsNullOrWhiteSpace(back_image) Then
            back_image = "Normal"
        End If

        If String.IsNullOrWhiteSpace(head_image) Then
            head_image = "Normal"
        End If

        If String.IsNullOrWhiteSpace(TitleBox.Text) Then
            e.Cancel = True
            MsgBox("标题必须为有效字符组成，不能空格、换行符或直接为空。", vbExclamation, "错误")
            Return
        End If

        Dim settingsList As New List(Of String) From {
    NoChangeHot.IsChecked.ToString(),
    back_image,
    head_image,
    TitleBox.Text,
    RecordPosition.IsChecked.ToString()
}
        Settings.settings = settingsList.ToArray()

        Dim settingsString As String = String.Join(Environment.NewLine, Settings.settings)
        File.WriteAllText(AppPath & "\hot_settings.Sr", settingsString)

        If Not RecordPosition.IsChecked And File.Exists(Path.Combine(AppPath, "Window.ini")) Then
            File.Delete(Path.Combine(AppPath, "Window.ini"))
        End If
    End Sub

    Private Sub Window2_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        IsOpen = True
        Try
            back_image = Settings.settings(1)
            head_image = Settings.settings(2)
            NoChangeHot.IsChecked = Convert.ToBoolean(Settings.settings(0))
            RecordPosition.IsChecked = Convert.ToBoolean(Settings.settings(4))
            TitleBox.Text = Settings.settings(3)
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
        openImage(outputFilePath, "选取头图封面", False)
        CompletedTip()
    End Sub

    Private Sub BackgroungCustom_Click(sender As Object, e As RoutedEventArgs) Handles BackgroungCustom.Click
        If Not Directory.Exists(Path.Combine(AppPath, "resource")) Then
            Directory.CreateDirectory(Path.Combine(AppPath, "resource"))
        End If
        Dim outputFilePath As String = Path.Combine(AppPath, "resource/HotSearch.png")
        openImage(outputFilePath, "选取背景图片", True)
        CompletedTip()
    End Sub

    Private Sub BackgroungNormal_Click(sender As Object, e As RoutedEventArgs) Handles BackgroungNormal.Click
        back_image = "Normal"
        CompletedTip()
    End Sub

    Private Sub HeadNormal_Click(sender As Object, e As RoutedEventArgs) Handles HeadNormal.Click
        head_image = "Normal"
        CompletedTip()
    End Sub

    Private Sub CheckUpdates_Click(sender As Object, e As RoutedEventArgs) Handles CheckUpdates.Click
        Dim startInfo As New ProcessStartInfo()
        startInfo.FileName = "https://github.com/SRInternet-Studio/Class_Hot_Search/releases"
        startInfo.UseShellExecute = True ' 确保这行代码存在

        Process.Start(startInfo)
    End Sub

    Private Function openImage(outputFilePath As String, t As String, IsBackImage As Boolean)
        Dim openFileDialog As New OpenFileDialog()
        openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png"
        openFileDialog.Title = t

        If openFileDialog.ShowDialog() = True Then
            Dim selectedFilePath As String = openFileDialog.FileName
            ' 处理图像
            Dim image As Bitmap = Nothing

            Try
                image = New Bitmap(selectedFilePath)

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

                    back_image = outputFilePath
                Else
                    ' 检查分辨率
                    If image.Width > 1920 OrElse image.Height > 1080 Then
                        ' 如果分辨率超过1080P，执行压缩
                        Dim compressedImage As Bitmap = CompressImage(image, 1920, 1080)
                        compressedImage.Save(outputFilePath, ImageFormat.Png)
                        compressedImage.Dispose()
                        'MessageBox.Show("图片已超出 1080P，已经被压缩并保存。", "完成", MessageBoxButton.OK)
                    Else
                        ' 如果分辨率不超过1080P，直接复制文件
                        File.Copy(selectedFilePath, outputFilePath, True)
                        'MessageBox.Show("图片已复制到根目录。", "完成", MessageBoxButton.OK)
                    End If

                    head_image = outputFilePath
                End If

            Catch ex As Exception
                MessageBox.Show("发生错误: " & ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error)
            Finally
                ' 确保资源得到释放
                If image IsNot Nothing Then
                    image.Dispose()
                End If
            End Try
        End If
    End Function

    Private Function CompressImage(originalImage As Bitmap, maxWidth As Integer, maxHeight As Integer) As Bitmap
        ' 计算缩放比例
        Dim ratioX As Double = maxWidth / originalImage.Width
        Dim ratioY As Double = maxHeight / originalImage.Height
        Dim ratio As Double = Math.Min(ratioX, ratioY)

        ' 计算新的宽高
        Dim newWidth As Integer = CInt(originalImage.Width * ratio)
        Dim newHeight As Integer = CInt(originalImage.Height * ratio)

        ' 创建新的图像
        Dim compressedImage As New Bitmap(newWidth, newHeight)
        Using g As Graphics = Graphics.FromImage(compressedImage)
            g.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
            g.DrawImage(originalImage, 0, 0, newWidth, newHeight)
        End Using

        Return compressedImage
    End Function

    Private Sub Window2_Closed(sender As Object, e As EventArgs) Handles Me.Closed
        IsOpen = False
        RaiseEvent WindowClosed(Me, EventArgs.Empty)
    End Sub
End Class
