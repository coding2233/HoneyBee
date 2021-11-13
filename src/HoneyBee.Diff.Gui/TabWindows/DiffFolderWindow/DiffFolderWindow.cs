using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using ImGuiNET;

namespace HoneyBee.Diff.Gui
{
    public class DiffFolderWindow: DiffTabWindow
    {
        private DiffFolder _leftDiffFolder;
        private DiffFolder _rightDiffFolder;

        private bool _showCompare = false;

        private string _name;
        public override string Name 
        {
            get
            {
                if (string.IsNullOrEmpty(_name))
                {
                    _name = "Diff Folder Window - " + Guid.NewGuid().ToString().Substring(0,6);
                }
                return $"{_name}";
            }
        }

        public override string IconName => Icon.Get(Icon.Material_folder);

        //同步打开文件夹
        private readonly Dictionary<string, bool> _syncOpenFolders = new Dictionary<string, bool>();

        public DiffFolderWindow()
        {
            string userPath = Environment.GetEnvironmentVariable("USERPROFILE");
            string folderPath = $"{userPath}\\Documents";

            _leftDiffFolder = new DiffFolder();
            _rightDiffFolder = new DiffFolder();

            _leftDiffFolder.FolderPath = folderPath;
            _rightDiffFolder.FolderPath = folderPath;
        }
        protected override void OnLeftToolbarDraw()
        {
            ImGui.InputText("", ref _leftDiffFolder.FolderPath, 500);
            ImGui.SameLine();
            if (ImGui.Button((Icon.Get(Icon.Material_open_in_browser))))
            {
                string openPath = string.IsNullOrEmpty(_rightDiffFolder.FolderPath) ? "./" : _rightDiffFolder.FolderPath;
                ImGuiFileDialog.OpenFolder((selectPath) => {
                    if (!string.IsNullOrEmpty(selectPath))
                    {
                        _leftDiffFolder.FolderPath = selectPath;
                    }
                }, openPath);
            }
        }

        protected override void OnRightToolbarDraw()
        {
            ImGui.InputText("", ref _rightDiffFolder.FolderPath, 500);
            ImGui.SameLine();
            if (ImGui.Button((Icon.Get(Icon.Material_open_in_browser))))
            {
                string openPath = string.IsNullOrEmpty(_rightDiffFolder.FolderPath) ? "./" : _rightDiffFolder.FolderPath;
                ImGuiFileDialog.OpenFolder((selectPath) => {
                    if (!string.IsNullOrEmpty(selectPath))
                    {
                        _rightDiffFolder.FolderPath = selectPath;
                    }
                }, openPath);
            }
        }

        protected override void OnLeftContentDraw()
        {
            OnDrawItem(_leftDiffFolder);
        }

        protected override void OnRightContentDraw()
        {
            OnDrawItem(_rightDiffFolder);
        }

        protected void OnDrawItem(DiffFolder diffFolde)
        {
            if (ImGui.BeginTable("DiffFolderTable", 3, ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders|ImGuiTableFlags.Resizable|ImGuiTableFlags.Reorderable))
            {
                ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableSetupColumn("Size", ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoResize);
                ImGui.TableSetupColumn("Update Time", ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoResize);
                ImGui.TableHeadersRow();

                if (_showCompare)
                {
                    for (int i = 0; i < diffFolde.DiffNode.ChildrenNodes.Count; i++)
                    {
                        var item = diffFolde.DiffNode.ChildrenNodes[i];
                        ShowItemColumns(item,ref diffFolde.SelectPath);
                    }
                }
                ImGui.EndTable();
            }
        }

        private unsafe void ShowItemColumns(DiffFolderNode node,ref string selectPath)
        {
            ImGui.TableNextRow();

            if (node.ChildrenHasDiff || node.Status == DiffStatus.Modified)
            {
                ImGui.TableSetBgColor(ImGuiTableBgTarget.RowBg0, userSettings.MarkBgColor);
                ImGui.TableSetBgColor(ImGuiTableBgTarget.RowBg1, userSettings.MarkBgColor);
            }

            ImGui.TableSetColumnIndex(0);
            string itemName = node.Name;
            ImGuiTreeNodeFlags flag = ImGuiTreeNodeFlags.SpanFullWidth;
            if (!string.IsNullOrEmpty(selectPath) && selectPath.Equals(node.FullName))
            {
                flag |= ImGuiTreeNodeFlags.Selected;
            }
            bool openFolder = node.IsFolder;
            if (openFolder)
            {
                if (_syncOpenFolders.TryGetValue(node.FullName, out bool nextOpen))
                {
                    ImGui.SetNextItemOpen(nextOpen);
                }
                string folderIcon = openFolder ? Icon.Get(Icon.Material_folder_open) : Icon.Get(Icon.Material_folder) ;
                openFolder = ImGui.TreeNodeEx(folderIcon+itemName, flag);
                _syncOpenFolders[node.FullName]=openFolder;
            }
            else
            {
                ImGui.TreeNodeEx(Icon.Get(node.IsEmpty?Icon.Material_no_sim:Icon.Material_text_snippet)+itemName, flag | ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.Bullet | ImGuiTreeNodeFlags.NoTreePushOnOpen );

                if(!node.IsEmpty && ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                {
                    Console.WriteLine($"Double click. {node.FullName}");
                    string leftFilePath = Path.Combine(_leftDiffFolder.FolderPath, node.FullName);
                    string rightFilePath = Path.Combine(_rightDiffFolder.FolderPath, node.FullName);
                    mainModel.CreateTab<DiffFileWindow>(leftFilePath, rightFilePath);
                }
            }

            if (node.IsEmpty)
            {
                var rectMin = ImGui.GetItemRectMin();
                var rectMax = ImGui.GetItemRectMax();
                var rectSize = rectMax - rectMin;
                rectMax.Y= rectMin.Y += rectSize.Y * 0.5f;
                ImGui.GetWindowDrawList().AddLine(rectMin, rectMax, ImGui.GetColorU32(ImGuiCol.TextDisabled));
            }

            if (ImGui.IsItemClicked())
                selectPath = node.FullName;

            ImGui.TableSetColumnIndex(1);
            ImGui.Text(node.SizeString);
            ImGui.TableSetColumnIndex(2);
            ImGui.Text(node.UpdateTime);

            if (openFolder)
            {
                if (node.FindChildren)
                {
                    if (node.ChildrenNodes.Count > 0)
                    {
                        foreach (var item in node.ChildrenNodes)
                        {
                            ShowItemColumns(item, ref selectPath);
                        }
                        
                    }
                }
                else
                {
                    var leftNode = FindChild(node.FullName, _leftDiffFolder.DiffNode);
                    var rightNode = FindChild(node.FullName, _rightDiffFolder.DiffNode);
                    if (leftNode.IsEmpty)
                    {
                        rightNode.GetChildren();
                        rightNode.CopyToEmptyNodeWithChildren(leftNode);
                    }
                    else if (rightNode.IsEmpty)
                    {
                        leftNode.GetChildren();
                        leftNode.CopyToEmptyNodeWithChildren(rightNode);
                    }
                    else
                    {
                        leftNode.GetChildren();
                        rightNode.GetChildren();
                    }
                    CompareFolderNode(leftNode, rightNode);
                }
                ImGui.TreePop();
            }
        }

        private DiffFolderNode FindChild(string fullName, DiffFolderNode node)
        {
            foreach (var item in node.ChildrenNodes)
            {
                if (item.FullName.Equals(fullName))
                {
                    return item;
                }
                if (item.IsFolder && fullName.StartsWith($"{item.FullName}/"))
                {
                    return FindChild(fullName, item);
                }
            }
            return null;
        }
    

        protected override async void OnCompare()
        {
            try
            {
                if (_loading)
                    return;

                _loading = true;
                Console.WriteLine(_leftDiffFolder.FolderPath + "\n" + _rightDiffFolder.FolderPath);
                await Task.Run(() =>
                {
                    var leftNode = _leftDiffFolder.GetNode();
                    var rightNode = _rightDiffFolder.GetNode();
                    CompareFolderNode(leftNode, rightNode);
                    _showCompare = _leftDiffFolder.DiffNode != null && _rightDiffFolder.DiffNode != null;
                });

                if (_showCompare)
                {
                    string leftName = _leftDiffFolder.DiffNode.Name;
                    string rightName = _rightDiffFolder.DiffNode.Name;
                    _name = leftName.Equals(rightName) ? leftName : $"{leftName}/{rightName}";
                    string oldName = _name;
                    while (mainModel.HasSameWindow(_name, this))
                    {
                        _name = $"{oldName} - {Guid.NewGuid().ToString().Substring(0, 6)}";
                    }
                }
                _loading = false;
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.ToString());
                mainModel.RemoveTab(this);
            }
        }

        protected override void OnExchange()
        {
            string leftPath = _leftDiffFolder.FolderPath;
            _leftDiffFolder.FolderPath = _rightDiffFolder.FolderPath;
            _rightDiffFolder.FolderPath = leftPath;
        }

        private void CompareFolderNode(DiffFolderNode leftNode, DiffFolderNode rightNode)
        {
            Dictionary<string, DiffFolderNode> allNodes = new Dictionary<string, DiffFolderNode>();
            HashSet<string> dirNodeNames = new HashSet<string>();
            HashSet<string> fileNodeNames = new HashSet<string>();
            foreach (var item in leftNode.ChildrenNodes)
            {
                if (!item.IsEmpty)
                {
                    if (item.IsFolder)
                        dirNodeNames.Add(item.Name);
                    else
                        fileNodeNames.Add(item.Name);

                    if(!allNodes.ContainsKey(item.Name))
                        allNodes.Add(item.Name,item);
                }
            }

            foreach (var item in rightNode.ChildrenNodes)
            {
                if (!item.IsEmpty)
                {
                    if (item.IsFolder)
                        dirNodeNames.Add(item.Name);
                    else
                        fileNodeNames.Add(item.Name);
                }
                if (!allNodes.ContainsKey(item.Name))
                    allNodes.Add(item.Name,item);
            }
            var dirList = dirNodeNames.ToList();
            dirList.Sort();
            var fileList = fileNodeNames.ToList();
            fileList.Sort();

            List<string> nodeNames = new List<string>();
            nodeNames.AddRange(dirList);
            nodeNames.AddRange(fileList);

            for (int i = 0; i < nodeNames.Count; i++)
            {
                var item = nodeNames[i];
                if (i < leftNode.ChildrenNodes.Count)
                {
                    var child = leftNode.ChildrenNodes[i];
                    if (!item.Equals(child.Name))
                    {
                        leftNode.ChildrenNodes.Insert(i, allNodes[item].CopyToEmptyNode(leftNode));
                    }
                }
                else
                {
                    leftNode.ChildrenNodes.Add(allNodes[item].CopyToEmptyNode(leftNode));
                }

                if (i < rightNode.ChildrenNodes.Count)
                {
                    var child = rightNode.ChildrenNodes[i];
                    if (!item.Equals(child.Name))
                    {
                        rightNode.ChildrenNodes.Insert(i, allNodes[item].CopyToEmptyNode(rightNode));
                    }
                }
                else
                {
                    rightNode.ChildrenNodes.Add(allNodes[item].CopyToEmptyNode(rightNode));
                }
            }

            for (int i = 0; i < nodeNames.Count; i++)
            {
                var a = leftNode.ChildrenNodes[i];
                var b = rightNode.ChildrenNodes[i];
                if (!a.IsEmpty && !b.IsEmpty)
                {
                    a.Status = b.Status = a.MD5.Equals(b.MD5) ? DiffStatus.Same : DiffStatus.Modified;
                }
                else
                {
                    a.Status = a.IsEmpty ? DiffStatus.Delete : DiffStatus.Add;
                    a.Status = b.IsEmpty ? DiffStatus.Delete : DiffStatus.Add;
                }

                if (!a.IsEmpty && a.IsFolder && !a.FindChildren)
                    a.Status = DiffStatus.Unknown;

                if (!b.IsEmpty && b.IsFolder && !b.FindChildren)
                    b.Status = DiffStatus.Unknown;
            }

            leftNode.UpdateStatus();
            rightNode.UpdateStatus();
        }

        public override string Serialize()
        {
            string path = $"{_name}|{_leftDiffFolder.FolderPath}|{_rightDiffFolder.FolderPath}";
            return path;
        }

        public override void Deserialize(string data)
        {
            string[] args = data.Split('|');
            _name = args[0];
            _leftDiffFolder.FolderPath = args[1];
            _rightDiffFolder.FolderPath = args[2];

            OnCompare();
        }


        public override void Dispose()
        {
        }
    
    }
}
