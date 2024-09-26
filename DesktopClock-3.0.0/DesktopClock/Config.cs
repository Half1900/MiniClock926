using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Linq;
using System.Windows;
using System.Collections.Generic;

namespace DesktopClock
{
    public class Config
    {
        [DllImport("kernel32.dll")]
        private static extern long WritePrivateProfileString(string section, string key, string value, string filepath);

        [DllImport("kernel32.dll")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        private readonly string config_path;
        private readonly string exe_path;
        private readonly string _configFilePath;
        public Config(string configFilePath)
        {
            _configFilePath = configFilePath;
        }
        public bool IsExistName(string name)
        {
            if (!File.Exists(_configFilePath))
            {
                throw new FileNotFoundException("Config文件不存在", _configFilePath);
            }

            var lineCount = 0; // 添加计数器
            var keys = new HashSet<string>(); // 用于保存所有的键
            var totalLines = File.ReadLines(_configFilePath).Count(); // 获取总行数
            using (var reader = new StreamReader(_configFilePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null && lineCount < totalLines)
                {
                    lineCount++; // 每读一行，计数器加一
                    // 忽略注释和空行
                    if (string.IsNullOrWhiteSpace(line) || line.Trim().StartsWith("#"))
                    {
                        continue;
                    }
                    // 分割key和value
                    var separator = new[] { '=' };
                    var parts = line.Split(separator, 2);
                    if (parts.Length == 2)
                    {
                       var key = parts[0].Trim();
                       keys.Add(key); // 保存键                   
                    }
                }
            }
            return keys.Contains(name, StringComparer.OrdinalIgnoreCase); // 检查传入的name是否存在于集合中
        }
        public Config()
        {
            exe_path = Process.GetCurrentProcess().MainModule.FileName;
            var exename = Process.GetCurrentProcess().MainModule.ModuleName;
            config_path = exe_path.Replace(exename, "") + "config.ini";
        }


        public void WriteKeyValue(string name,string value)
        {
            WriteKey(name, $"\"{value}\"");
        }
        public int ReadBool(string key)
        {
            var str = ReadString(key);
            return str switch
            {
                "True" => 1,
                "0" => -1,
                _ => 0
            };
        }

        public string ReadString(string key)
        {
            var v = new StringBuilder(1024);
            string v2;
            GetPrivateProfileString("config", key, "0", v, 1024, config_path);
            v2 = v.ToString();
            return v2;
        }

        public string ReadBoundleString(string key)
        {
            var v = new StringBuilder(1024);
            string v2;
            GetPrivateProfileString("config", key, "0.00", v, 1024, config_path);
            v2 = v.ToString();
            return v2.Trim('"');
        }

        public string GetTextColor(string name)
        {
            var v = new StringBuilder(1024);
            GetPrivateProfileString("config", name, "#000000", v, 1024, config_path);
            var color = v.ToString();
            return color.Trim('"'); // 去掉首尾的引号
        }
        public string GetOuterColor(string name)
        {
            var v = new StringBuilder(1024);
            GetPrivateProfileString("config", name, "#FFFFFF", v, 1024, config_path);
            var color = v.ToString();
            return color.Trim('"'); // 去掉首尾的引号
        }
        public string GetColorName(string name)
        {
            var v = new StringBuilder(1024);
            GetPrivateProfileString("config", name, "Custom", v, 1024, config_path);
            var color = v.ToString();
            return color.Trim('"'); // 去掉首尾的引号
        }

        public void Write(string key, bool value) => WriteKey(key, Convert.ToString(value));


        public void WriteKey(string key, string value)
        {
            WritePrivateProfileString("config", key, value, config_path);
        }

        public bool Read_auto_start()
        {
            try
            {
                using (var registryKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
                {
                    var str = (string)registryKey.GetValue("DesktopClock");
                    Trace.WriteLine("GetValue:" + str);
                    registryKey.Close();
                    return str == exe_path;
                }
            }
            catch (Exception ex)
            {
                // 在这里处理异常，例如记录日志或向用户显示错误消息  
                Console.WriteLine("Error accessing registry: " + ex.Message);
                return false; // 或其他适当的默认值  
            }
        }
        public void Add_auto_start()
        {
            SetSelfStart(true);
        }
        public void Del_auto_start()
        {
            SetSelfStart(false);
        }
      
        private static void CreateShortcut(string lnkFilePath, string args = "")
        {
            var shellType = Type.GetTypeFromProgID("WScript.Shell");
            dynamic shell = Activator.CreateInstance(shellType);
            var shortcut = shell.CreateShortcut(lnkFilePath);
            shortcut.TargetPath = Assembly.GetEntryAssembly().Location;
            shortcut.Arguments = args;
            shortcut.WorkingDirectory = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            shortcut.Save();
        }
        public void SetSelfStart(bool b)
        {
            try
            {
                var exePath = Process.GetCurrentProcess().MainModule.FileName;
                var exeName = Path.GetFileNameWithoutExtension(exePath);
                var dir = Directory.GetCurrentDirectory();
                var exeDir = Path.Combine(dir, exeName + ".lnk");
                var StartupPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
                var lnkFilePath = Path.Combine(StartupPath, exeName + ".lnk");
                // 检查是否需要管理员权限  
                if (b)
                {
                    // 设置开机自启动        
                    //var dir = Directory.GetCurrentDirectory();
                    //var exeDir = dir + @"\DesktopClock.lnk";
                    if (!File.Exists(exeDir))
                    {
                        CreateShortcut(exeDir, "");
                    }
                    File.Copy(exeDir,lnkFilePath, true);
                }
                else
                {
                    // 取消开机自启动                       
                    try
                    {
                        File.Delete(lnkFilePath);
                        File.Delete(exeDir);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("无法删除文件: " + ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}

