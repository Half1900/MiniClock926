using System;
using System.Collections.Generic;
using System.Windows;
using DesktopClock.Properties;
using Microsoft.Win32;
namespace DesktopClock;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    // https://www.materialui.co/colors - A100, A700.
    public static IReadOnlyList<Theme> Themes { get; } =
    [
        new Theme("浅色", "#F5F5F5", "#212121"),
        new Theme("深色", "#212121", "#E2E2E2"),
        new Theme("红色", "#D50000", "#FF8A80"),
        new Theme("粉色", "#C51162", "#FF80AB"),
        new Theme("紫色", "#AA00FF", "#EA80FC"),
        new Theme("蓝色", "#2962FF", "#82B1FF"),
        new Theme("青色", "#00B8D4", "#84FFFF"),
        new Theme("绿色", "#00C853", "#B9F6CA"),
        new Theme("橙色", "#FF6D00", "#FFD180"),

        new Theme("白字粉底", "#FFFFFF", "#FFC2BD"),
        new Theme("白字紫底", "#FFFFFF", "#BB33FF"),
        new Theme("白字蓝底", "#FFFFFF", "#C7DBFF"),
        new Theme("白字绿底", "#FFFFFF", "#C9F8D4"),
        new Theme("白字棕底", "#FFFFFF", "#996633"),
        new Theme("白字蓝灰", "#FFFFFF", "#607D8B"),
        
        new Theme("灰字紫底0", "#455A64", "#F3E5F5"),
        new Theme("白字紫底1", "#FFFFFF", "#E1BEE7"),
        new Theme("白字紫底2", "#FFFFFF", "#CE93D8"),
        new Theme("白字紫底3", "#FFFFFF", "#BA68C8"),
        new Theme("白字紫底4", "#FFFFFF", "#AB47BC"),
        new Theme("白字紫底5", "#FFFFFF", "#9C27B0"),
        new Theme("白字紫底6", "#FFFFFF", "#8E24AA"),
        new Theme("白字紫底7", "#FFFFFF", "#7B1FA2"),
        new Theme("白字紫底8", "#FFFFFF", "#6A1B9A"),

        new Theme("灰字红底0", "#455A64", "#FFEBEE"),
        new Theme("白字红底1", "#FFFFFF", "#FFCDD2"),
        new Theme("白字红底2", "#FFFFFF", "#EF9A9A"),
        new Theme("白字红底3", "#FFFFFF", "#E57373"),
        new Theme("白字红底4", "#FFFFFF", "#EF5350"),
        new Theme("白字红底5", "#FFFFFF", "#F44336"),
        new Theme("白字红底6", "#FFFFFF", "#E53935"),
        new Theme("白字红底7", "#FFFFFF", "#D32F2F"),
        new Theme("白字红底8", "#FFFFFF", "#C62828"),
        new Theme("白字红底9", "#FFFFFF", "#B71C1C"),

        new Theme("灰字青底0", "#455A64", "#E0F2F1"),
        new Theme("白字青底1", "#FFFFFF", "#B2DFDB"),
        new Theme("白字青底2", "#FFFFFF", "#80CBC4"),
        new Theme("白字青底3", "#FFFFFF", "#4DB6AC"),
        new Theme("白字青底4", "#FFFFFF", "#26A69A"),
        new Theme("白字青底5", "#FFFFFF", "#009688"),
        new Theme("白字青底6", "#FFFFFF", "#00897B"),
        new Theme("白字青底7", "#FFFFFF", "#00796B"),
        new Theme("白字青底8", "#FFFFFF", "#00695C"),
        new Theme("白字青底9", "#FFFFFF", "#004D40"),

        new Theme("灰字橙底0", "#455A64", "#FBE9E7"),
        new Theme("白字橙底1", "#FFFFFF", "#FFCCBC"),
        new Theme("白字橙底2", "#FFFFFF", "#FFAB91"),
        new Theme("白字橙底3", "#FFFFFF", "#FF8A65"),
        new Theme("白字橙底4", "#FFFFFF", "#FF7043"),
        new Theme("白字橙底5", "#FFFFFF", "#FF5722"),
        new Theme("白字橙底6", "#FFFFFF", "#F4511E"),
        new Theme("白字橙底7", "#FFFFFF", "#E64A19"),
        new Theme("白字橙底8", "#FFFFFF", "#D84315"),
        new Theme("白字橙底9", "#FFFFFF", "#BF360C"),


        new Theme("灰字绿底0", "#455A64", "#E8F5E9"),
        new Theme("白字绿底1", "#FFFFFF", "#C8E6C9"),
        new Theme("白字绿底2", "#FFFFFF", "#A5D6A7"),
        new Theme("白字绿底3", "#FFFFFF", "#81C784"),
        new Theme("白字绿底4", "#FFFFFF", "#66BB6A"),
        new Theme("白字绿底5", "#FFFFFF", "#4CAF50"),
        new Theme("白字绿底6", "#FFFFFF", "#43A047"),
        new Theme("白字绿底7", "#FFFFFF", "#388E3C"),
        new Theme("白字绿底8", "#FFFFFF", "#2E7D32"),
        new Theme("白字绿底9", "#FFFFFF", "#1B5E20"),

        new Theme("灰字蓝灰0", "#455A64", "#ECEFF1"),
        new Theme("白字蓝灰1", "#FFFFFF", "#CFD8DC"),
        new Theme("白字蓝灰2", "#FFFFFF", "#B0BEC5"),
        new Theme("白字蓝灰3", "#FFFFFF", "#90A4AE"),
        new Theme("白字蓝灰4", "#FFFFFF", "#78909C"),
        new Theme("白字蓝灰5", "#FFFFFF", "#607D8B"),
        new Theme("白字蓝灰6", "#FFFFFF", "#546E7A"),
        new Theme("白字蓝灰7", "#FFFFFF", "#455A64"),
        new Theme("白字蓝灰8", "#FFFFFF", "#37474F"),
        new Theme("白字蓝灰9", "#FFFFFF", "#263238")
    ];

    /// <summary>
    /// Gets the time zone selected in settings, or local by default.
    /// </summary>
    public static TimeZoneInfo GetTimeZone() =>
        DateTimeUtil.TryGetTimeZoneById(Settings.Default.TimeZone, out var timeZoneInfo) ? timeZoneInfo : TimeZoneInfo.Local;

    /// <summary>
    /// Selects a time zone to use.
    /// </summary>
    public static void SetTimeZone(TimeZoneInfo timeZone) =>
        Settings.Default.TimeZone = timeZone.Id;

    /// <summary>
    /// Sets a value in the registry determining whether the current executable should run on system startup.
    /// </summary>
    /// <param name="runOnStartup"></param>
    public static void SetRunOnStartup(bool runOnStartup)
    {
        var exePath = ResourceAssembly.Location;
        var keyName = GetSha256Hash(exePath);
        using var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);

        if (runOnStartup)
            key?.SetValue(keyName, exePath); // Use the path as the name so we can handle multiple exes, but hash it or Windows won't like it.
        else
            key?.DeleteValue(keyName, false);
    }
    protected override void OnExit(ExitEventArgs e)
    {
        Settings.Default.SaveIfNotModifiedExternally();
        base.OnExit(e);
    }
    internal static string GetSha256Hash(string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        using var sha = new System.Security.Cryptography.SHA256Managed();
        var textData = System.Text.Encoding.UTF8.GetBytes(text);
        var hash = sha.ComputeHash(textData);
        return BitConverter.ToString(hash).Replace("-", string.Empty);
    }
}