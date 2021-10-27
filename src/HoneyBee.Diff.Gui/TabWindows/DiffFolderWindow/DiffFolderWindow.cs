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

        public override string IconName => Icon.Get(Icon.Material_folder_special);

        [Import]
        public IMainWindowModel mainModel { get; set; }
        [Import]
        public IUserSettingsModel userSettings { get; set; }

        //同步打开文件夹
        private readonly Dictionary<string, bool> _syncOpenFolders = new Dictionary<string, bool>();


        public DiffFolderWindow()
        {
            DiffProgram.ComposeParts(this);

            _leftDiffFolder = new DiffFolder();
            _rightDiffFolder = new DiffFolder();
        }
        protected override void OnLeftToolbarDraw()
        {
            ImGui.InputText("", ref _leftDiffFolder.FolderPath, 500);
            ImGui.SameLine();
            if (ImGui.Button("Select"))
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
            if (ImGui.Button("Select"))
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

            if (node.ChildrenHasDiff || node.Status != DiffStatus.Same)
            {
                ImGui.TableSetBgColor(ImGuiTableBgTarget.RowBg0, userSettings.MarkBgColor);
                ImGui.TableSetBgColor(ImGuiTableBgTarget.RowBg1, userSettings.MarkBgColor);
            }

            ImGui.TableSetColumnIndex(0);
            string itemName = node.IsEmpty ? "---" : node.Name;
            ImGuiTreeNodeFlags flag = ImGuiTreeNodeFlags.SpanFullWidth;
            if (!string.IsNullOrEmpty(selectPath) && selectPath.Equals(node.FullName))
            {
                flag |= ImGuiTreeNodeFlags.Selected;
            }
            bool openFolder = !node.IsEmpty && node.IsFolder;
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
                ImGui.TreeNodeEx(Icon.Get(Icon.Material_file_copy)+itemName, flag | ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.Bullet | ImGuiTreeNodeFlags.NoTreePushOnOpen );

                if(!node.IsEmpty && ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                {
                    Console.WriteLine($"Double click. {node.FullName}");
                    string leftFilePath = Path.Combine(_leftDiffFolder.FolderPath, node.FullName);
                    string rightFilePath = Path.Combine(_rightDiffFolder.FolderPath, node.FullName);
                    mainModel.CreateTab<DiffFileWindow>(leftFilePath, rightFilePath);
                }
            }

            if (!node.IsEmpty && ImGui.IsItemClicked())
            {
                selectPath = node.FullName;
            }

            ImGui.TableSetColumnIndex(1);
            ImGui.Text(node.SizeString);
            ImGui.TableSetColumnIndex(2);
            ImGui.Text(node.UpdateTime);

            if (openFolder)
            {
                foreach (var item in node.ChildrenNodes)
                {
                    ShowItemColumns(item,ref selectPath);
                }
                ImGui.TreePop();
            }

        }
    

        protected override async void OnCompare()
        {
            mainModel.ShowLoading.Add(Name);
            Console.WriteLine(_leftDiffFolder.FolderPath+"\n"+ _rightDiffFolder.FolderPath);
            await Task.Run( () => {
                _showCompare = _leftDiffFolder.GetDiffFlag(_rightDiffFolder);
             });

            if (_showCompare)
            {
                string leftName = _leftDiffFolder.DiffNode.Name;
                string rightName = _rightDiffFolder.DiffNode.Name;
                _name = leftName.Equals(rightName)? leftName:$"{leftName}/{rightName}";
                string oldName = _name;
                while (mainModel.HasSameWindow(_name,this))
                {
                    _name = $"{oldName} - {Guid.NewGuid().ToString().Substring(0, 6)}";
                }
            }
            mainModel.ShowLoading.Remove(Name);
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
