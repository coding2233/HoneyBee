using ImGuiNET;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace HoneyBee.Diff.Gui
{
    public class GitRepoWindow : ITabWindow
    {
        public string Name => "GitRepoWindow";

        public string IconName => Icon.Get(Icon.Material_gite);

        public bool Unsave => false;

        public bool ExitModal { get; set; }

        private bool _isGitRepo;
        private string _repoName;
        private string _repoPath;
        private string repoPath
        {
            get
            {
                return _repoPath;
            }
            set
            {
                _repoPath = value;
                _repoName = string.Empty;
                if (!string.IsNullOrEmpty(_repoPath))
                {
                    _repoName = Path.GetFileName(Path.GetDirectoryName(_repoPath));
                }
                _isGitRepo = IsGitRepo();
            }
        }

        //Clone / Add / Create tool item index.
        private int _toolItemIndex = 0;


        public void Setup(params object[] parameters)
        {
        }

        private string _remoteURL="";
        private string _localPath="";
        public void OnDraw()
        {
            if (_isGitRepo)
            {
                
            }
            else
            {
                int toolItemIndex = _toolItemIndex;
                if (DrawToolItem(Icon.Get(Icon.Material_download),"Clone", toolItemIndex == 0))
                {
                    toolItemIndex = 0;
                }
                ImGui.SameLine();
                if (DrawToolItem(Icon.Get(Icon.Material_add),"Add", toolItemIndex == 1))
                {
                    toolItemIndex = 1;
                }
                ImGui.SameLine();
                if (DrawToolItem(Icon.Get(Icon.Material_create), "Create", toolItemIndex == 2))
                {
                    toolItemIndex = 2;
                }
                if (toolItemIndex != _toolItemIndex)
                {
                    _toolItemIndex = toolItemIndex;
                }
                ImGui.Separator();

                if (_toolItemIndex == 0)
                {
                    ImGui.Text("Clone");
                    ImGui.InputText("Git_Remote_URL", ref _remoteURL, 500);
                    ImGui.InputText("Git_Local_Path", ref _localPath, 500);
                    ImGui.SameLine();
                    ImGui.Button(Icon.Get(Icon.Material_open_in_browser));
                    if (ImGui.Button(Icon.Get(Icon.Material_download) + "Clone"))
                    {
                        
                        var result = Repository.Clone("https://gitee.com/xiyoufang/aij.git", "./aij");
                        Console.WriteLine(result);
                    }
                }
            }
        }

        public void OnExitModalSure()
        {
        }

        public string Serialize()
        {
            return repoPath;
        }

        public void Deserialize(string data)
        {
            repoPath = data;
        }
        public void Dispose()
        {
        }

        bool IsGitRepo()
        {
            return !string.IsNullOrEmpty(_repoPath) && Repository.IsValid(_repoPath);
        }

        private bool DrawToolItem(string icon, string tip,bool active)
        {
            bool buttonClick = ImGui.Button(icon);
            var p1 = ImGui.GetItemRectMin();
            var p2 = ImGui.GetItemRectMax();
            p1.Y = p2.Y;
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35.0f);
                ImGui.TextUnformatted(tip);
                ImGui.PopTextWrapPos();
                ImGui.EndTooltip();
            }
            if(active)
                ImGui.GetWindowDrawList().AddLine(p1, p2, ImGui.GetColorU32(ImGuiCol.ButtonActive));
            return buttonClick;
        }
    }
}
