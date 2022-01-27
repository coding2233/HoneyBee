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
        public class BranchNode
        {
            public string Name;
            public string FullName;
            public Branch Branch;
            public List<BranchNode> Children;
        }

        public enum WorkSpaceRadio
        {
            WorkTree,
            CommitHistory,
        }

        private string _repoName ="";
        private Repository _repository;
        private RepositoryStatus _currentStatuses;
        private WorkSpaceRadio _workSpaceRadio= WorkSpaceRadio.WorkTree;
        private int _commitAddInterval = 5;
        private int _commitViewIndex = 0;
        private int _commitViewMax = 100;
        private float _lastCommitScrollY = 0.0f;
        private SplitView _splitView = new SplitView(SplitView.SplitType.Horizontal,2,200);
        private SplitView _contentSplitView = new SplitView(SplitView.SplitType.Vertical,2,600,0.9f);
        private ShowCommitView _showCommitView = new ShowCommitView();
        private WorkTreeView _workTreeView = new WorkTreeView();
        private Commit _selectCommit=null;

        private List<BranchNode> _localBranchNodes;
        private List<BranchNode> _remoteBranchNodes;

        public void SetRepoPath(string repoPath)
        {
            RepoPath = repoPath;
            if (!string.IsNullOrEmpty(RepoPath))
            {
                _repoName = Path.GetFileNameWithoutExtension(RepoPath);
                _repository = new Repository(RepoPath);
                //Set branch nodes.
                _localBranchNodes = new List<BranchNode>();
                _remoteBranchNodes = new List<BranchNode>();
                Task.Run(() =>
                {
                    foreach (var branch in _repository.Branches)
                    {
                        string[] nameArgs = branch.FriendlyName.Split('/');
                        Queue<string> nameTree = new Queue<string>();
                        foreach (var item in nameArgs)
                        {
                            nameTree.Enqueue(item);
                        }
                        if (branch.IsRemote)
                        {
                            JointBranchNode(_remoteBranchNodes, nameTree, branch);
                        }
                        else
                        {
                            JointBranchNode(_localBranchNodes, nameTree, branch);
                        }
                    }
                    //RetrieveStatus
                    _currentStatuses = _repository.RetrieveStatus();
                });
                
            }
        }

        private void JointBranchNode(List<BranchNode> branchNodes,Queue<string> nameTree,Branch branch)
        {
            if (nameTree.Count == 1)
            {
                BranchNode branchNode = new BranchNode();
                branchNode.Name = nameTree.Dequeue();
                branchNode.FullName = branch.FriendlyName;
                branchNode.Branch = branch;
                branchNodes.Add(branchNode);
            }
            else
            {
                string name = nameTree.Dequeue();
                var findNode = branchNodes.Find(x => x.Name.Equals(name));
                if (findNode == null)
                {
                    findNode = new BranchNode();
                    findNode.Name = name;
                    findNode.Children = new List<BranchNode>();
                    branchNodes.Add(findNode);
                }
                JointBranchNode(findNode.Children, nameTree, branch);
            }
        }

        protected override void OnToolbarDraw()
        {
            DrawToolItem(Icon.Get(Icon.Material_add), "Commit", true);
            ImGui.SameLine();
            DrawToolItem(Icon.Get(Icon.Material_sync), "Pull", false);
            ImGui.SameLine();
            DrawToolItem(Icon.Get(Icon.Material_download), "Fetch", false);
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
            ImGui.SetNextItemOpen(true);
            if (ImGui.TreeNode("Workspace"))
            {
                if (ImGui.RadioButton("Work tree", _workSpaceRadio==WorkSpaceRadio.WorkTree))
                {
                    _workSpaceRadio = WorkSpaceRadio.WorkTree;
                   _currentStatuses=  _repository.RetrieveStatus();
                }

                if (ImGui.RadioButton("Commit history", _workSpaceRadio==WorkSpaceRadio.CommitHistory))
                {
                    _workSpaceRadio = WorkSpaceRadio.CommitHistory;
                }
                ImGui.TreePop();
            }

            if (ImGui.TreeNode("Branch"))
            {
                foreach (var item in _localBranchNodes)
                {
                    DrawBranchTreeNode(item);
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
                foreach (var item in _remoteBranchNodes)
                {
                    DrawBranchTreeNode(item);
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
                //if (ImGui.IsMouseHoveringRect(ImGui.GetCursorPos(), ImGui.GetCursorPos() + textSize))
                //{
                //    textColor = ImGui.GetColorU32(ImGuiCol.HeaderActive);
                //}
                if (branchNode.Branch.IsCurrentRepositoryHead)
                {
                    textColor= ImGui.GetColorU32(ImGuiCol.HeaderActive);
                }
                ImGui.TextColored(ImGui.ColorConvertU32ToFloat4(textColor), $"\t{branchNode.Name}");
                if (branchNode.Branch.IsTracking)
                {
                    var pos = ImGui.GetItemRectMax();
                    pos.Y -= 15;

                    var trackingDetails = branchNode.Branch.TrackingDetails;
                    if (trackingDetails.BehindBy > 0)
                    {
                        string showTipText = $"{Icon.Get(Icon.Material_arrow_downward)}{trackingDetails.BehindBy}";
                        textSize = ImGui.CalcTextSize(showTipText);
                        ImGui.GetWindowDrawList().AddText(pos, ImGui.GetColorU32(ImGuiCol.Text), showTipText);
                        pos.X += textSize.X;
                    }

                    if (trackingDetails.AheadBy > 0)
                    {
                        string showTipText = $"{Icon.Get(Icon.Material_arrow_upward)}{trackingDetails.AheadBy}";
                        //Vector2 textSize = ImGui.CalcTextSize(showTipText);
                        ImGui.GetWindowDrawList().AddText(pos, ImGui.GetColorU32(ImGuiCol.Text), showTipText);
                    }
                }
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
                _workTreeView.OnDraw(_currentStatuses, _repository.Diff);
        }

        private void OnDrawCommitHistory()
        {
            if (_selectCommit != null)
            {
                _contentSplitView.Begin();
            }

            int commitMax = _repository.Commits.Count();
            if (_lastCommitScrollY <= 0.0f)
            {
                _commitViewIndex -= _commitAddInterval;
                _commitViewIndex = Math.Max(_commitViewIndex, 0);
                if (_commitViewIndex > 0)
                    ImGui.SetScrollY(10);
            }
            else if (_lastCommitScrollY >= ImGui.GetScrollMaxY())
            {
                _commitViewIndex += _commitAddInterval;
                if (commitMax > _commitViewMax)
                {
                    commitMax = commitMax - _commitViewMax;
                }
                _commitViewIndex = Math.Min(_commitViewIndex, commitMax);
                if (_commitViewIndex < commitMax)
                    ImGui.SetScrollY(_lastCommitScrollY - 1);
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
    }
}
