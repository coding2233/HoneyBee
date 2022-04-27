using ImGuiNET;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoneyBee.Diff.Gui
{
    public class TerminalWindow : ITabWindow
    {
        public string Name => "Terminal";

        public string IconName => Icon.Get(Icon.Material_terminal);

        public bool Unsave => false;

        public bool ExitModal { get; set; }

        private string _rootPath = "";
        private string _userName;
        private string _userDomain;

        private string _cdPath = "";
        private string _command = "";
        public TerminalWindow()
        {
            _rootPath = Environment.GetEnvironmentVariable("USERPROFILE");
            if (string.IsNullOrEmpty(_rootPath))
            {
                _rootPath = "./";
            };
            _cdPath = _rootPath = Path.GetFullPath(_rootPath);
            _userName = Environment.GetEnvironmentVariable("USERNAME");
            _userDomain = Environment.GetEnvironmentVariable("USERDOMAIN");
        }

        public bool Deserialize(string data)
        {
            return false;
        }

        public void Dispose()
        {
           
        }

        public void OnDraw(bool canInput=true)
        {
            string cdPath = _cdPath.Equals(_rootPath) ? "~" : _cdPath;
            ImGui.Text($"{_userName}@{_userDomain} {cdPath}");
            if (ImGui.InputText(_command, ref _command,200))
            {
                
            }
        }

        public void OnExitModalSure()
        {
            
        }

        public string Serialize()
        {
            return null;
        }

        public void Setup(params object[] parameters)
        {
            
        }
    }
}
