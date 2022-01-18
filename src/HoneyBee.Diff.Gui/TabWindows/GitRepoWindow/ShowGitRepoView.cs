using ImGuiNET;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using LibGit2Sharp;

namespace HoneyBee.Diff.Gui
{
    public class ShowGitRepoView:GitRepoView
    {
        private string _repoName ="";
        private Repository _repository;
        private int _commitAddInterval = 5;
        private int _commitViewIndex = 0;
        private int _commitViewMax = 100;
        private float _lastCommitScrollY = 0.0f;
        private SplitView _splitView = new SplitView(SplitView.SplitType.Horizontal);
        private Commit _selectCommit;

        public void SetRepoPath(string repoPath)
        {
            RepoPath = repoPath;
            if (!string.IsNullOrEmpty(RepoPath))
            {
                _repoName = Path.GetFileNameWithoutExtension(RepoPath);
                _repository = new Repository(RepoPath);
            }
        }

        protected override void OnToolbarDraw()
        {
            DrawToolItem(Icon.Get(Icon.Material_add), "Commit", true);
            ImGui.SameLine();
            DrawToolItem(Icon.Get(Icon.Material_get_app), "Pull", false);
            ImGui.SameLine();
            DrawToolItem(Icon.Get(Icon.Material_get_app), "Fetch", false);
            ImGui.SameLine();
            ImGui.Spacing();
            ImGui.SameLine();
            DrawToolItem(Icon.Get(Icon.Material_settings), "Settings", false);

            ImGui.Separator();

        }

        protected override void OnDrawContent()
        {
            _splitView.Begin();
            OnRepoKeysDraw();
            _splitView.Separate();
            OnRepoContentDraw();
            _splitView.End();
        }


        private void OnRepoKeysDraw()
        {
            if (ImGui.TreeNode("Workspace"))
            {
                if (ImGui.Button("File status"))
                {
                    
                }
                if (ImGui.Button("History"))
                {

                }
                ImGui.TreePop();
            }

            if (ImGui.TreeNode("Branch"))
            {
                foreach (var item in _repository.Branches)
                {
                    if (!item.IsRemote)
                    {
                        ImGui.Button($"{item.FriendlyName}");
                    }
                }
                ImGui.TreePop();
            }

            if (ImGui.TreeNode("Tag"))
            {
                foreach (var item in _repository.Tags)
                {
                    ImGui.Button($"{item.FriendlyName}");
                }
                ImGui.TreePop();
            }

            if (ImGui.TreeNode("Remote"))
            {
                foreach (var item in _repository.Branches)
                {
                    if (item.IsRemote)
                        ImGui.Button($"{item.FriendlyName}");
                }
                ImGui.TreePop();
            }

            if (ImGui.TreeNode("Submodule"))
            {
                foreach (var item in _repository.Submodules)
                {
                    ImGui.Button($"{item.Name}");
                }
                ImGui.TreePop();
            }
        }

        private void OnRepoContentDraw()
        {
            int commitMax = _repository.Commits.Count();
            if (_lastCommitScrollY <= 0.0f)
            {
                _commitViewIndex-= _commitAddInterval;
                _commitViewIndex = Math.Max(_commitViewIndex, 0);
                if(_commitViewIndex>0)
                    ImGui.SetScrollY(10);
            }
            else if (_lastCommitScrollY >= ImGui.GetScrollMaxY())
            {
                _commitViewIndex += _commitAddInterval ;
                if (commitMax > _commitViewMax)
                {
                    commitMax = commitMax - _commitViewMax;
                }
                _commitViewIndex = Math.Min(_commitViewIndex, commitMax);
                if(_commitViewIndex < commitMax)
                    ImGui.SetScrollY(_lastCommitScrollY-10);
            }
            _lastCommitScrollY = ImGui.GetScrollY();

            if (ImGui.BeginTable("GitRepo-Commits", 4, ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders | ImGuiTableFlags.Resizable | ImGuiTableFlags.Reorderable))
            {
                ImGui.TableSetupColumn("Description", ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableSetupColumn("Date", ImGuiTableColumnFlags.WidthFixed );
                ImGui.TableSetupColumn("Author", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("Commit", ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoResize);
                ImGui.TableHeadersRow();
                int index = 0;
                foreach (var item in _repository.Commits)
                {
                    index++;
                    if (index < _commitViewIndex)
                        continue;
                    else if (index >= _commitViewIndex + _commitViewMax)
                        break;

                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.Text(item.MessageShort);
                    if (_selectCommit == item || ImGui.IsItemHovered())
                    {
                        ImGui.TableSetBgColor(ImGuiTableBgTarget.RowBg0, ImGui.GetColorU32(ImGuiCol.TabActive));
                        ImGui.TableSetBgColor(ImGuiTableBgTarget.RowBg1, ImGui.GetColorU32(ImGuiCol.TabActive));
                    }
                    if (ImGui.IsItemClicked())
                    {
                        _selectCommit = item;
                    }
                    ImGui.TableSetColumnIndex(1);
                    ImGui.Text(item.Committer.When.ToString("yyyy-MM-dd HH:mm:ss"));
                    ImGui.TableSetColumnIndex(2);
                    ImGui.Text($"{item.Committer.Name}");// [{item.Committer.Email}]
                    ImGui.TableSetColumnIndex(3);
                    ImGui.Text($"{item.Sha.Substring(0, 10)}");
                }
                ImGui.EndTable();
            }

            
        }

    }
}
