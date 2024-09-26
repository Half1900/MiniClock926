using System;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
namespace DesktopClock;

public readonly struct Theme
{
    public string Name { get; }
    public string PrimaryColor { get; }
    public string SecondaryColor { get; }

    public Theme(string name, string primaryColor, string secondaryColor)
    {
        Name = name;
        PrimaryColor = primaryColor;
        SecondaryColor = secondaryColor;
    }

    public static implicit operator Theme(string v) => throw new NotImplementedException();

    public static (string name,string PrimaryColor, string SecondaryColor) GetThemesByName(string name)
    {
        var json = GetThemesJson();
        var themes = JObject.Parse(json)["Themes"].ToObject<List<Theme>>();       
        var theme = themes.FirstOrDefault(t => t.Name == name);
        return (theme.Name,theme.PrimaryColor, theme.SecondaryColor);
    }
    public static string GetThemesJson()
    {
        return @"
        {
        ""Themes"": [
            { ""Name"": ""Custom"", ""PrimaryColor"": ""#FFFFFF"", ""SecondaryColor"": ""#AB47BC"" },
            { ""Name"": ""浅色"", ""PrimaryColor"": ""#F5F5F5"", ""SecondaryColor"": ""#212121"" },
            { ""Name"": ""深色"", ""PrimaryColor"": ""#212121"", ""SecondaryColor"": ""#E2E2E2"" },
            { ""Name"": ""红色"", ""PrimaryColor"": ""#D50000"", ""SecondaryColor"": ""#FF8A80"" },
            { ""Name"": ""粉色"", ""PrimaryColor"": ""#C51162"", ""SecondaryColor"": ""#FF80AB"" },
            { ""Name"": ""紫色"", ""PrimaryColor"": ""#AA00FF"", ""SecondaryColor"": ""#EA80FC"" },
            { ""Name"": ""蓝色"", ""PrimaryColor"": ""#2962FF"", ""SecondaryColor"": ""#82B1FF"" },
            { ""Name"": ""青色"", ""PrimaryColor"": ""#00B8D4"", ""SecondaryColor"": ""#84FFFF"" },
            { ""Name"": ""绿色"", ""PrimaryColor"": ""#00C853"", ""SecondaryColor"": ""#B9F6CA"" },
            { ""Name"": ""橙色"", ""PrimaryColor"": ""#FF6D00"", ""SecondaryColor"": ""#FFD180"" },
            { ""Name"": ""白字粉底"", ""PrimaryColor"": ""#FFFFFF"", ""SecondaryColor"": ""#FFC2BD"" },
            { ""Name"": ""白字紫底"", ""PrimaryColor"": ""#FFFFFF"", ""SecondaryColor"": ""#BB33FF"" },
            { ""Name"": ""白字蓝底"", ""PrimaryColor"": ""#FFFFFF"", ""SecondaryColor"": ""#C7DBFF"" },
            { ""Name"": ""白字绿底"", ""PrimaryColor"": ""#FFFFFF"", ""SecondaryColor"": ""#C9F8D4"" },
            { ""Name"": ""白字棕底"", ""PrimaryColor"": ""#FFFFFF"", ""SecondaryColor"": ""#996633"" },
            { ""Name"": ""白字蓝灰"", ""PrimaryColor"": ""#FFFFFF"", ""SecondaryColor"": ""#607D8B"" },
            { ""Name"": ""灰字紫底0"", ""PrimaryColor"": ""#455A64"", ""SecondaryColor"": ""#F3E5F5"" },
            { ""Name"": ""白字紫底1"", ""PrimaryColor"": ""#FFFFFF"", ""SecondaryColor"": ""#E1BEE7"" },
            { ""Name"": ""白字紫底2"", ""PrimaryColor"": ""#FFFFFF"", ""SecondaryColor"": ""#CE93D8"" },
            { ""Name"": ""白字紫底3"", ""PrimaryColor"": ""#FFFFFF"", ""SecondaryColor"": ""#BA68C8"" },
            { ""Name"": ""白字紫底4"", ""PrimaryColor"": ""#FFFFFF"", ""SecondaryColor"": ""#AB47BC"" },
            { ""Name"": ""白字紫底5"", ""PrimaryColor"": ""#FFFFFF"", ""SecondaryColor"": ""#9C27B0"" },
            { ""Name"": ""白字紫底6"", ""PrimaryColor"": ""#FFFFFF"", ""SecondaryColor"": ""#8E24AA"" },
            { ""Name"": ""白字紫底7"", ""PrimaryColor"": ""#FFFFFF"", ""SecondaryColor"": ""#7B1FA2"" },
            { ""Name"": ""白字紫底8"", ""PrimaryColor"": ""#FFFFFF"", ""SecondaryColor"": ""#6A1B9A"" },
            { ""Name"": ""灰字红底0"", ""PrimaryColor"": ""#455A64"", ""SecondaryColor"": ""#FFEBEE"" },
            { ""Name"": ""白字红底1"", ""PrimaryColor"": ""#FFFFFF"", ""SecondaryColor"": ""#FFCDD2"" },
            { ""Name"": ""白字红底2"", ""PrimaryColor"": ""#FFFFFF"", ""SecondaryColor"": ""#EF9A9A"" },
            { ""Name"": ""白字红底3"", ""PrimaryColor"": ""#FFFFFF"", ""SecondaryColor"": ""#E57373"" },
            { ""Name"": ""白字红底4"", ""PrimaryColor"": ""#FFFFFF"", ""SecondaryColor"": ""#EF5350"" },
            { ""Name"": ""白字红底5"", ""PrimaryColor"": ""#FFFFFF"", ""SecondaryColor"": ""#F44336"" },
            { ""Name"": ""白字红底6"", ""PrimaryColor"": ""#FFFFFF"", ""SecondaryColor"": ""#E53935"" },
            { ""Name"": ""白字红底7"", ""PrimaryColor"": ""#FFFFFF"", ""SecondaryColor"": ""#D32F2F"" },
            { ""Name"": ""白字红底8"", ""PrimaryColor"": ""#FFFFFF"", ""SecondaryColor"": ""#C62828"" },
            { ""Name"": ""白字红底9"", ""PrimaryColor"": ""#FFFFFF"", ""SecondaryColor"": ""#B71C1C"" },
            { ""Name"": ""灰字青底0"", ""PrimaryColor"": ""#455A64"", ""SecondaryColor"": ""#E0F2F1"" },
            { ""Name"": ""白字青底1"", ""PrimaryColor"": ""#FFFFFF"", ""SecondaryColor"": ""#B2DFDB"" },
            { ""Name"": ""白字青底2"", ""PrimaryColor"": ""#FFFFFF"", ""SecondaryColor"": ""#80CBC4"" },
            { ""Name"": ""白字青底3"", ""PrimaryColor"": ""#FFFFFF"", ""SecondaryColor"": ""#4DB6AC"" },
            { ""Name"": ""白字青底4"", ""PrimaryColor"": ""#FFFFFF"", ""SecondaryColor"": ""#26A69A"" },
            { ""Name"": ""白字青底5"", ""PrimaryColor"": ""#FFFFFF"", ""SecondaryColor"": ""#009688"" },
            { ""Name"": ""白字青底6"", ""PrimaryColor"": ""#FFFFFF"", ""SecondaryColor"": ""#00897B"" },
            { ""Name"": ""白字青底7"", ""PrimaryColor"": ""#FFFFFF"", ""SecondaryColor"": ""#00796B"" },
            { ""Name"": ""白字青底8"", ""PrimaryColor"": ""#FFFFFF"", ""SecondaryColor"": ""#00695C"" },
            { ""Name"": ""白字青底9"", ""PrimaryColor"": ""#FFFFFF"", ""SecondaryColor"": ""#004D40"" },
            { ""Name"": ""灰字橙底0"", ""PrimaryColor"": ""#455A64"", ""SecondaryColor"": ""#FBE9E7"" },
            { ""Name"": ""白字橙底1"", ""PrimaryColor"": ""#FFFFFF"", ""SecondaryColor"": ""#FFCCBC"" },
            { ""Name"": ""白字橙底2"", ""PrimaryColor"": ""#FFFFFF"", ""SecondaryColor"": ""#FFAB91"" },
            { ""Name"": ""白字橙底3"", ""PrimaryColor"": ""#FFFFFF"", ""SecondaryColor"": ""#FF8A65"" },
            { ""Name"": ""白字橙底4"", ""PrimaryColor"": ""#FFFFFF"", ""SecondaryColor"": ""#FF7043"" },
            { ""Name"": ""白字橙底5"", ""PrimaryColor"": ""#FFFFFF"", ""SecondaryColor"": ""#FF5722"" },
            { ""Name"": ""白字橙底6"", ""PrimaryColor"": ""#FFFFFF"", ""SecondaryColor"": ""#F4511E"" },
            { ""Name"": ""白字橙底7"", ""PrimaryColor"": ""#FFFFFF"", ""SecondaryColor"": ""#E64A19"" },
            { ""Name"": ""白字橙底8"", ""PrimaryColor"": ""#FFFFFF"", ""SecondaryColor"": ""#D84315"" },
            { ""Name"": ""白字橙底9"", ""PrimaryColor"": ""#FFFFFF"", ""SecondaryColor"": ""#BF360C"" },
            { ""Name"": ""灰字绿底0"", ""PrimaryColor"": ""#455A64"", ""SecondaryColor"": ""#E8F5E9"" },
            { ""Name"": ""白字绿底1"", ""PrimaryColor"": ""#FFFFFF"", ""SecondaryColor"": ""#C8E6C9"" },
            { ""Name"": ""白字绿底2"", ""PrimaryColor"": ""#FFFFFF"", ""SecondaryColor"": ""#A5D6A7"" },
            { ""Name"": ""白字绿底3"", ""PrimaryColor"": ""#FFFFFF"", ""SecondaryColor"": ""#81C784"" },
            { ""Name"": ""白字绿底4"", ""PrimaryColor"": ""#FFFFFF"", ""SecondaryColor"": ""#66BB6A"" },
            { ""Name"": ""白字绿底5"", ""PrimaryColor"": ""#FFFFFF"", ""SecondaryColor"": ""#4CAF50"" },
            { ""Name"": ""白字绿底6"", ""PrimaryColor"": ""#FFFFFF"", ""SecondaryColor"": ""#43A047"" },
            { ""Name"": ""白字绿底7"", ""PrimaryColor"": ""#FFFFFF"", ""SecondaryColor"": ""#388E3C"" },
            { ""Name"": ""白字绿底8"", ""PrimaryColor"": ""#FFFFFF"", ""SecondaryColor"": ""#2E7D32"" },
            { ""Name"": ""白字绿底9"", ""PrimaryColor"": ""#FFFFFF"", ""SecondaryColor"": ""#1B5E20"" },
            { ""Name"": ""灰字蓝灰0"", ""PrimaryColor"": ""#455A64"", ""SecondaryColor"": ""#ECEFF1"" },
            { ""Name"": ""白字蓝灰1"", ""PrimaryColor"": ""#FFFFFF"", ""SecondaryColor"": ""#CFD8DC"" },
            { ""Name"": ""白字蓝灰2"", ""PrimaryColor"": ""#FFFFFF"", ""SecondaryColor"": ""#B0BEC5"" },
            { ""Name"": ""白字蓝灰3"", ""PrimaryColor"": ""#FFFFFF"", ""SecondaryColor"": ""#90A4AE"" },
            { ""Name"": ""白字蓝灰4"", ""PrimaryColor"": ""#FFFFFF"", ""SecondaryColor"": ""#78909C"" },
            { ""Name"": ""白字蓝灰5"", ""PrimaryColor"": ""#FFFFFF"", ""SecondaryColor"": ""#607D8B"" },
            { ""Name"": ""白字蓝灰6"", ""PrimaryColor"": ""#FFFFFF"", ""SecondaryColor"": ""#546E7A"" },
            { ""Name"": ""白字蓝灰7"", ""PrimaryColor"": ""#FFFFFF"", ""SecondaryColor"": ""#455A64"" },
            { ""Name"": ""白字蓝灰8"", ""PrimaryColor"": ""#FFFFFF"", ""SecondaryColor"": ""#37474F"" },
            { ""Name"": ""白字蓝灰9"", ""PrimaryColor"": ""#FFFFFF"", ""SecondaryColor"": ""#263238"" }
        ]
        }";
    }
}