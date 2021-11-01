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


        protected override void OnToolbarDraw()
        {
            int toolItemIndex = _toolItemIndex;
            if (DrawToolItem(Icon.Get(Icon.Material_download), "Clone", toolItemIndex == 0))
            {
                toolItemIndex = 0;
            }
            ImGui.SameLine();
            if (DrawToolItem(Icon.Get(Icon.Material_add), "Add", toolItemIndex == 1))
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
                    if (string.IsNullOrEmpty(_remoteURL) || string.IsNullOrEmpty(_localPath))
                        return;

                    string remoteURL = _remoteURL;
                    string repoName = Path.GetFileNameWithoutExtension(remoteURL);
                    if (!remoteURL.EndsWith(".git"))
                    {
                        remoteURL = $"{remoteURL}.git";
                    }
                    string localPath = Path.Combine(_localPath, repoName);
                    if (Directory.Exists(localPath))
                        return;

                    var cloneOptions = new CloneOptions();
                    cloneOptions.OnProgress = OnCloneProcess;
                    var result = Repository.Clone(remoteURL, localPath, cloneOptions);
                    Console.WriteLine(result);
                }
            }
        }


        private bool OnCloneProcess(string serverProgressOutput)
        {
            Console.WriteLine(serverProgressOutput);
            return true;
        }
    }
}
