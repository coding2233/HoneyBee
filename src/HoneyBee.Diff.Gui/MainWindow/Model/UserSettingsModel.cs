using ImGuiNET;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace HoneyBee.Diff.Gui
{
    [Export(typeof(IUserSettingsModel))]
    public class UserSettingsModel : IUserSettingsModel
    {
        private int _styleColors = -1;
        public int StyleColors 
        {
            get
            {
                if (_styleColors == -1)
                {
                    _styleColors = GetInt("StyleColors", 1);
                }
                return _styleColors;
            }
            set
            {
                _styleColors = value;
                SetInt("StyleColors", value);
            }
        }

        public uint MarkBgColor
        {
            get
            {
                uint markBgColor = 0;
                //lightģʽ
                if (_styleColors == 0)
                {
                    markBgColor = ImGui.GetColorU32(new Vector4(1, 0.8f, 0.8f, 1));
                }
                else
                {
                    markBgColor = ImGui.GetColorU32(new Vector4(0.8f, 0.2f, 0.2f, 1));
                }
                return markBgColor;
            }
        }
        public Vector4 MarkRedColor
        {
            get
            {
                //lightģʽ
                if (_styleColors == 0)
                {
                    return new Vector4(0.8f, 0.2f, 0.2f, 1);
                }
                else
                {
                     return new Vector4(1, 0.8f, 0.8f, 1);
                }
            }
        }

        public Vector4 MarkGreenColor
        {
            get
            {
                //lightģʽ
                if (_styleColors == 0)
                {
                    return new Vector4(0.2f, 0.8f, 0.2f, 1);
                }
                else
                {
                    return new Vector4(0.8f, 1.0f, 0.8f, 1);
                }
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
