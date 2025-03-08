Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Windows.Media.Animation
Imports System.Windows.Threading
Imports MaterialDesignThemes.Wpf
Imports MaterialDesignThemes.Wpf.Theme
Imports Microsoft.Win32
Imports Newtonsoft.Json
Class MainWindow
    Public Shared AppPath = AppContext.BaseDirectory()
    'Private hot_value As List(Of Integer) = New List(Of Integer)
    'Private hot_contents As List(Of String) = New List(Of String)
    Private hot_value As Dictionary(Of String, Integer) = New Dictionary(Of String, Integer)
    Private hot_texts As List(Of Controls.TextBox) = New List(Of Controls.TextBox)
    Private hot_s As List(Of TextBlock) = New List(Of TextBlock)

    Private clickCount As Integer = 0
    Private lastClickTime As DateTime
    Private WithEvents saveTimer As DispatcherTimer = New DispatcherTimer() With {
        .Interval = TimeSpan.FromMilliseconds(5000)}
    Private old_hot_text As String
    Dim winLeft As Double
    Dim winTop As Double

    Public TextEdited As Boolean = False

    Dim w2 As New Window2
    Dim w1 As New Window1

    Private ReadOnly paletteHelper As New PaletteHelper()

    Public Sub New()

        ' 此调用是设计器所必需的。
        InitializeComponent()

        ' 在 InitializeComponent() 调用之后添加任何初始化。
        AddHandler SystemEvents.UserPreferenceChanged, AddressOf SystemEvents_UserPreferenceChanged

    End Sub

    Private Sub ResetTimer()
        If saveTimer.IsEnabled Then
            saveTimer.Stop()  ' 如果计时器在运行，先停止
        End If
        saveTimer.Start()  ' 启动计时器
    End Sub

    Async Function read_hot() As Task
        Dim UserSetting = File.ReadAllText(AppPath & "\hot_content.Sr")

        Dim entries = Regex.Matches(UserSetting, "\[(.*?)\]", RegexOptions.Singleline) ' 匹配方括号内的内容

        For Each entry As Match In entries
            Dim rawEntry As String = entry.Value
            If Not String.IsNullOrWhiteSpace(rawEntry) Then
                Console.WriteLine(rawEntry)
                Dim values As List(Of Object) = JsonConvert.DeserializeObject(Of List(Of Object))(rawEntry)
                If values IsNot Nothing AndAlso values.Count >= 2 Then ' 确保反序列化的结果有效
                    hot_value.Add(values(0).ToString(), Convert.ToInt32(values(1)))
                End If
            End If
        Next

        hot_value = hot_value.OrderByDescending(Function(pair) pair.Value).ToDictionary(Function(pair) pair.Key, Function(pair) pair.Value)
        Return
    End Function
    Async Function save_hot() As Task
        saveTimer.Stop()
        DialogHost.Show(LoadingDialog.DialogContent, "Loading")
        Await Task.Delay(750) '缓冲
        Dim hot_database As String = String.Empty
        hot_value = hot_value.OrderByDescending(Function(pair) pair.Value).ToDictionary(Function(pair) pair.Key, Function(pair) pair.Value)
        For Each kvp In hot_value
            hot_database &= $"[""{kvp.Key}"", {kvp.Value}]{Environment.NewLine}"  ' 进行字符串拼接
        Next

        Try
            File.WriteAllText(AppPath & "\hot_content.Sr", hot_database)
        Catch ex As Exception
            MsgBox(ex.Message())
        End Try

        read_hot()
        AddControlsToListBox(10)
        GC.Collect()
        GC.WaitForFullGCComplete()
        DialogHost.Close("Loading")
    End Function
    Private Sub SaveTimer_Tick(sender As Object, e As EventArgs) Handles saveTimer.Tick
        saveTimer.Stop()  ' 停止计时器

        Console.WriteLine("Saved")
        Dim hot_database As String
        For Each kvp In hot_value
            hot_database = hot_database & ($"[""{kvp.Key}"", {kvp.Value}]") & Environment.NewLine()
        Next
        File.WriteAllText(AppPath & "\hot_content.Sr", hot_database)

    End Sub

    Private Sub UpdateWindowStyle()
        Dim wf() As String = File.ReadAllText(Path.Combine(AppPath, "Window.ini")).ToString().Split("*")
        'On Error Resume Next
        'Me.Left = Convert.ToDouble(wf(0))
        'Me.Top = Convert.ToDouble(wf(1))
        'Me.Width = Convert.ToDouble(wf(2))
        'Me.Height = Convert.ToDouble(wf(3))

        ' 获取目标属性值
        Dim targetLeft As Double = Convert.ToDouble(wf(0))
        Dim targetTop As Double = Convert.ToDouble(wf(1))
        Dim targetWidth As Double = Convert.ToDouble(wf(2))
        Dim targetHeight As Double = Convert.ToDouble(wf(3))
        Dim time_duration As Double = 1

        ' 创建并设置动画
        Dim storyboard As New Storyboard()

        ' 动画：改变 Left
        Dim leftAnimation As New DoubleAnimation()
        leftAnimation.From = Me.Left
        leftAnimation.To = targetLeft
        leftAnimation.Duration = TimeSpan.FromSeconds(time_duration)
        leftAnimation.EasingFunction = New CubicEase() With {.EasingMode = EasingMode.EaseOut} ' 设置缓动
        Storyboard.SetTarget(leftAnimation, Me)
        Storyboard.SetTargetProperty(leftAnimation, New PropertyPath("Left"))

        ' 动画：改变 Top
        Dim topAnimation As New DoubleAnimation()
        topAnimation.From = Me.Top
        topAnimation.To = targetTop
        topAnimation.Duration = TimeSpan.FromSeconds(time_duration)
        topAnimation.EasingFunction = New CubicEase() With {.EasingMode = EasingMode.EaseOut} ' 设置缓动
        Storyboard.SetTarget(topAnimation, Me)
        Storyboard.SetTargetProperty(topAnimation, New PropertyPath("Top"))

        ' 动画：改变 Width
        Dim widthAnimation As New DoubleAnimation()
        widthAnimation.From = Me.Width
        widthAnimation.To = targetWidth
        widthAnimation.Duration = TimeSpan.FromSeconds(time_duration)
        widthAnimation.EasingFunction = New CubicEase() With {.EasingMode = EasingMode.EaseOut} ' 设置缓动
        Storyboard.SetTarget(widthAnimation, Me)
        Storyboard.SetTargetProperty(widthAnimation, New PropertyPath("Width"))

        ' 动画：改变 Height
        Dim heightAnimation As New DoubleAnimation()
        heightAnimation.From = Me.Height
        heightAnimation.To = targetHeight
        heightAnimation.Duration = TimeSpan.FromSeconds(time_duration)
        heightAnimation.EasingFunction = New CubicEase() With {.EasingMode = EasingMode.EaseOut} ' 设置缓动
        Storyboard.SetTarget(heightAnimation, Me)
        Storyboard.SetTargetProperty(heightAnimation, New PropertyPath("Height"))

        ' 将所有动画添加到 Storyboard
        storyboard.Children.Add(leftAnimation)
        storyboard.Children.Add(topAnimation)
        storyboard.Children.Add(widthAnimation)
        storyboard.Children.Add(heightAnimation)

        ' 开始动画
        storyboard.FillBehavior = FillBehavior.Stop
        AddHandler storyboard.Completed, Sub()
                                             Me.Left = Convert.ToDouble(wf(0))
                                             Me.Top = Convert.ToDouble(wf(1))
                                             Me.Width = Convert.ToDouble(wf(2))
                                             Me.Height = Convert.ToDouble(wf(3))
                                         End Sub
        storyboard.Begin()
    End Sub

    Private Async Sub MainWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        HotSearchList.Items.Clear()
        LoadingText.Text = "正在加载界面……"
        Me.Dispatcher.BeginInvoke(Sub()
                                      DialogHost.Show(LoadingDialog.DialogContent, "Loading")
                                  End Sub)
        'Await Task.Delay(500)

        AddDialog.Visibility = Visibility.Visible
        LoadingDialog.Visibility = Visibility.Visible
        SingleDialog.Visibility = Visibility.Visible
        MessageDialog.Visibility = Visibility.Visible
        drawerHost.Visibility = Visibility.Visible

        If Not File.Exists(AppPath & "\hot_content.Sr") Then
            File.Create(AppPath & "\hot_content.Sr")
        End If

        Application.Current.Dispatcher.Invoke(Sub()
                                                  If File.Exists(Path.Combine(AppPath, "Window.ini")) Then
                                                      UpdateWindowStyle()
                                                  End If
                                                  'BackImage.Source = New BitmapImage(New Uri(Path.Combine(AppPath, "HotSearch.png")))
                                                  ' HeadImage.Source = New BitmapImage(New Uri(Path.Combine(AppPath, "HotSearchHead.png")))
                                                  Settings.Read()
                                                  ApplySettings()
                                                  BackImage.Visibility = Visibility.Visible
                                                  read_hot()
                                                  AddControlsToListBox(10)
                                              End Sub)

        LoadingText.Text = "正在更新排行榜……"
        'Dim menu As ContextMenu = New ContextMenu
        'Dim refresh As MenuItem = New MenuItem() With {
        '    .Header = New Run() With {
        '    .Text = "关闭",
        '    .FontStretch = FontStretches.Medium,
        '    .FontFamily = New FontFamily("HarmonyOS Sans SC"),
        '    .FontSize = 16},
        '    .Icon = New PackIcon() With {
        '    .Kind = PackIconKind.CloseCircle,
        '    .Width = 16,
        '    .Height = 16}}
        'AddHandler refresh.Click, AddressOf CloseWindow_Click
        'menu.Items.Add(refresh)
        'Me.ContextMenu = menu

        Await Task.Delay(500)
        DialogHost.Close("Loading")
    End Sub

    Private Sub MainWindow_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles Me.MouseLeftButtonDown
        winLeft = Me.Left
        winTop = Me.Top
        If Not MainForm.IsMouseOver Then
            Me.DragMove()
        End If
    End Sub

    Private Sub BackImage_MouseDown(sender As Object, e As MouseButtonEventArgs) Handles BackImage.MouseDown
        clickCount += 1
        If clickCount = 1 Then
            lastClickTime = DateTime.Now
        ElseIf clickCount = 2 Then
            If (DateTime.Now - lastClickTime).TotalMilliseconds <= 500 Then
                ' 检测到双击事件
                If Me.WindowState = WindowState.Maximized Then
                    Me.WindowState = WindowState.Normal
                Else
                    Me.WindowState = WindowState.Maximized
                End If
                clickCount = 0 ' 重置计数器
                Return
            End If
        End If

        ' 重置计数器
        If (DateTime.Now - lastClickTime).TotalMilliseconds > 500 Then
            clickCount = 0
        End If
    End Sub
    Private Sub OnTextEdit(sender As Object, e As RoutedEventArgs)
        Dim textBox As Controls.TextBox = DirectCast(sender, Controls.TextBox)
        textBox.Focus()
        'old_hot_text = textBox.Text()
        textBox.Focus()
        textBox.TextWrapping = TextWrapping.WrapWithOverflow
        textBox.Focus()
    End Sub
    Private Sub InTextEditing()
        TextEdited = True
    End Sub
    Private Async Sub UnTextEdit(sender As Object, e As RoutedEventArgs)
        If Not TextEdited Then
            Return
        End If
        TextEdited = False
        Dim textBox As Controls.TextBox = DirectCast(sender, Controls.TextBox)
        Dim keys As List(Of String) = hot_value.Keys.ToList()
        old_hot_text = keys(Convert.ToInt32(Regex.Match(textBox.Name(), "\d+$").Value())).ToString()

        If old_hot_text <> textBox.Text Then
            If hot_value.ContainsKey(textBox.Text()) Then
                singleText.Text = "排行榜上不能有两个相同的热点。" & vbCrLf & $"""{textBox.Text}"""
                textBox.Text = old_hot_text
                DialogHost.Show(SingleDialog.DialogContent, "single")
            Else
                Dim match As Match = Regex.Match(textBox.Name(), "\d+$")
                Dim indexToChange As Integer = match.Value()
                Dim newKey As String = textBox.Text()

                Dim oldKey As String = Nothing
                Dim i As Integer = 0

                For Each kvp In hot_value
                    If i = indexToChange Then
                        oldKey = kvp.Key
                        Exit For
                    End If
                    i += 1
                Next

                If oldKey IsNot Nothing And hot_value(oldKey) = 0 Then
                    hot_value(newKey) = 0
                    hot_value.Remove(oldKey)

                    Console.WriteLine("ToSave")
                    save_hot()
                Else
                    DialogText.Text = "更改排行榜内容将会清空当前条目的热度， " & vbCrLf & "是否继续？"
                    MessageGood.Content = "更改"
                    MessageBad.Content = "取消"
                    If Settings.settings.NoChangeHot Then
                        Retain.Visibility = Visibility.Visible
                    End If
                    Dim question_to_save_dialog = Await DialogHost.Show(MessageDialog.DialogContent, "Msg")
                    Retain.Visibility = Visibility.Collapsed

                    If question_to_save_dialog IsNot Nothing And question_to_save_dialog.ToString() = "True" Then
                        If Retain.IsChecked Then
                            hot_value(newKey) = hot_value(oldKey)
                            Retain.IsChecked = False
                        Else
                            hot_value(newKey) = 0
                        End If
                        hot_value = hot_value.OrderByDescending(Function(pair) pair.Value).ToDictionary(Function(pair) pair.Key, Function(pair) pair.Value)

                        If oldKey IsNot Nothing Then
                            hot_value.Remove(oldKey)
                        End If
                        hot_value = hot_value.OrderByDescending(Function(pair) pair.Value).ToDictionary(Function(pair) pair.Key, Function(pair) pair.Value)

                        Console.WriteLine("ToSave")
                        save_hot()
                    Else
                        old_hot_text = keys(Convert.ToInt32(Regex.Match(textBox.Name(), "\d+$").Value())).ToString()
                        textBox.Text = old_hot_text
                    End If
                End If

            End If

        End If

        textBox.TextWrapping = TextWrapping.NoWrap
    End Sub

    Private Sub hot(sender As Object, e As MouseButtonEventArgs)
        ResetTimer()
        Dim pi As PackIcon = DirectCast(sender, PackIcon)
        Dim match As Match = Regex.Match(pi.Name(), "\d+$")
        ' 指定索引和新值
        Dim indexToModify As Integer = match.Value() ' 我们要修改的索引
        Console.WriteLine(indexToModify)
        If indexToModify >= 0 AndAlso indexToModify < hot_value.Count Then
            ' 获取键列表
            Dim keys As List(Of String) = hot_value.Keys.ToList()

            ' 根据索引获取对应的键
            Dim keyToModify As String = keys(indexToModify)

            ' 修改对应的值
            hot_value(keyToModify) += 1
            hot_s(match.Value).Text = hot_value(keyToModify).ToString()

            Dim originalKeys As List(Of String) = hot_value.Keys.ToList()
            hot_value = hot_value.OrderByDescending(Function(pair) pair.Value).ToDictionary(Function(pair) pair.Key, Function(pair) pair.Value)
            Dim sortedKeys As List(Of String) = hot_value.Keys.ToList()
            If (Not originalKeys.SequenceEqual(sortedKeys)) = True Then
                save_hot()
            End If

            Dim ptip As New Controls.ToolTip With {
                        .Content = "加热成功 🔥"}
            Controls.ToolTipService.SetToolTip(pi, ptip)
            Controls.ToolTipService.SetShowDuration(pi, 0)
            ptip.PlacementTarget = pi
            ptip.IsOpen = True
            CloseTip(ptip)
        Else
            MsgBox("该项目没有热点。")
        End If


    End Sub

    Private Sub cold(sender As Object, e As MouseButtonEventArgs)
        ResetTimer()
        Dim pi As PackIcon = DirectCast(sender, PackIcon)
        Dim match As Match = Regex.Match(pi.Name(), "\d+$")
        ' 指定索引和新值
        Dim indexToModify As Integer = match.Value() ' 修改的索引
        Console.WriteLine(indexToModify)
        If indexToModify >= 0 AndAlso indexToModify < hot_value.Count Then
            ' 获取键列表
            Dim keys As List(Of String) = hot_value.Keys.ToList()

            ' 根据索引获取对应的键
            Dim keyToModify As String = keys(indexToModify)

            ' 修改对应的值
            If hot_value(keyToModify) = 0 Then
                Dim ptip As New Controls.ToolTip With {
                                           .Content = "再泼冷水就要坏掉了 (o´Д`)"}
                Controls.ToolTipService.SetToolTip(pi, ptip)
                Controls.ToolTipService.SetShowDuration(pi, 0)
                ptip.PlacementTarget = pi
                ptip.IsOpen = True
                CloseTip(ptip)
            Else
                hot_value(keyToModify) -= 1
                hot_s(match.Value).Text = hot_value(keyToModify).ToString()

                Dim originalKeys As List(Of String) = hot_value.Keys.ToList()
                hot_value = hot_value.OrderByDescending(Function(pair) pair.Value).ToDictionary(Function(pair) pair.Key, Function(pair) pair.Value)
                Dim sortedKeys As List(Of String) = hot_value.Keys.ToList()
                If (Not originalKeys.SequenceEqual(sortedKeys)) = True Then
                    save_hot()
                End If

                Dim ptip As New Controls.ToolTip With {
                           .Content = "取消加热 💦"}
                Controls.ToolTipService.SetToolTip(pi, ptip)
                Controls.ToolTipService.SetShowDuration(pi, 0)
                ptip.PlacementTarget = pi
                ptip.IsOpen = True
                CloseTip(ptip)
            End If

        Else
            MsgBox("该项目没有热点。")
        End If


    End Sub

    Private Async Sub CloseTip(ptip As Controls.ToolTip)
        Await Task.Delay(2000)
        ptip.IsOpen = False
    End Sub

    Private Sub AddControlsToListBox(hot_items As Integer)
        Dim index As Integer = 0
        hot_texts.Clear()
        hot_s.Clear()
        HotSearchList.Items.Clear(）

        Dim menu As ContextMenu = New ContextMenu

        Dim refresh As MenuItem = New MenuItem() With {
            .Header = New Run() With {
            .Text = "刷新排行榜",
            .FontStretch = FontStretches.Medium,
            .FontFamily = New FontFamily("HarmonyOS Sans SC"),
            .FontSize = 16},
            .Icon = New PackIcon() With {
            .Kind = PackIconKind.Refresh,
            .Width = 16,
            .Height = 16}}
        AddHandler refresh.Click, Sub()
                                      save_hot()
                                  End Sub

        Dim del As MenuItem = New MenuItem() With {
            .Header = New Run() With {
                .Text = "删除此热点",
                .FontStretch = FontStretches.Medium,
                .FontFamily = New FontFamily("HarmonyOS Sans SC"),
                .FontSize = 16
            },
            .Icon = New PackIcon() With {
                .Kind = PackIconKind.DeleteSweepOutline,
                .Width = 16,
                .Height = 16
            }
        }
        AddHandler del.Click, AddressOf DelHotItem

        menu.Items.Add(refresh)
        menu.Items.Add(del)

        While index <= hot_items
            Console.WriteLine(index)
            Dim grid As New Grid()
            Select Case index
                Case 0
                    grid.Margin = New Thickness(0, 0, 0, 0)
                Case 1
                    grid.Margin = New Thickness(0, -8, 0, 0)
                Case Else
                    grid.Margin = New Thickness(0, -10, 0, 0)
            End Select

            ' 定义列
            grid.ColumnDefinitions.Add(New ColumnDefinition() With {.Width = New GridLength(1, GridUnitType.Auto)}) ' PackIcon 列
            grid.ColumnDefinitions.Add(New ColumnDefinition() With {.Width = New GridLength(1, GridUnitType.Star)}) ' TextBox 列
            grid.ColumnDefinitions.Add(New ColumnDefinition() With {.Width = New GridLength(1, GridUnitType.Auto)}) ' PackIcon 列
            grid.ColumnDefinitions.Add(New ColumnDefinition() With {.Width = New GridLength(1, GridUnitType.Auto)}) ' TextBlock 列

            If index = 0 Then
                Dim ranking As New PackIcon() With {
                            .Name = $"ranking{index}",
                            .Kind = PackIconKind.GasBurner,
                            .Foreground = New SolidColorBrush(Color.FromRgb(255, 0, 0)), ' 使用 RGB 定义颜色
                            .Height = 38,
                            .Width = 38,
                            .Margin = New Thickness(0, 6, 0, 0),
                            .ContextMenu = menu
                        }
                Grid.SetColumn(ranking, 0) ' 设置在第 0 列
                grid.Children.Add(ranking)
            Else
                Dim ranking As New TextBlock() With {
                    .Name = $"ranking{index}",
                    .Text = index.ToString(),
                    .Opacity = 1,
                    .FontFamily = New FontFamily("HarmonyOS Sans SC Black"),
                    .FontSize = 28,
                    .HorizontalAlignment = HorizontalAlignment.Center,
                    .VerticalAlignment = VerticalAlignment.Top,
                    .Padding = New Thickness(10, 0, 11, 0),
                    .Margin = New Thickness(0, 9, 0, 0),
                            .ContextMenu = menu
                }
                Select Case index
                    Case 1
                        ranking.Foreground = New SolidColorBrush(Color.FromRgb(255, 0, 0))
                    Case 2
                        ranking.Foreground = New SolidColorBrush(Color.FromRgb(255, 93, 0))
                    Case 3
                        ranking.Foreground = New SolidColorBrush(Color.FromRgb(255, 175, 0))
                    Case Else
                        ranking.Foreground = New SolidColorBrush(Color.FromRgb(127, 127, 127))
                End Select
                Grid.SetColumn(ranking, 0) ' 设置在第 0 列
                grid.Children.Add(ranking)
            End If

            Dim hotText0 As New Controls.TextBox() With {
                        .Name = $"hotText{index}",
                        .Text = "",
                        .FontSize = 28,
                        .Margin = New Thickness(5, 0, 5, 0),
                        .AcceptsReturn = True,
                        .HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                        .Padding = New Thickness(0, 6, 0, 0)
                    }

            Select Case index
                Case 0
                    hotText0.FontFamily = New FontFamily("HarmonyOS Sans SC Black")
                Case 1
                    hotText0.FontFamily = New FontFamily("HarmonyOS Sans SC Medium")
                Case 2, 3
                    hotText0.FontFamily = New FontFamily("HarmonyOS Sans SC")
                Case Else
                    hotText0.FontFamily = New FontFamily("HarmonyOS Sans SC Light")
            End Select

            AddHandler hotText0.GotFocus, AddressOf OnTextEdit ' 加入事件处理
            AddHandler hotText0.TextChanged, AddressOf InTextEditing ' 加入事件处理
            AddHandler hotText0.LostFocus, AddressOf UnTextEdit ' 加入事件处理
            Grid.SetColumn(hotText0, 1) ' 设置在第 1 列

            ' 创建并添加另一个 PackIcon
            Dim burn0 As New PackIcon() With {
                .Name = $"burn{index}",
                .Background = New SolidColorBrush(Color.FromRgb(255, 255, 255)) With {
            .Opacity = 0.01
        },
                .Kind = PackIconKind.FireCircle,
                .Foreground = New SolidColorBrush(Color.FromRgb(255, 0, 0)),
                .Height = 32,
                .Width = 36,
                .Margin = New Thickness(-2, 2, 0, 0),
                .HorizontalAlignment = HorizontalAlignment.Center,
                .VerticalAlignment = VerticalAlignment.Top,
                .ContextMenu = New ContextMenu()
            }
            AddHandler burn0.MouseLeftButtonDown, AddressOf hot ' 加入事件处理
            AddHandler burn0.MouseRightButtonDown, AddressOf cold ' 加入事件处理
            Grid.SetColumn(burn0, 2) ' 设置在第 2 列

            ' 创建并添加 TextBlock
            Dim hot0 As New Controls.TextBlock() With {
                .Name = $"hot{index}",
                .Text = "0",
                .VerticalAlignment = VerticalAlignment.Top,
                .HorizontalAlignment = HorizontalAlignment.Center,
                .FontSize = 16,
                .FontFamily = New FontFamily("HarmonyOS Sans Medium"),
                .Margin = New Thickness(0, 34, 0, 0)
            }
            If index = 0 Then
                hot0.Foreground = New SolidColorBrush(Color.FromRgb(255, 26, 26))
            End If
            Grid.SetColumn(hot0, 2) ' 设置在第 2 列

            If hot_value.Count() > index Then
                ' 获取键
                Dim keyAtIndex = hot_value.Keys.ToArray()(index)
                Dim valueAtIndex = hot_value(keyAtIndex)
                hotText0.Text = keyAtIndex
                hot0.Text = valueAtIndex.ToString()
            Else
                Exit While
            End If

            ' 将控件添加到 Grid
            Try

                grid.Children.Add(hotText0)
                grid.Children.Add(burn0)
                grid.Children.Add(hot0)

                hot_texts.Add(hotText0)
                hot_s.Add(hot0)
            Catch ex As Exception
                MessageBox.Show($"Error: {ex.Message}")
            End Try

            ' 将 Grid 添加到 ListBox
            HotSearchList.Items.Add(grid)

            If index = 0 Then
                Dim s As New Controls.Separator() With {
                    .Margin = New Thickness(0, -8, 0, 7)
                }
                s.SetResourceReference(Controls.Separator.BackgroundProperty, "MaterialDesign.Brush.Primary.Foreground")
                HotSearchList.Items.Add(s)
            End If

            index += 1
        End While


    End Sub

    Private Sub CloseWindow_Click()
        Dim CloseHotAni As New DoubleAnimation()
        CloseHotAni.From = 1
        CloseHotAni.To = 0
        CloseHotAni.Duration = New Duration(TimeSpan.FromSeconds(0.75))

        AddHandler CloseHotAni.Completed, Sub()
                                              Close()
                                              Environment.Exit(0)
                                          End Sub

        BeginAnimation(window.OpacityProperty, CloseHotAni)
    End Sub

    Private Sub MainWindow_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs) Handles Me.MouseLeftButtonUp
        If (winTop <> Me.Top OrElse winLeft <> Me.Left) And Settings.settings.RecordPosition Then
            winTop = Me.Top
            winLeft = Me.Left
            Dim winfo = $"{Me.Left().ToString()}*{Me.Top().ToString()}*{Me.ActualWidth().ToString()}*{Me.ActualHeight().ToString()}"
            Console.WriteLine("记录窗口位置 in " & Path.Combine(AppPath, "Window.ini").ToString())
            Console.WriteLine(winfo)
            File.WriteAllText(Path.Combine(AppPath, "Window.ini"), winfo)
        End If
    End Sub

    Private Sub MenuToggleButton_OnClick()
        DemoItemsSearchBox.Focus()
    End Sub

    Private Sub UIElement_OnPreviewMouseLeftButtonUp()
        Dim dependencyObject As DependencyObject = TryCast(Mouse.Captured, DependencyObject)

        While dependencyObject IsNot Nothing
            If TypeOf dependencyObject Is System.Windows.Controls.Primitives.ScrollBar Then
                Return
            End If
            dependencyObject = VisualTreeHelper.GetParent(dependencyObject)
        End While

        MenuToggleButton.IsChecked = False
    End Sub

    Private Sub DemoItemsListBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles DemoItemsListBox.SelectionChanged
        DemoItemsListBox.SelectedIndex = -1
    End Sub

    Private Sub CloseHot_Selected(sender As Object, e As RoutedEventArgs) Handles CloseHot.Selected
        CloseWindow_Click()
    End Sub

    Private Sub SystemEvents_UserPreferenceChanged(sender As Object, e As UserPreferenceChangedEventArgs)
        If e.Category = UserPreferenceCategory.General And Settings.settings.AutoDarkMode Then
            ChangePreference(GetSysTheme.GetSysTheme())
        End If
    End Sub

    Private Sub ApplySettings()
        Me.Title = Settings.settings.Title
        HotTitleText.Text = Settings.settings.Title
        If Settings.settings.AutoDarkMode Then
            ChangePreference(GetSysTheme.GetSysTheme())
        End If

        HeadPanel.InvalidateVisual()
        HeadPanel.UpdateLayout()

        Me.Dispatcher.BeginInvoke(Sub()
                                      If HeadPanel.ActualWidth < drawerPanel.MinWidth Then
                                          HeadImage.Width = drawerPanel.MinWidth
                                      Else
                                          HeadImage.Width = HeadPanel.ActualWidth
                                      End If
                                  End Sub)

    End Sub

    Private Async Sub AddHotItem_Selected(sender As Object, e As RoutedEventArgs) Handles AddHotItem.Selected
        If Not MenuToggleButton.IsChecked Then
            Return
        End If
        If hot_value.Count() >= 11 Then
            singleText.Text = "排行榜上最多只能有 11 条热点。" & vbCrLf & "ヽ(゜▽゜　)"
            DialogHost.Show(SingleDialog.DialogContent, "single")
        Else
            AddTextTip.Text = "添加新的热点"
            Dim YesOrNo = Await DialogHost.Show(AddDialog.DialogContent, "Add")
            If YesOrNo Then
                If hot_value.ContainsKey(NewHot.Text()) Then
                    singleText.Text = "排行榜上不能有两个相同的热点。" & vbCrLf & $"""{NewHot.Text()}"""
                    DialogHost.Show(SingleDialog.DialogContent, "single")
                Else
                    hot_value.Add(NewHot.Text(), 0)
                    save_hot()
                    NewHot.Text = ""
                End If
            End If
        End If
    End Sub

    Private Async Sub DelHotItem(sender As Object, e As RoutedEventArgs)

        ' 获取当前点击的 MenuItem
        Dim menuItem As MenuItem = CType(sender, MenuItem)

        ' 获取关联的 ContextMenu
        Dim contextMenu As ContextMenu = CType(menuItem.Parent, ContextMenu)

        ' 通过 ContextMenu 的 PlacementTarget 获取 PackIcon
        Dim packIcon
        Try
            packIcon = CType(contextMenu.PlacementTarget, PackIcon)
        Catch ex As Exception
            packIcon = CType(contextMenu.PlacementTarget, TextBlock)
        End Try

        If packIcon IsNot Nothing Then
            ' 获取 PackIcon 的 Name
            Dim packIconName As String = packIcon.Name

            Dim keys As List(Of String) = hot_value.Keys.ToList()
            Dim indexToDel As Int32 = Convert.ToInt32(Regex.Match(packIconName, "\d+$").Value())
            old_hot_text = keys(indexToDel).ToString()

            DialogText.Text = "确实要删除这个热点？ " & vbCrLf & $"""{old_hot_text}"""
            MessageGood.Content = "确认"
            MessageBad.Content = "取消"

            Dim ConfirmToDel = Await DialogHost.Show(MessageDialog.DialogContent, "Msg")

            If ConfirmToDel IsNot Nothing And ConfirmToDel.ToString() = "True" Then
                Dim keyToDel As String = keys(indexToDel)

                hot_value.Remove(keyToDel)

                save_hot()
            End If
        End If
    End Sub

    Private Sub AboutHot_Selected(sender As Object, e As RoutedEventArgs) Handles AboutHot.Selected
        If w1.IsOpen Then
            w1.Activate()
        Else
            Try
                w1.Show()
            Catch ex As Exception
                w1 = New Window1
                w1.Show()
            End Try

        End If
    End Sub

    Private Sub HotSettingsButton_Selected(sender As Object, e As RoutedEventArgs) Handles HotSettingsButton.Selected
        If w2.IsOpen Then
            w2.Activate()
        Else
            Try
                AddHandler w2.WindowClosed, AddressOf ApplySettings
                w2.Show()
            Catch ex As Exception
                w2 = New Window2
                AddHandler w2.WindowClosed, AddressOf ApplySettings
                w2.Show()
            End Try

        End If

    End Sub

    Private Sub DemoItemsListBox_LostMouseCapture(sender As Object, e As MouseEventArgs) Handles DemoItemsListBox.LostMouseCapture

    End Sub

    Private Sub DarkModeButton_Click(sender As Object, e As RoutedEventArgs) Handles DarkModeButton.Click
        Dim theme = paletteHelper.GetTheme()
        If theme.GetBaseTheme() = BaseTheme.Dark Then
            ChangePreference("Light")
        Else
            ChangePreference("Dark")
        End If
    End Sub

    Private Sub ChangePreference(mode As String)
        Dim theme = paletteHelper.GetTheme()
        Select Case mode
            Case "Light"
                DarkModeButtonSign.Kind = PackIconKind.Lightbulb
                theme.SetBaseTheme(BaseTheme.Light)
                Application.Current.Resources("MaterialDesign.Brush.Primary.Foreground") = Application.Current.Resources("MaterialDesign.Brush.Primary.Light.Foreground")
                Application.Current.Resources("MaterialDesign.Brush.Primary.Background") = Application.Current.Resources("MaterialDesign.Brush.Primary.Dark.Foreground")
                Application.Current.Resources("MaterialDesign.Brush.Company") = Application.Current.Resources("MaterialDesign.Brush.Company.Light")
                CurrentEvents.RaiseThemeChangedEvent("Light")

                If Settings.settings.BackgroundLight <> "Normal" And Directory.Exists(Path.Combine(AppPath, "resource")) And File.Exists(Path.Combine(AppPath, "resource/HotSearch.png")) Then
                    BackImage.Source = LoadImageToMEM.LoadImageFromPath(Settings.settings.BackgroundLight)
                Else
                    BackImage.Source = New BitmapImage(New Uri("pack://application:,,,/HotSearch.png"))
                End If

                If Settings.settings.HeadImageLight <> "Normal" And Directory.Exists(Path.Combine(AppPath, "resource")) And File.Exists(Path.Combine(AppPath, "resource/HotSearchHead.png")) Then
                    HeadImage.Source = LoadImageToMEM.LoadImageFromPath(Settings.settings.HeadImageLight)
                Else
                    HeadImage.Source = New BitmapImage(New Uri("pack://application:,,,/HotSearchHead.png"))
                End If

            Case "Dark"
                DarkModeButtonSign.Kind = PackIconKind.LightbulbOutline
                theme.SetBaseTheme(BaseTheme.Dark)
                Application.Current.Resources("MaterialDesign.Brush.Primary.Background") = Application.Current.Resources("MaterialDesign.Brush.Secondary.Foreground")
                Application.Current.Resources("MaterialDesign.Brush.Primary.Foreground") = Application.Current.Resources("MaterialDesign.Brush.Primary.Dark.Foreground")
                Application.Current.Resources("MaterialDesign.Brush.Company") = Application.Current.Resources("MaterialDesign.Brush.Company.Dark")
                CurrentEvents.RaiseThemeChangedEvent("Dark")

                If Settings.settings.BackgroundDark <> "Normal" And Directory.Exists(Path.Combine(AppPath, "resource")) And File.Exists(Path.Combine(AppPath, "resource/HotSearch.Dark.png")) Then
                    BackImage.Source = LoadImageToMEM.LoadImageFromPath(Settings.settings.BackgroundDark)
                Else
                    BackImage.Source = New BitmapImage(New Uri("pack://application:,,,/HotSearch.Dark.png"))
                End If

                If Settings.settings.HeadImageDark <> "Normal" And Directory.Exists(Path.Combine(AppPath, "resource")) And File.Exists(Path.Combine(AppPath, "resource/HotSearchHead.Dark.png")) Then
                    HeadImage.Source = LoadImageToMEM.LoadImageFromPath(Settings.settings.HeadImageDark)
                Else
                    HeadImage.Source = New BitmapImage(New Uri("pack://application:,,,/HotSearchHead.Dark.png"))
                End If

        End Select
        paletteHelper.SetTheme(Theme)
    End Sub
End Class
