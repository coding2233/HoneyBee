using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoneyBee.Diff.Gui
{
    [Export(typeof(IUserSettingsModel))]
    public class UserSettingsModel : IUserSettingsModel
    {
        public int StyleColors 
        {
            get
            {
                return GetInt("StyleColors",1);
            }
            set
            {
                SetInt("StyleColors", value);
            }
        }


        private string _userSettingsPath
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

        private string GetPath(string key)
        {
            return $"{_userSettingsPath}/{key}";
        }

        public void SetInt(string key, int value)
        {
            File.WriteAllText(GetPath(key), value.ToString());
        }

        public int GetInt(string key, int defaultValue = 0)
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
