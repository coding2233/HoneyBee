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

        [Import]
        public IUserSettingsModel userSettingsModel { get; set; }

        private Dictionary<string, int> _toolItems;
        private string _toolItemSelected = "";

        public ShowGitRepoView()
        {
            DiffProgram.ComposeParts(this);

            _toolItems = new Dictionary<string, int>();
            _toolItems.Add("Commit", Icon.Material_add);
            _toolItems.Add("Pull", Icon.Material_sync);
            _toolItems.Add("Fetch", Icon.Material_download);
            _toolItems.Add("Settings", Icon.Material_settings);
            _toolItems.Add("Git Bash", Icon.Material_terminal);
            _toolItemSelected = "Commit";
        }

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
            _splitView.Begin();
            OnRepoKeysDraw();
            _splitView.Separate();
            OnRepoContentDraw();
            _splitView.End();
        }


        private void OnClickToolbar(string item)
        {
            if (item.Equals("Terminal"))
            {
                try
                {
                    string repoPath = RepoPath.EndsWith(".git") ? Path.GetDirectoryName(RepoPath) : RepoPath;
                    Process.Start(@"git-bash.exe", $"--cd={repoPath}");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Terminal exception: {e}");
                }
            }
        }

        private void OnRepoKeysDraw()
        {
            DrawTreeNodeHead("Workspace", () => {
                if (ImGui.RadioButton("Work tree", _workSpaceRadio == WorkSpaceRadio.WorkTree))
                {
                    _workSpaceRadio = WorkSpaceRadio.WorkTree;
                    _currentStatuses = _repository.RetrieveStatus();
                }

                if (ImGui.RadioButton("Commit history", _workSpaceRadio == WorkSpaceRadio.CommitHistory))
                {
                    _workSpaceRadio = WorkSpaceRadio.CommitHistory;
                }
            });

            DrawTreeNodeHead("Branch", () => {
                foreach (var item in _localBranchNodes)
                {
                    DrawBranchTreeNode(item);
                }
            });

            DrawTreeNodeHead("Tag", () => {
                foreach (var item in _repository.Tags)
                {
                    ImGui.Button($"{item.FriendlyName}");
                }
            });

            DrawTreeNodeHead("Remote", () => {
                foreach (var item in _remoteBranchNodes)
                {
                    DrawBranchTreeNode(item);
                }
            });

            DrawTreeNodeHead("Submodule", () => {
                foreach (var item in _repository.Submodules)
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
    }
}
