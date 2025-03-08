Imports System.IO

Public Class LoadImageToMEM
    Public Shared Function LoadImageFromPath(imagePath As String) As BitmapImage
        Dim bitmapImage As New BitmapImage()
        Using fileStream As New FileStream(imagePath, FileMode.Open, FileAccess.Read, FileShare.Read)
            bitmapImage.BeginInit()
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad ' 完全加载到内存中
            bitmapImage.StreamSource = fileStream
            bitmapImage.EndInit()
        End Using
        bitmapImage.Freeze() ' 冻结 BitmapImage 以便在 UI 线程中使用
        Return bitmapImage
    End Function
End Class
