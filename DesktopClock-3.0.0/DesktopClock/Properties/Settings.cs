using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Media;
using Newtonsoft.Json;
using WpfWindowPlacement;
using System.Runtime.CompilerServices;
using System.Configuration;
using System.Diagnostics;

namespace DesktopClock.Properties;

public sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase, INotifyPropertyChanged
{
    private readonly FileSystemWatcher _watcher;
    private DateTime _fileDate = DateTime.UtcNow;
    private static readonly Lazy<Settings> _default = new(() => Load() ?? new Settings());
    readonly Config con =new();
    private static readonly JsonSerializerSettings _jsonSerializerSettings = new()
    {
        Formatting = Formatting.Indented
    };


    public string SelectedThemeName
    {
        get => _selectedThemeName;
        set
        {
            _selectedThemeName = value;
            OnPropertyChanged(nameof(SelectedThemeName));
        }
    }

    private string _selectedThemeName;
    private string _selectedThemePrimaryColor;
    private string _selectedThemeSecondaryColor;
    public string SelectedThemePrimaryColor
    {
        get => _selectedThemePrimaryColor;
        set
        {
            _selectedThemePrimaryColor = value;
            OnPropertyChanged(nameof(SelectedThemePrimaryColor));
        }
    }    
    
    public string SelectedThemeSecondaryColor
    {
        get => _selectedThemeSecondaryColor;
        set
        {
            _selectedThemeSecondaryColor = value;
            OnPropertyChanged(nameof(SelectedThemeSecondaryColor));
        }
    }
    private void NotifyPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    
    [UserScopedSetting]
    [DebuggerNonUserCode]
    public double BackgroundOpacity
    {
        get { return (double)(this["BackgroundOpacity"] ?? 1.0); }
        set 
        { 
            this["BackgroundOpacity"] = value;
            NotifyPropertyChanged();
        }
    }
    private void OnPropertyChanged(string propertyName) =>  PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    
    private Settings()
    {
        var exeInfo = new FileInfo(System.Reflection.Assembly.GetEntryAssembly().Location);
        var settingsFileName = Path.GetFileNameWithoutExtension(exeInfo.FullName) + ".settings";
        FilePath = Path.Combine(exeInfo.DirectoryName, settingsFileName);

        // Watch for changes.
        _watcher = new(exeInfo.DirectoryName, settingsFileName)
        {
            EnableRaisingEvents = true
        };
        _watcher.Changed += FileChanged;
        
        Theme =new Theme(con.GetColorName("ColorName"), con.GetTextColor("TextColor"), con.GetOuterColor("OuterColor")); //App.Themes[11];
    }
    public void UpdateTheme(Theme newTheme)
    {
        Theme = newTheme;
        Save(); // 立即保存更改
    }

    public new event PropertyChangedEventHandler PropertyChanged;
    public static Settings Default => _default.Value;

    public static string FilePath { get; private set; }

    #region "Properties"

    public DateTimeOffset CountdownTo { get; set; } = DateTimeOffset.MinValue;
    public string Format { get; set; } = "yyyy-MM-dd dddd HH:mm:ss";
    public string TimeZone { get; set; } = string.Empty;
    public string FontFamily { get; set; } = "Consolas";
    public Color TextColor { get; set; }
    public Color OuterColor { get; set; }
    public bool BackgroundEnabled { get; set; } = true;

    public bool TrayEnabled { get; set; } = true;
    //public double BackgroundOpacity { get; set; } = 1.0;
    public double OutlineThickness { get; set; } = 0;
    
    public bool ShowInTaskbar { get; set; } = true;
    public int Height { get; set; } = int.Parse(new Config().ReadString("Height"));
    public int Opacity { get; set; } = int.Parse(new Config().ReadString("Opacity"));
    public int CornerRadius { get; set; } = 3;
    public double BackgroundCornerRadius { get; set; } = 1;
    public bool RunOnStartup { get; set; } = false;
 
    public WindowPlacement Placement { get; set; }

    [JsonIgnore]
    public Theme Theme
    {
        get => new("Custom", TextColor.ToString(), OuterColor.ToString());
        set
        {
            TextColor = (Color)ColorConverter.ConvertFromString(value.PrimaryColor);
            OuterColor = (Color)ColorConverter.ConvertFromString(value.SecondaryColor);
        }
    }

    #endregion "Properties"

    public bool CheckIfModifiedExternally() => File.GetLastWriteTimeUtc(FilePath) > _fileDate;

  
    #pragma warning disable CS0114 // 成员隐藏继承的成员；缺少关键字 override
    public void Save()
    #pragma warning restore CS0114 // 成员隐藏继承的成员；缺少关键字 override
    {
        using (var fileStream = new FileStream(FilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
        using (var streamWriter = new StreamWriter(fileStream))
        using (var jsonWriter = new JsonTextWriter(streamWriter))
            JsonSerializer.Create(_jsonSerializerSettings).Serialize(jsonWriter, this);

        _fileDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Saves to the default path unless a save has already happened from an external source.
    /// </summary>
    public void SaveIfNotModifiedExternally()
    {
        if (!CheckIfModifiedExternally())
            Save();
    }

    /// <summary>
    /// Populates the given settings with values from the default path.
    /// </summary>
    private static void Populate(Settings settings)
    {
        using var fileStream = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var streamReader = new StreamReader(fileStream);
        using var jsonReader = new JsonTextReader(streamReader);

        JsonSerializer.Create(_jsonSerializerSettings).Populate(jsonReader, settings);
    }

    /// <summary>
    /// Returns loaded settings from the default path or null if it fails.
    /// </summary>
    private static Settings Load()
    {
        try
        {
            var settings = new Settings();
            Populate(settings);
            return settings;
        }
        catch
        {
            return null;
        }
    }

    private void FileChanged(object sender, FileSystemEventArgs e)
    {
        try
        {
            Populate(this);
        }
        catch
        {

        }
    }

    public void Dispose()
    {
        _watcher?.Dispose();
    }
}