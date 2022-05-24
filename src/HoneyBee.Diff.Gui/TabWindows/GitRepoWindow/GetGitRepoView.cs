using ImGuiNET;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoneyBee.Diff.Gui
{ 
    public class GetGitRepoView: GitRepoView
    {
        //Clone / Add / Create tool item index.
        private int _toolItemIndex = 0;
        private string _remoteURL = "";
        private string _localPath = "";
        private bool _useDefaultRepoName = true;
        private string _repoName = "";

        private Action<string> _getGitRepoPath;
        private List<ToolbarTab> _toolbarTabs;

        public GetGitRepoView(Action<string> getGitRepoPath)
        {
            _getGitRepoPath = getGitRepoPath;

            _toolbarTabs = new List<ToolbarTab>();
            _toolbarTabs.Add(new ToolbarTab() { Icon = Icon.Material_download, Tip = "Clone", OnDraw = DrawClone });
            _toolbarTabs.Add(new ToolbarTab() { Icon = Icon.Material_add, Tip = "Add", OnDraw = DrawAdd });
            _toolbarTabs.Add(new ToolbarTab() { Icon = Icon.Material_create, Tip = "Create", OnDraw = DrawCreate });
        }

        protected override void OnToolbarDraw()
        {
            for (int i = 0; i < _toolbarTabs.Count; i++)
            {
                var item = _toolbarTabs[i];
                if (DrawToolItem(Icon.Get(item.Icon), item.Tip, _toolItemIndex == i))
                {
                    _toolItemIndex = i;
                }
                if (i < _toolbarTabs.Count - 1)
                    ImGui.SameLine();
            }
            ImGui.Separator();
        }

        protected override void OnDrawContent()
        {
            _toolbarTabs[_toolItemIndex].OnDraw();
        }

       

        private void DrawClone()
        {
            ImGui.Text("Clone");
            if (ImGui.InputText("Git_Remote_URL", ref _remoteURL, 200))
            {
                if (_useDefaultRepoName || string.IsNullOrEmpty(_repoName))
                {
                    _repoName = Path.GetFileNameWithoutExtension(_remoteURL);
                }
            }
            ImGui.InputText("Git_Local_Path", ref _localPath, 200);
            ImGui.SameLine();
            if (ImGui.Button(Icon.Get(Icon.Material_open_in_browser)))
            {
                string localFolderPath = string.IsNullOrEmpty(_localPath) ? "./" : _localPath;
                ImGuiFileDialog.OpenFolder((selectPath) => {
                    if (!string.IsNullOrEmpty(selectPath))
                    {
                        _localPath = selectPath;
                    }
                }, localFolderPath);
            }

            string localPath = null;
            if (!string.IsNullOrEmpty(_remoteURL))
            {
                if (ImGui.Checkbox("Default repository name", ref _useDefaultRepoName))
                {
                    if (_useDefaultRepoName)
                    {
                        _repoName = Path.GetFileNameWithoutExtension(_remoteURL);
                    }
                }
                ImGui.SameLine();
                if (_useDefaultRepoName)
                {
                    ImGui.Text(_repoName);
                }
                else
                {
                     ImGui.InputText("", ref _repoName, 50);
                }

                if (!string.IsNullOrEmpty(_localPath))
                {
                    localPath = Path.Combine(_localPath, _repoName);
                    if (Directory.Exists(localPath))
                    {
                        //ImGui.SameLine();
                        ImGui.TextColored(new System.Numerics.Vector4(1,0, 0, 1), "Folder already exists");
                        localPath = null;
                    }
                }
            }

            bool disableClone = string.IsNullOrEmpty(localPath);
            if (disableClone)
            {
                ImGui.BeginDisabled();
            }

            if (ImGui.Button(Icon.Get(Icon.Material_download) + "Clone"))
            {
                string remoteURL = _remoteURL;
                if (!remoteURL.EndsWith(".git"))
                {
                    remoteURL = $"{remoteURL}.git";
                }

                Git.Clone(remoteURL, localPath,(log)=> {
                    GlobalControl.DisplayProgressBar("Clone", log, 0);
                },(gitPath)=>{
                    GlobalControl.DisplayProgressBar("Clone", "Compolete", -99.0f);
                    _getGitRepoPath?.Invoke(gitPath);
                });
            }

            if (disableClone)
            {
                ImGui.EndDisabled();
            }
        }
        private void DrawAdd()
        {
            ImGui.InputText("Git_Local_Path", ref _localPath, 200);
            ImGui.SameLine();
            if (ImGui.Button(Icon.Get(Icon.Material_open_in_browser)))
            {
                string localPath = string.IsNullOrEmpty(_localPath) ? "./" : _localPath;
                ImGuiFileDialog.OpenFolder((selectPath) => {
                    if (!string.IsNullOrEmpty(selectPath))
                    {
                        _localPath = selectPath;
                    }
                }, localPath);
            }

            if (ImGui.Button(Icon.Get(Icon.Material_add) + "Add"))
            {
                if (string.IsNullOrEmpty(_localPath))
                    return;
                string gitPath = string.Empty;
                if (_localPath.EndsWith(".git"))
                {
                    gitPath = _localPath;

                }
                else
                {
                    string fullPath = Path.Combine(_localPath, ".git");
                    if (Directory.Exists(fullPath))
                    {
                        gitPath = fullPath;
                    }
                }
                if (!string.IsNullOrEmpty(gitPath))
                    _getGitRepoPath?.Invoke(gitPath);
            }
        }

        private void DrawCreate()
        {
            ImGui.InputText("Git_Local_Path", ref _localPath, 200);
            ImGui.SameLine();
            if (ImGui.Button(Icon.Get(Icon.Material_open_in_browser)))
            {
                string localPath = string.IsNullOrEmpty(_localPath) ? "./" : _localPath;
                ImGuiFileDialog.OpenFolder((selectPath) => {
                    if (!string.IsNullOrEmpty(selectPath))
                    {
                        _localPath = selectPath;
                    }
                }, localPath);
            }

            if (ImGui.Button(Icon.Get(Icon.Material_create) + "Create"))
            {
                if (string.IsNullOrEmpty(_localPath))
                    return;

                string gitPath = string.Empty;
                if (!_localPath.EndsWith(".git"))
                {
                    string fullPath = Path.Combine(_localPath, ".git");
                    if (!Directory.Exists(fullPath))
                    {
                        Repository.Init(fullPath);
                        gitPath = fullPath;
                    }

                }
                if (!string.IsNullOrEmpty(gitPath))
                    _getGitRepoPath?.Invoke(gitPath);
            }
        }

        private bool OnCloneProcess(string serverProgressOutput)
        {
            Console.WriteLine(serverProgressOutput);
            return true;
        }

        struct ToolbarTab
        {
            public int Icon;
            public string Tip;
            public Action OnDraw;
        }
    }
}
