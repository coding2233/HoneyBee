using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoneyBee.Diff.Gui
{
    public class UserSettings
    {
        private static string _userSettingsPath 
        { 
            get 
            {
                string path = "./.userSettings";
                if (!Directory.Exists("path"))
                {
                    Directory.CreateDirectory(path);
                }
                return path;
            } 
        }

        private static string GetPath(string key)
        {
            return $"{_userSettingsPath}/{key}";
        }

        public static void SetInt(string key, int value)
        {
            File.WriteAllText(GetPath(key), value.ToString());
        }

        public static int GetInt(string key, int defaultValue = 0)
        {
            var path = GetPath(key);
            if (File.Exists(path))
            {
                int.TryParse(File.ReadAllText(path), out defaultValue);
            }
            return defaultValue;
        }

    }
}
