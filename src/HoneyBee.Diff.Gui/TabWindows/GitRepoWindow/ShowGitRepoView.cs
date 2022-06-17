using ImGuiNET;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using LibGit2Sharp;
using System.ComponentModel.Composition;
using System.Diagnostics;

namespace HoneyBee.Diff.Gui
{
    public class ShowGitRepoView:GitRepoView
    {
        public enum WorkSpaceRadio
        {
            WorkTree,
            CommitHistory,
        }

        private Git _git;

        private WorkSpaceRadio _workSpaceRadio= WorkSpaceRadio.WorkTree;
        private int _commitAddInterval = 5;
        private int _commitViewIndex = 0;
        private int _commitViewMax = 100;
        private float _lastCommitScrollY = 0.0f;
        private SplitView _splitView = new SplitView(SplitView.SplitType.Horizontal,2,200);
        private SplitView _contentSplitView = new SplitView(SplitView.SplitType.Vertical,2,300,0.9f);
        private ShowCommitView _showCommitView = new ShowCommitView();
        private WorkTreeView _workTreeView = new WorkTreeView();
        private Commit _selectCommit=null;

        [Import]
        public IUserSettingsModel userSettingsModel { get; set; }

        private Dictionary<string, int> _toolItems;
        private string _toolItemSelected = "";

        private bool _showFetch;
        private bool _forcePush;
        private RemoteBranchTrack _remoteBranchTrack=new RemoteBranchTrack();

        public ShowGitRepoView()
        {
            DiffProgram.ComposeParts(this);

            _toolItems = new Dictionary<string, int>();
            _toolItems.Add("Commit", Icon.Material_add);
            _toolItems.Add("Sync", Icon.Material_sync);
            _toolItems.Add("Pull", Icon.Material_download);
            _toolItems.Add("Push", Icon.Material_publish);
            _toolItems.Add("Fetch", Icon.Material_downloading);
            _toolItems.Add("Settings", Icon.Material_settings);
            _toolItems.Add("Terminal", Icon.Material_terminal);
            _toolItemSelected = "Commit";
        }

        public void SetRepoPath(string repoPath)
        {
            if (!string.IsNullOrEmpty(repoPath))
            {
                _git = new Git(repoPath);
                RepoPath = _git.RepoPath;
            }
        }


        protected override void OnToolbarDraw()
        {
            int itemIndex = 0;
            foreach (var item in _toolItems)
            {
                if (DrawToolItem(Icon.Get(item.Value), item.Key, item.Key == _toolItemSelected))
                {
                    _toolItemSelected = item.Key;
                    OnClickToolbar(_toolItemSelected);
                }
                if (itemIndex >= 0 && itemIndex < _toolItems.Count - 1)
                {
                    ImGui.SameLine();
                }
                itemIndex++;
            }
            ImGui.Separator();
        }

        protected override void OnDrawContent()
        {
            if (_git == null)
                return;

            _splitView.Begin();
            OnRepoKeysDraw();
            _splitView.Separate();
            OnRepoContentDraw();
            _splitView.End();

            //Draw fetch
            DrawFetchWindow();
            DrawPushWindow();
        }


        private void OnClickToolbar(string item)
        {
            switch (item)
            {
                case "Sync":
                    _git.UpdateStatus();
                    break;
                case "Terminal":
                    Terminal.SetShow(_git.RepoRootPath);
                    break;
                case "Pull":
                    Terminal.Pull(_git.RepoRootPath);
                    break;
                case "Fetch":
                    ImGui.OpenPopup("Fetch");
                    _remoteBranchTrack.GetData(_git);
                    _showFetch = true;
                    break;
                case "Push":
                    ImGui.OpenPopup("Push");
                    _remoteBranchTrack.GetData(_git);
                    _showFetch = true;
                    break;
                default:
                    break;
            }
           
        }

        //private bool OnProgressHandler(string s)
        //{
        //    GlobalControl.DisplayProgressBar("Clone", s, 0); 
        //    return true;
        //}

        private void OnRepoKeysDraw()
        {
            DrawTreeNodeHead("Workspace", () => {
                if (ImGui.RadioButton("Work tree", _workSpaceRadio == WorkSpaceRadio.WorkTree))
                {
                    _workSpaceRadio = WorkSpaceRadio.WorkTree;
                    _git.Status();
                }

                if (ImGui.RadioButton("Commit history", _workSpaceRadio == WorkSpaceRadio.CommitHistory))
                {
                    _workSpaceRadio = WorkSpaceRadio.CommitHistory;
                }
            });

            DrawTreeNodeHead("Branch", () => {
                foreach (var item in _git.LocalBranchNodes)
                {
                    DrawBranchTreeNode(item);
                }
            });

            DrawTreeNodeHead("Tag", () => {
                foreach (var item in _git.Tags)
                {
                    ImGui.Button($"{item.FriendlyName}");
                }
            });

            DrawTreeNodeHead("Remote", () => {
                foreach (var item in _git.RemoteBranchNodes)
                {
                    DrawBranchTreeNode(item);
                }
            });

            DrawTreeNodeHead("Submodule", () => {
                foreach (var item in _git.Submodules)
                {
                    ImGui.Button($"{item.Name}");
                }
            });
        }

        private void DrawTreeNodeHead(string name,Action onDraw)
        {
            string key = $"TreeNode_{name}";
            bool oldTreeNodeOpen = userSettingsModel.Get<bool>(key, false);
            ImGui.SetNextItemOpen(oldTreeNodeOpen);
            bool treeNodeOpen = ImGui.TreeNode(name);
            if(treeNodeOpen)
            {
                onDraw();
                ImGui.TreePop();
            }
            if (treeNodeOpen != oldTreeNodeOpen)
            {
                userSettingsModel.Set<bool>(key, treeNodeOpen);
            }
        }

        private void DrawBranchTreeNode(BranchNode branchNode)
        {
            if (branchNode.Children != null && branchNode.Children.Count > 0)
            {
                if (ImGui.TreeNode(branchNode.Name))
                {
                    foreach (var item in branchNode.Children)
                    {
                        DrawBranchTreeNode(item);
                    }
                    ImGui.TreePop();
                }
            }
            else
            {
                Vector2 textSize = ImGui.CalcTextSize(branchNode.Name);
                uint textColor = ImGui.GetColorU32(ImGuiCol.Text);
                if (branchNode.Branch.IsCurrentRepositoryHead)
                {
                    textColor= ImGui.GetColorU32(ImGuiCol.HeaderActive);
                }
                ImGui.TextColored(ImGui.ColorConvertU32ToFloat4(textColor), $"\t{branchNode.Name}");
            }


            var pos = ImGui.GetItemRectMax();
            pos.Y -= 15;

            if (branchNode.BehindBy > 0)
            {
                string showTipText = $"{Icon.Get(Icon.Material_arrow_downward)}{branchNode.BehindBy}";
                var textSize = ImGui.CalcTextSize(showTipText);
                ImGui.GetWindowDrawList().AddText(pos, ImGui.GetColorU32(ImGuiCol.Text), showTipText);
                pos.X += textSize.X;
            }

            if (branchNode.AheadBy > 0)
            {
                string showTipText = $"{Icon.Get(Icon.Material_arrow_upward)}{branchNode.AheadBy}";
                //Vector2 textSize = ImGui.CalcTextSize(showTipText);
                ImGui.GetWindowDrawList().AddText(pos, ImGui.GetColorU32(ImGuiCol.Text), showTipText);
            }
        }

        private void OnRepoContentDraw()
        {
            if (_workSpaceRadio == WorkSpaceRadio.CommitHistory)
            {
                OnDrawCommitHistory();
            }
            else
            {
                OnDrawWorkTree();
            }
        }

        private void OnDrawWorkTree()
        {
            _workTreeView.OnDraw(_git,_git.CurrentStatuses, _git.Diff);
        }

        private void OnDrawCommitHistory()
        {
            var historyCommits = _git.HistoryCommits;
            if (historyCommits == null)
                return;

            if (_selectCommit != null)
            {
                _contentSplitView.Begin();
            }

            int commitMax = historyCommits.Count();
            if (_lastCommitScrollY <= 0.0f)
            {
                //float moveInterval = GetScrollInterval(_commitViewIndex - _commitAddInterval >= 0 ? _commitAddInterval : _commitViewIndex - _commitAddInterval);
                _commitViewIndex -= _commitAddInterval;
                _commitViewIndex = Math.Max(_commitViewIndex, 0);
                if (_commitViewIndex > 0)
                    ImGui.SetScrollY(GetScrollInterval(_commitAddInterval));
            }
            else if (_lastCommitScrollY >= ImGui.GetScrollMaxY())
            {
                if (commitMax >= _commitViewMax)
                {
                    _commitViewIndex += _commitAddInterval;
                    commitMax = commitMax - _commitViewMax;
                    _commitViewIndex = Math.Min(_commitViewIndex, commitMax);
                }
                else
                {
                    _commitViewIndex = 0;
                }

                if (_commitViewIndex >0 && _commitViewIndex < commitMax)
                    ImGui.SetScrollY(ImGui.GetScrollMaxY()-GetScrollInterval(_commitAddInterval));
            }
            _lastCommitScrollY = ImGui.GetScrollY();

            if (ImGui.BeginTable("GitRepo-Commits", 4, ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders | ImGuiTableFlags.Resizable | ImGuiTableFlags.Reorderable))
            {
                ImGui.TableSetupColumn("Description", ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableSetupColumn("Date", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("Author", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("Commit", ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoResize);
                ImGui.TableHeadersRow();
                int index = 0;

                foreach (var item in historyCommits)
                {
                    index++;
                    if (index < _commitViewIndex)
                        continue;
                    else if (index >= _commitViewIndex + _commitViewMax)
                        break;

                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.Text(item.MessageShort);
                    var rectMin = ImGui.GetItemRectMin();
                    var rectMax = ImGui.GetItemRectMax();
                    rectMax.X = rectMin.X+ImGui.GetColumnWidth();
                    if (ImGui.IsMouseHoveringRect(rectMin, rectMax))
                    {
                        ImGui.TableSetBgColor(ImGuiTableBgTarget.RowBg0, ImGui.GetColorU32(ImGuiCol.TabActive));
                        ImGui.TableSetBgColor(ImGuiTableBgTarget.RowBg1, ImGui.GetColorU32(ImGuiCol.TabActive));
                        if (ImGui.IsMouseDown(ImGuiMouseButton.Left))
                        {
                            _selectCommit = item;
                        }
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

            if (_selectCommit != null)
            {
                _contentSplitView.Separate();
                OnDrawSelectCommit(_selectCommit);
                _contentSplitView.End();
            }
        }

        private void OnDrawSelectCommit(Commit commit)
        {
            _showCommitView.DrawSelectCommit(commit);
        }

        private float GetScrollInterval(float size)
        {
            return ImGui.GetScrollMaxY() * (size / _commitViewMax);
        }


        private void DrawFetchWindow()
        {
            var center = ImGui.GetMainViewport().GetCenter();
            ImGui.SetNextWindowPos(center, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));
            if (ImGui.BeginPopupModal("Fetch",ref _showFetch, ImGuiWindowFlags.AlwaysAutoResize))
            {
                if (ImGui.Combo("Remote", ref _remoteBranchTrack.RemoteIndex, _remoteBranchTrack.Remotes, _remoteBranchTrack.Remotes.Length))
                {
                    _remoteBranchTrack.UpdateRemote(_git);
                }
                if (!string.IsNullOrEmpty(_remoteBranchTrack.RemoteUrl))
                {
                    ImGui.TextDisabled(_remoteBranchTrack.RemoteUrl);
                }

                
                if (ImGui.Combo("Remote Branch", ref _remoteBranchTrack.RemoteBranchIndex, _remoteBranchTrack.RemoteBranchs, _remoteBranchTrack.RemoteBranchs.Length))
                {
                }

                if (ImGui.Combo("Local Branch", ref _remoteBranchTrack.LocalBranchIndex, _remoteBranchTrack.LocalBranchs, _remoteBranchTrack.LocalBranchs.Length))
                {
                    _remoteBranchTrack.UpdateRemoteBranchIndex();
                }

                ImGui.Separator();

                Vector2 buttonSize = new Vector2(120,0);
                if (ImGui.Button("Fetch", buttonSize)) 
                {
                    if (_remoteBranchTrack.Check())
                    {
                        ImGui.CloseCurrentPopup();
                        Terminal.Fetch(_git.RepoRootPath, _remoteBranchTrack.Remote, _remoteBranchTrack.RemoteBranch, _remoteBranchTrack.LocalBranch);
                    }
                }
                ImGui.SetItemDefaultFocus();
                ImGui.SameLine();
                if (ImGui.Button("Cancel", buttonSize)) { ImGui.CloseCurrentPopup(); }

                ImGui.EndPopup();
            }
        }


        private void DrawPushWindow()
        {
            var center = ImGui.GetMainViewport().GetCenter();
            ImGui.SetNextWindowPos(center, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));
            if (ImGui.BeginPopupModal("Push", ref _showFetch, ImGuiWindowFlags.AlwaysAutoResize))
            {
                if (ImGui.Combo("Remote", ref _remoteBranchTrack.RemoteIndex, _remoteBranchTrack.Remotes, _remoteBranchTrack.Remotes.Length))
                {
                    _remoteBranchTrack.UpdateRemote(_git);
                }
                if (!string.IsNullOrEmpty(_remoteBranchTrack.RemoteUrl))
                {
                    ImGui.TextDisabled(_remoteBranchTrack.RemoteUrl);
                }


                if (ImGui.Combo("Remote Branch", ref _remoteBranchTrack.RemoteBranchIndex, _remoteBranchTrack.RemoteBranchs, _remoteBranchTrack.RemoteBranchs.Length))
                {
                }

                if (ImGui.Combo("Local Branch", ref _remoteBranchTrack.LocalBranchIndex, _remoteBranchTrack.LocalBranchs, _remoteBranchTrack.LocalBranchs.Length))
                {
                    _remoteBranchTrack.UpdateRemoteBranchIndex();
                }

                ImGui.Separator();

                if (ImGui.Checkbox("Force", ref _forcePush))
                {

                }

                ImGui.SameLine();
                Vector2 buttonSize = new Vector2(120, 0);
                if (ImGui.Button("Push", buttonSize))
                {
                    if (_remoteBranchTrack.Check())
                    {
                        ImGui.CloseCurrentPopup();
                        Terminal.Push(_git.RepoRootPath, _remoteBranchTrack.Remote, _remoteBranchTrack.LocalBranch,_remoteBranchTrack.RemoteBranch,_forcePush);
                    }
                }
                ImGui.SetItemDefaultFocus();
                ImGui.SameLine();
                if (ImGui.Button("Cancel", buttonSize)) { ImGui.CloseCurrentPopup(); }

                if (_forcePush)
                {
                    ImGui.TextColored(new Vector4(1,0.992f,0.3333f,1),"A forced push overwrites the remote repository and is irreversible!");
                }

                ImGui.EndPopup();
            }
        }
    }
}
