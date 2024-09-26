using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopClock.Properties;
using Humanizer;
using System.Windows.Media;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using Clipboard = System.Windows.Forms.Clipboard;
using MessageBox = System.Windows.MessageBox;
using ContextMenu = System.Windows.Forms.ContextMenu;
using MenuItem = System.Windows.Forms.MenuItem;
using System.Threading;
using System.Windows.Threading;
using System.Text;

namespace DesktopClock;

[ObservableObject]
public partial class MainWindow : Window
{
    private readonly SystemClockTimer _systemClockTimer;
    private TimeZoneInfo _timeZone;
    NotifyIcon notifyIcon = new();
    private static readonly Guid _instanceId = Guid.NewGuid();
    private static readonly Mutex _mutex = new(true, $"DesktopClock_{_instanceId}");
    readonly Config con = new();
    public bool DragEnabled { get; set; } = true;
    public bool TopmostEnabled { get; set; } = true;

    readonly static string configPath = Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName), "config.ini");

    readonly Config config = new(configPath);

    public MainWindow()
    {
        try
        {
            var processes = Process.GetProcesses();

            var currentCount = 0;

            foreach (var item in processes)
            {
                if (item.ProcessName == Process.GetCurrentProcess().ProcessName)
                {
                    currentCount++;
                }
            }

            if (currentCount > 1)
            {
                Environment.Exit(1);
                return;
            }
            EnsureSingleInstance();
            InitializeComponent();
            InitialTray();
            Show();
            Loaded += MainWindow_Loaded;
            notifyIcon.Visible = true;
            DataContext = this;
            LoadConfig();
            ApplyConfigChange();
            //SetClockViewboxCornerRadius(new CornerRadius(10));
            _systemClockTimer = new();
            _systemClockTimer.SecondChanged += SystemClockTimer_SecondChanged;
            _systemClockTimer.Start();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"报错: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    private void SetClockViewboxCornerRadius(CornerRadius cornerRadius)
    {
        if (ClockViewbox.Child is Border border)
        {
            border.CornerRadius = cornerRadius;
        }
    }

    private void LoadConfig()
    {
        check_auto_start.IsChecked = con.ReadBool("auto_starts") == 1;
        ToggleAutoStart();

        check_show_task.IsChecked = con.ReadBool("show_task") == 1;
        ShowInTaskbar = check_show_task.IsChecked;

        check_tray.IsChecked = con.ReadBool("show_tray") == 1;
        notifyIcon.Visible = check_tray.IsChecked;

        Settings.Default.RunOnStartup = check_auto_start.IsChecked;
        App.SetRunOnStartup(Settings.Default.RunOnStartup);

        DragEnabled = con.ReadBool("drag_move") == 1;
        TopmostEnabled = con.ReadBool("top_most") == 1;
        Topmost = TopmostEnabled;

        if (!config.IsExistName("drag_move"))
            con.Write("drag_move", true);

        SetWindowPosition();
        SetThemes();

        _timeZone = App.GetTimeZone() ?? TimeZoneInfo.Local;
        Settings.Default.PropertyChanged += Settings_PropertyChanged;

        var configHeight = int.Parse(con.ReadString("Height"));
        var configOpacity = int.Parse(con.ReadString("Opacity"));

        Settings.Default.Height = configHeight;
        Settings.Default.Opacity = configOpacity;

        ApplyHeight(configHeight);
        ApplyOpacity(configOpacity);
    }
    private void SetThemes()
    {
        var (Name, PrimaryColor, SecondaryColor) = Theme.GetThemesByName(con.GetColorName("ColorName"));
        var theme = new Theme(Name, PrimaryColor, SecondaryColor);
        Settings.Default.Theme = SetThemeFormat(theme);
        SaveSelectedTheme(Settings.Default.Theme);
        SetTheme(theme);
    }
    private void ToggleAutoStart()
    {
        if (check_auto_start.IsChecked)
            con.Add_auto_start();
        else
            con.Del_auto_start();
    }

    private void SetWindowPosition()
    {
        if (!config.IsExistName("Top") || !config.IsExistName("Left"))
        {
            var screenWidth = SystemParameters.PrimaryScreenWidth;
            var screenHeight = SystemParameters.PrimaryScreenHeight;
            var windowWidth = Width;
            var windowHeight = Height;

            Left = (screenWidth - windowWidth) / 2; // 居中
            Top = (screenHeight - windowHeight) / 2;
        }
        else
        {
            Left = double.Parse(con.ReadBoundleString("Left"));
            Top = double.Parse(con.ReadBoundleString("Top"));
        }
        con.WriteKeyValue("Left", Left.ToString());
        con.WriteKeyValue("Top", Top.ToString());
    }
    private void ApplyConfigChange()
    {
        check_auto_start.IsChecked = con.ReadBool("auto_starts") == 1;
        check_show_task.IsChecked = con.ReadBool("show_task") == 1;
        check_tray.IsChecked = con.ReadBool("show_tray") == 1;
        ShowInTaskbar = check_show_task.IsChecked;
        notifyIcon.Visible = check_tray.IsChecked;

        var theme = new Theme(con.GetColorName("ColorName"), con.GetTextColor("TextColor"), con.GetOuterColor("OuterColor"));
        SetTheme(theme);

        ApplyHeight(int.Parse(con.ReadString("Height")));
        ApplyOpacity(int.Parse(con.ReadString("Opacity")));
    }
    private void OnConfigChanged(object sender, FileSystemEventArgs e)
    {
        // 重新加载配置
        Dispatcher.Invoke(() =>
        {
            try
            {
                //config.Reload();
                ApplyConfigChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to reload config: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        });
    }
    private void ApplyConfigChanges()
    {
        // 应用配置变化
        check_auto_start.IsChecked = con.ReadBool("auto_starts") == 1;
        check_show_task.IsChecked = con.ReadBool("show_task") == 1;
        check_tray.IsChecked = con.ReadBool("show_tray") == 1;
        ShowInTaskbar = check_show_task.IsChecked;
        notifyIcon.Visible = check_tray.IsChecked;

        var theme = new Theme(con.GetColorName("ColorName"), con.GetTextColor("TextColor"), con.GetOuterColor("OuterColor"));
        SetTheme(theme);

        ApplyHeight(int.Parse(con.ReadString("Height")));
        ApplyOpacity(int.Parse(con.ReadString("Opacity")));
    }
    private void EnsureSingleInstance()
    {
        if (!_mutex.WaitOne(TimeSpan.Zero, true))
            Environment.Exit(1);
    }
    private void ApplyHeight(int height)
    {
        Height = height;
        Settings.Default.Height = height;
        OnPropertyChanged(nameof(Height));
    }
    private void ApplyOpacity(int opacity)
    {
        Opacity = opacity;   
        Settings.Default.Opacity = opacity;
        OnPropertyChanged(nameof(Opacity));
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        Dispatcher.BeginInvoke(new Action(() =>
        {
            ClockBorder.Visibility = Visibility.Visible;
        }), DispatcherPriority.Loaded);
    }
    private void Check_Click(object sender, RoutedEventArgs e)
    {
        var auto_starts = (bool)check_auto_start.IsChecked;
        con.Write("auto_starts", auto_starts);
        if (auto_starts)
            con.Add_auto_start();
        else
            con.Del_auto_start();
    }
    private void Drag_Click(object sender, RoutedEventArgs e)
    {
        var dragEnabled = (bool)drag_move.IsChecked;

        con.Write("drag_move", dragEnabled);

    }
    private void Top_Most_Click(object sender, RoutedEventArgs e)
    {
        TopmostEnabled = (bool)top_most.IsChecked;
        Topmost = TopmostEnabled;
        con.Write("top_most", TopmostEnabled);
    }

    private void BackgroundOpacity_Click(object sender, RoutedEventArgs e)
    {

        Settings.Default.BackgroundOpacity = Settings.Default.BackgroundEnabled ? 1.0 : 0.0;
        con.Write("BackgroundEnabled", Settings.Default.BackgroundEnabled);
        con.WriteKeyValue("BackgroundOpacity", Settings.Default.BackgroundOpacity.ToString());

        var (Name, PrimaryColor, SecondaryColor) = Theme.GetThemesByName(con.GetColorName("ColorName"));
        var theme = new Theme(Name, PrimaryColor, SecondaryColor);
        Settings.Default.Theme = theme;
        SaveSelectedTheme(Settings.Default.Theme);
        SetTheme(theme);
        Settings.Default.Save();

    }
    private void Task_Click(object sender, RoutedEventArgs e)
    {
        var task_bar = (bool)check_show_task.IsChecked;
        ShowInTaskbar = task_bar;
        con.Write("show_task", task_bar);
    }

    private void Tray_Click(object sender, RoutedEventArgs e)
    {
        var tray = (bool)check_tray.IsChecked;
        notifyIcon.Visible = tray;
        con.Write("show_tray", tray);
    }
    public void InitialTray()
    {
        Visibility = Visibility.Visible;
        notifyIcon = new NotifyIcon {
            Text = "桌面时间显示",
            Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath),
            Visible = true
        };
        //notifyIcon.Text = "桌面时间显示";
        notifyIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath);

        // 添加右键菜单
        var contextMenu = new ContextMenu();
        var showItem = new MenuItem("显示");
        var hideItem = new MenuItem("隐藏");
        var exit = new MenuItem("退出");

        showItem.Click += Show_Click;
        hideItem.Click += Hide_Click;
        exit.Click += Exit_Click;

        contextMenu.MenuItems.Add(showItem);
        contextMenu.MenuItems.Add(hideItem);
        contextMenu.MenuItems.Add(exit);
        notifyIcon.ContextMenu = contextMenu;

        notifyIcon.DoubleClick += NotifyIcon_DoubleClick;
    }
    private void NotifyIcon_DoubleClick(object sender, EventArgs e)
    {

        if (IsVisible)
        {
            Hide();
            WindowState = WindowState.Minimized;
            
        }
        else
        {
            WindowState = WindowState.Normal;
            Show();
            Activate();
            Left = double.Parse(con.ReadBoundleString("Left"));
            Top = double.Parse(con.ReadBoundleString("Top"));
            Topmost = con.ReadBool("top_most") == 1;

        }
    }
    void Exit_Click(object sender, EventArgs e)
    {
        notifyIcon.Visible = false;
        Close();
    }
    void Show_Click(object sender, EventArgs e)
    {
        Show();
        WindowState = WindowState.Normal;
        Activate();
    }
    void Hide_Click(object sender, EventArgs e)
    {
        Hide();
        WindowState = WindowState.Minimized;
    }

    private DateTimeOffset CurrentTimeInSelectedTimeZone => TimeZoneInfo.ConvertTime(DateTimeOffset.Now, _timeZone);


    private bool IsCountdown => Settings.Default.CountdownTo > DateTimeOffset.MinValue;


    public string CurrentTimeOrCountdownString =>
        IsCountdown ?
        Settings.Default.CountdownTo.Humanize(CurrentTimeInSelectedTimeZone) :
        CurrentTimeInSelectedTimeZone.ToString(Settings.Default.Format);

    [RelayCommand]
    public void CopyToClipboard() => Clipboard.SetText(TimeTextBlock.Text);

    [RelayCommand]
    public void SetTheme(Theme theme)
    {
        Settings.Default.Theme = SetThemeFormat(theme);
        Settings.Default.CornerRadius = 3;
        SaveSelectedTheme(Settings.Default.Theme);
        Settings.Default.Save();
        //Debug.WriteLine("00---"+ SetThemeFormat(theme));
        ApplyTheme(SetThemeFormat(theme), con.ReadBool("BackgroundEnabled") == 1 ? 1 : 0);
    }

    [RelayCommand]
    public void SetFormat(string format) => Settings.Default.Format = format;

    [RelayCommand]
    public void SetTimeZone(TimeZoneInfo tzi) => App.SetTimeZone(tzi);

    [DllImport("shell32.dll", CharSet = CharSet.Auto)]
    private static extern bool SHGetSpecialFolderPath(IntPtr hwndOwner, [Out] StringBuilder lpszPath, int nFolder, bool fCreate);
    private const int CSIDL_DESKTOP = 0x0000; // 桌面文件夹
    public static string GetDesktopPath()
    {
        var path = new StringBuilder(260);
        SHGetSpecialFolderPath(IntPtr.Zero, path, CSIDL_DESKTOP, false);
        return path.ToString();
    }

    [RelayCommand]
    public void NewClock()
    {
        var result = MessageBox.Show(this,
            $"该操作将复制当前的可执行文件，并使用新设置启动它。\n\n" +
            $"是否继续？",
            Title, MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.OK);

        if (result != MessageBoxResult.OK)
            return;

        var exeInfo = new FileInfo(System.Reflection.Assembly.GetEntryAssembly().Location);
        var newExePath = Path.Combine(exeInfo.DirectoryName, Guid.NewGuid().ToString() + exeInfo.Name);
        File.Copy(exeInfo.FullName, newExePath);
        Process.Start(newExePath);
    }
    [RelayCommand]
    public void CountdownTo()
    {
        var result = MessageBox.Show(this,
            $"In advanced settings: change {nameof(Settings.Default.CountdownTo)}, then save.\n" +
            "Go back by replacing it with \"0001-01-01T00:00:00+00:00\".\n\n" +
            "Open advanced settings now?",
            Title, MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.OK);

        if (result == MessageBoxResult.OK)
            OpenSettings();
    }

    [RelayCommand]
    public void OpenSettings()
    {
        Settings.Default.Save();

        var exeInfo = new FileInfo(System.Reflection.Assembly.GetEntryAssembly().Location);
        var settingsFileName = "config.ini";
        var FilePath = Path.Combine(exeInfo.DirectoryName, settingsFileName);

        if (!File.Exists(FilePath))
            Settings.Default.Save();
        try
        {
            Process.Start("notepad", FilePath);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this,
                "无法打开文件\n\n" +
                "可能是文件不存在或者没有足够的权限\n\n" +
                $"当前报错: \"{ex.Message}\"");
        }
    }

    [RelayCommand]
    public void MiniWindow() {
        Hide();
        notifyIcon.Visible = true;
    }

    [RelayCommand]
    public void CheckForUpdates() => Process.Start("https://github.com/danielchalmers/DesktopClock/releases");


    [RelayCommand]
    public void Exit() => Close();

    private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(Settings.Default.TimeZone):
                _timeZone = App.GetTimeZone() ?? TimeZoneInfo.Local;
                UpdateTimeString();
                break;

            case nameof(Settings.Default.Format):
                UpdateTimeString();
                break;
        }
    }

    private void SystemClockTimer_SecondChanged(object sender, EventArgs e) => UpdateTimeString();

    private void UpdateTimeString() => OnPropertyChanged(nameof(CurrentTimeOrCountdownString));

    private void Window_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            if (con.ReadBool("drag_move") == 1)
                DragMove();
            // 确保窗口边界不超过可视桌面
            var workingArea = SystemParameters.WorkArea;
            Left = Math.Max(workingArea.Left, Math.Min(workingArea.Right - Width, Left));
            Top = Math.Max(workingArea.Top, Math.Min(workingArea.Bottom - Height, Top));
            con.WriteKeyValue("Left", Left.ToString());
            con.WriteKeyValue("Top", Top.ToString());
        }
    }
    private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e) => CopyToClipboard();

    private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (Keyboard.Modifiers == ModifierKeys.Control)
        {
            var steps = e.Delta / (double)Mouse.MouseWheelDeltaForOneLine;
            var change = Settings.Default.Height * steps * 0.15;
            Settings.Default.Height = (int)Math.Min(Math.Max(Settings.Default.Height + change, 8), 300);
            con.WriteKeyValue("Height", Settings.Default.Height.ToString());
        }
    }

    private void Slider_SizeChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (sender is Slider slider && IsLoaded)
        {
            var newHeight = (int)slider.Value;
            ApplyHeight(newHeight);
            con.WriteKeyValue("Height", newHeight.ToString());
        }
    }

    private void Slider_OpacityChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (sender is Slider slider && IsLoaded)
        {
            var newOpacity = (int)slider.Value;
            ApplyOpacity(newOpacity);
            con.WriteKeyValue("Opacity", newOpacity.ToString());

            var (Name, PrimaryColor, SecondaryColor) = Theme.GetThemesByName(con.GetColorName("ColorName"));
            var (_, color) = SplitHexColor(SecondaryColor);
            var theme = new Theme(Name, PrimaryColor, color);
            Settings.Default.Theme = SetThemeFormat(theme);
            ApplyTheme(SetThemeFormat(theme), con.ReadBool("BackgroundEnabled") == 1 ? 1 : 0);

        }
    }
    private string ConvertOpacityToHex(int opacity)
    {
        // 将0-100的透明度值映射到0-255
        //byte opacityByte = (byte)(opacity * 2.55);
        // 将0-255的值转换为十六进制字符串
        return opacity.ToString("X2");
    }
    private Color CombineOpacityWithColor(string hexOpacity, string colorHex)
    {
        if (!colorHex.StartsWith("#"))// 确保颜色值以 "#" 开头
            colorHex = "#" + colorHex;
        var alpha = Convert.ToByte(hexOpacity, 16);// 将十六进制透明度值转换为 byte
        var color = (Color)ColorConverter.ConvertFromString(colorHex);// 使用 ColorConverter 将十六进制颜色值转换为 Color
        return Color.FromArgb(alpha, color.R, color.G, color.B); // 使用 FromArgb 方法结合透明度和颜色
    }
    private void ApplyTheme(Theme theme, int tag)
    {
        con.WriteKeyValue("ColorName", SetThemeFormat(theme).Name);
        con.WriteKeyValue("TextColor", SetThemeFormat(theme).PrimaryColor);
        con.WriteKeyValue("OuterColor", SetThemeFormat(theme).SecondaryColor);
        Settings.Default.TextColor = (Color)ColorConverter.ConvertFromString(con.ReadString("TextColor"));
        Settings.Default.CornerRadius=3;
        //Debug.WriteLine("0---"+ SetThemeFormat(theme).SecondaryColor);
        Foreground = con.ReadString("Opacity") == "0" ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFFFF")) : new SolidColorBrush((Color)ColorConverter.ConvertFromString(SetThemeFormat(theme).PrimaryColor));
        if (FindName("ClockBorder") is Border clockViewbox)
        {
            clockViewbox.Background = tag == 1
                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString(SetThemeFormat(theme).SecondaryColor))
                : new SolidColorBrush(Colors.Transparent);
        }
    }

    private void SaveSelectedTheme(Theme theme)
    {
        // 使用配置文件或其他持久化方法保存主题信息
        Settings.Default.SelectedThemeName = SetThemeFormat(theme).Name;
        Settings.Default.SelectedThemePrimaryColor = SetThemeFormat(theme).PrimaryColor;
        Settings.Default.SelectedThemeSecondaryColor = SetThemeFormat(theme).SecondaryColor;
        con.WriteKeyValue("ColorName", Settings.Default.SelectedThemeName);
        con.WriteKeyValue("TextColor", Settings.Default.SelectedThemePrimaryColor);
        con.WriteKeyValue("OuterColor", Settings.Default.SelectedThemeSecondaryColor);
        Settings.Default.Save();
        //Debug.WriteLine("1---"+ SetThemeFormat(theme).SecondaryColor);
        Foreground = con.ReadString("Opacity") == "0" ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFFFF")) : new SolidColorBrush((Color)ColorConverter.ConvertFromString(SetThemeFormat(theme).PrimaryColor));
        //Background = con.ReadBool("BackgroundEnabled") == 1  ? new SolidColorBrush((Color)ColorConverter.ConvertFromString(SetThemeFormat(theme).SecondaryColor)) : new SolidColorBrush(Colors.Transparent);
        if (FindName("ClockBorder") is Border clockViewbox)
        {
            // 设置背景颜色
            clockViewbox.Background = con.ReadBool("BackgroundEnabled") == 1
                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString(SetThemeFormat(theme).SecondaryColor))
                : new SolidColorBrush(Colors.Transparent);
        }


    }
    private Theme SetThemeFormat(Theme theme)
    {
        var opacity = con.ReadString("Opacity");       
        var hexOpacity = ConvertOpacityToHex(int.Parse(opacity));

        var name = theme.Name;
        var (_, primaryColor) = SplitHexColor(theme.PrimaryColor);
        var (opacitys, secondaryColor)= SplitHexColor(theme.SecondaryColor);

        var combinedPrimaryColor   = CombineOpacityWithColor("FF", primaryColor);
        var combinedSecondaryColor = CombineOpacityWithColor(opacitys=="-1" ? hexOpacity:opacitys, secondaryColor);
        
        var combinedPrimaryColorString = combinedPrimaryColor.ToString();
        var combinedSecondaryColorString = combinedSecondaryColor.ToString();

        var themes = new Theme(name, combinedPrimaryColorString, combinedSecondaryColorString);
        return themes;
    }

    public static (string opacity, string color) SplitHexColor(string hexColor)
    {
        //if (!hexColor.StartsWith("#") || (hexColor.Length != 7 && hexColor.Length != 9))
        //throw new ArgumentException("无效的十六进制颜色格式");
        if (hexColor.Length == 7)
            return ("-1", hexColor.Substring(1));
        else if (hexColor.Length == 6)
            return ("-1",hexColor.Substring(0));
        else
            return (hexColor.Substring(1, 2), hexColor.Substring(3));
    }


}