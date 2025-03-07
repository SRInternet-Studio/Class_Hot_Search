Imports System.Net.Http
Imports System.Net.Http.Headers
Imports System.Reflection
Imports Newtonsoft.Json

Public Class UpdateChecker

    Private Shared ReadOnly GitHubOwner As String = "SRInternet-Studio"
    Private Shared ReadOnly GitHubRepo As String = "Class_Hot_Search"
    Private Shared ReadOnly GitHubPersonalAccessToken As String = Keys.GitHubPersonalAccessToken
    Private Shared ReadOnly HttpClient As New HttpClient()

    Public Shared informationSet As Assembly = Assembly.GetExecutingAssembly()
    Public Shared fileVersionInfo As FileVersionInfo = FileVersionInfo.GetVersionInfo(informationSet.Location)
    Public Shared fileVersion As String = fileVersionInfo.FileVersion
    Public Shared informationalVersion As String = fileVersionInfo.ProductVersion
    Public Shared versionParts As String = informationalVersion.Split("+"c)(0)

    Public Shared ExceptionMessage As String = String.Empty

    Public Shared Async Function GetLatestReleaseInfoAsync() As Task(Of UpdateCheckResult)
        Dim result As New UpdateCheckResult()

        Try
            ' 获取当前应用程序版本
            Dim currentVersion As Version = Assembly.GetExecutingAssembly().GetName().Version
            result.CurrentVersion = currentVersion

            ' 从 GitHub API 获取最新 Release 信息
            Dim latestRelease As GitHubRelease = Await GetLatestReleaseAsync()

            If latestRelease Is Nothing Then
                result.IsNewVersionAvailable = False
                Return result
            End If

            ' 获取下载 URL 信息
            Dim downloadInfo As DownloadInfo = GetDownloadInfo(latestRelease)
            result.DownloadUrl = downloadInfo.Url
            result.DownloadType = downloadInfo.Type

            ' 比较版本
            Dim latestVersion As Version = New Version(latestRelease.TagName.TrimStart("v"c)) ' 移除 "v" 前缀

            If latestVersion > currentVersion Then
                result.IsNewVersionAvailable = True
            Else
                result.IsNewVersionAvailable = False
            End If

            result.LatestVersion = latestRelease

            ExceptionMessage = String.Empty
        Catch ex As Exception
            ExceptionMessage = "更新检查出错: " & ex.Message
            result.IsNewVersionAvailable = False
        End Try

        Return result
    End Function

    Private Shared Async Function GetLatestReleaseAsync() As Task(Of GitHubRelease)
        Try
            HttpClient.DefaultRequestHeaders.UserAgent.Add(New ProductInfoHeaderValue("UpdateChecker", "1.0"))

            ' 添加 Authorization Header (使用 Personal Access Token)
            'HttpClient.DefaultRequestHeaders.Authorization = New AuthenticationHeaderValue("Bearer", GitHubPersonalAccessToken)

            Dim apiUrl As String = $"http://api.github.com/repos/{GitHubOwner}/{GitHubRepo}/releases/latest"
            Console.WriteLine($"API URL: {apiUrl}")

            Dim response As HttpResponseMessage = Await HttpClient.GetAsync(apiUrl)
            Console.WriteLine($"响应状态码: {response.StatusCode}")
            Dim responseContent As String = Await response.Content.ReadAsStringAsync()
            Console.WriteLine($"响应内容: {responseContent}")

            response.EnsureSuccessStatusCode()

            Dim release As GitHubRelease = Nothing
            Try
                release = JsonConvert.DeserializeObject(Of GitHubRelease)(responseContent)
                Console.WriteLine($"Release 对象反序列化成功: {release IsNot Nothing}")
            Catch ex As Exception
                Console.WriteLine($"反序列化错误: {ex.Message}")
                Return Nothing ' 反序列化失败，返回 Nothing
            End Try

            Console.WriteLine($"Release 对象: {release}")
            Return release

            ExceptionMessage = String.Empty
        Catch ex As Exception
            ExceptionMessage = "获取最新 Release 错误: " & ex.Message
            Return Nothing
        End Try
    End Function

    Private Shared Function GetDownloadInfo(release As GitHubRelease) As DownloadInfo
        Dim exeUrls As New List(Of String)()

        If release IsNot Nothing AndAlso release.Assets IsNot Nothing Then
            For Each asset As GitHubReleaseAsset In release.Assets
                If asset.Name.ToLower().EndsWith(".exe") Or asset.Name.ToLower().EndsWith(".zip") Then
                    exeUrls.Add(asset.BrowserDownloadUrl)
                End If
            Next
        End If

        Dim downloadInfo As New DownloadInfo()

        If exeUrls.Count = 0 Then
            downloadInfo.Type = DownloadType.None
            downloadInfo.Url = String.Empty ' 没有找到发布文件
        ElseIf exeUrls.Count = 1 Then
            ' 单个 EXE 文件，直接下载
            downloadInfo.Type = DownloadType.DirectDownload
            downloadInfo.Url = exeUrls(0)
        Else
            ' 多个 EXE 文件，打开 Release 页面
            downloadInfo.Type = DownloadType.ReleasePage
            downloadInfo.Url = release.HtmlUrl
        End If

        Return downloadInfo
    End Function

End Class

Public Class UpdateCheckResult
    Public Property IsNewVersionAvailable As Boolean
    Public Property LatestVersion As GitHubRelease
    Public Property CurrentVersion As Version
    Public Property DownloadUrl As String
    Public Property DownloadType As DownloadType  ' 下载类型
End Class

Public Enum DownloadType
    None
    DirectDownload
    ReleasePage
End Enum

Public Class DownloadInfo
    Public Property Url As String
    Public Property Type As DownloadType
End Class

Public Class GitHubRelease
    <JsonProperty("url")>
    Public Property Url As String

    <JsonProperty("assets_url")>
    Public Property AssetsUrl As String

    <JsonProperty("upload_url")>
    Public Property UploadUrl As String

    <JsonProperty("html_url")>
    Public Property HtmlUrl As String

    <JsonProperty("id")>
    Public Property Id As Integer

    <JsonProperty("author")>
    Public Property Author As GitHubAuthor

    <JsonProperty("node_id")>
    Public Property NodeId As String

    <JsonProperty("tag_name")>
    Public Property TagName As String

    <JsonProperty("target_commitish")>
    Public Property TargetCommitish As String

    <JsonProperty("name")>
    Public Property Name As String

    <JsonProperty("draft")>
    Public Property Draft As Boolean

    <JsonProperty("prerelease")>
    Public Property Prerelease As Boolean

    <JsonProperty("created_at")>
    Public Property CreatedAt As String

    <JsonProperty("published_at")>
    Public Property PublishedAt As String

    <JsonProperty("assets")>
    Public Property Assets As List(Of GitHubReleaseAsset)

    <JsonProperty("tarball_url")>
    Public Property TarballUrl As String

    <JsonProperty("zipball_url")>
    Public Property ZipballUrl As String

    <JsonProperty("body")>
    Public Property Body As String
End Class

Public Class GitHubAuthor
    <JsonProperty("login")>
    Public Property Login As String

    <JsonProperty("id")>
    Public Property Id As Integer

    <JsonProperty("node_id")>
    Public Property NodeId As String

    <JsonProperty("avatar_url")>
    Public Property AvatarUrl As String

    <JsonProperty("gravatar_id")>
    Public Property GravatarId As String

    <JsonProperty("url")>
    Public Property Url As String

    <JsonProperty("html_url")>
    Public Property HtmlUrl As String

    <JsonProperty("followers_url")>
    Public Property FollowersUrl As String

    <JsonProperty("following_url")>
    Public Property FollowingUrl As String

    <JsonProperty("gists_url")>
    Public Property GistsUrl As String

    <JsonProperty("starred_url")>
    Public Property StarredUrl As String

    <JsonProperty("subscriptions_url")>
    Public Property SubscriptionsUrl As String

    <JsonProperty("organizations_url")>
    Public Property OrganizationsUrl As String

    <JsonProperty("repos_url")>
    Public Property ReposUrl As String

    <JsonProperty("events_url")>
    Public Property EventsUrl As String

    <JsonProperty("received_events_url")>
    Public Property ReceivedEventsUrl As String

    <JsonProperty("type")>
    Public Property Type As String

    <JsonProperty("site_admin")>
    Public Property SiteAdmin As Boolean
End Class

Public Class GitHubReleaseAsset
    <JsonProperty("url")>
    Public Property Url As String

    <JsonProperty("id")>
    Public Property Id As Integer

    <JsonProperty("node_id")>
    Public Property NodeId As String

    <JsonProperty("name")>
    Public Property Name As String

    <JsonProperty("label")>
    Public Property Label As String

    <JsonProperty("uploader")>
    Public Property Uploader As GitHubAuthor

    <JsonProperty("content_type")>
    Public Property ContentType As String

    <JsonProperty("state")>
    Public Property State As String

    <JsonProperty("size")>
    Public Property Size As Integer

    <JsonProperty("download_count")>
    Public Property DownloadCount As Integer

    <JsonProperty("created_at")>
    Public Property CreatedAt As String

    <JsonProperty("updated_at")>
    Public Property UpdatedAt As String

    <JsonProperty("browser_download_url")>
    Public Property BrowserDownloadUrl As String
End Class