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
    public class DiffFolderWindow: ITabWindow
    {
        private DiffFolder _leftDiffFolder;
        private DiffFolder _rightDiffFolder;

        private bool _showCompare = false;
        private bool _prepare = false;

        private string _name;
        public string Name 
        {
            get
            {
                if (string.IsNullOrEmpty(_name))
                {
                    _name = "Diff Folder Window - " + Guid.NewGuid().ToString().Substring(0,6);
                }
                return _name;
            }
        }

        [Import]
        public IMainWindowModel mainModel { get; set; }

        public DiffFolderWindow()
        {
            DiffProgram.ComposeParts(this);

            _leftDiffFolder = new DiffFolder();
            _rightDiffFolder = new DiffFolder();

            _leftDiffFolder.FolderPath = @"C:\Users\EDY\Desktop\fazai1013\Scripts\FazaiUI";
            _rightDiffFolder.FolderPath = @"E:\source\HappyMahjongForDeveloper\HappyMahjongForArtist\Assets\Scripts\FazaiUI";
        }

        public void OnDraw()
        {
            if (ImGui.BeginChild("Left",new Vector2(ImGui.GetContentRegionAvail().X*0.5f,0),true,ImGuiWindowFlags.HorizontalScrollbar))
            {
                ImGui.InputText("",ref _leftDiffFolder.FolderPath,500);
                ImGui.SameLine();
                if (ImGui.Button("Select"))
                { 
                }
                ImGui.SameLine();
                if (ImGui.Button("OK"))
                {
                    Compare();
                }
                OnDrawItem(_leftDiffFolder);
            }
            ImGui.EndChild();

            ImGui.SameLine();

            //ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, 5.0f);
            if (ImGui.BeginChild("Right",new Vector2(0,0),true))
            {
                ImGui.InputText("",ref _rightDiffFolder.FolderPath, 500);
                ImGui.SameLine();
                if (ImGui.Button("Select"))
                {
                }
                ImGui.SameLine();
                if (ImGui.Button("OK"))
                {
                    Compare();
                }
                OnDrawItem(_rightDiffFolder);
            }
            ImGui.EndChild();

            //var center = ImGui.GetMainViewport().GetCenter();
            //ImGui.SetNextWindowPos(center, ImGuiCond.Appearing,Vector2.One*0.5f);
            //if (ImGui.BeginPopupModal("Delete"))
            //{
            //    ImGui.Text("All those beautiful files will be deleted.\nThis operation cannot be undone!\n\n");
            //    ImGui.Separator();

            //    //static int unused_i = 0;
            //    //ImGui::Combo("Combo", &unused_i, "Delete\0Delete harder\0");

            //    ImGui.EndPopup();
            //}

        }

        protected void OnDrawItem(DiffFolder diffFolde)
        {
            if (ImGui.BeginTable("DiffFolderTable", 3, ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders|ImGuiTableFlags.Resizable|ImGuiTableFlags.Reorderable))
            {
                ImGui.TableSetupColumn("名称", ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableSetupColumn("大小", ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoResize);
                ImGui.TableSetupColumn("修改时间", ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoResize);
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


        private unsafe uint ShowItemColumns(DiffFolderNode node,ref string selectPath)
        {
            ImGui.TableNextRow();

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
                openFolder = ImGui.TreeNodeEx(itemName, flag);
            }
            else
            {
                ImGui.TreeNodeEx(itemName, flag | ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.Bullet | ImGuiTreeNodeFlags.NoTreePushOnOpen );

                if(!node.IsEmpty && ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                {
                    Console.WriteLine($"Double click. {node.FullName}");
                    string leftFilePath = Path.Combine(_leftDiffFolder.FolderPath, node.FullName);
                    string rightFilePath = Path.Combine(_rightDiffFolder.FolderPath, node.FullName);
                    //mainModel.CreateTab<DiffFileWindow>(leftFilePath, rightFilePath);


                    var diff = InlineDiffBuilder.Diff(File.ReadAllText(leftFilePath), File.ReadAllText(rightFilePath));

                    var savedColor = Console.ForegroundColor;
                    foreach (var line in diff.Lines)
                    {
                        switch (line.Type)
                        {
                            case ChangeType.Inserted:
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.Write("+ ");
                                break;
                            case ChangeType.Deleted:
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.Write("- ");
                                break;
                            default:
                                Console.ForegroundColor = ConsoleColor.Gray; // compromise for dark or light background
                                Console.Write("  ");
                                break;
                        }

                        Console.WriteLine(line.Text);
                    }
                    Console.ForegroundColor = savedColor;
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

            uint showRowBgColor = 0;
            if (openFolder)
            {
                foreach (var item in node.ChildrenNodes)
                {
                    uint childRowBgColor = ShowItemColumns(item,ref selectPath);
                    if (childRowBgColor > 0)
                    {
                        showRowBgColor = childRowBgColor;
                    }
                }
                ImGui.TreePop();
            }

            if (showRowBgColor == 0)
            {
                switch (node.Status)
                {
                    case DiffStatus.Same:
                        break;
                    case DiffStatus.Add:
                        showRowBgColor = ImGui.GetColorU32(new Vector4(0.8f, 1, 0.8f, 1));
                        break;
                    case DiffStatus.Delete:
                        break;
                    case DiffStatus.Modified:
                        showRowBgColor = ImGui.GetColorU32(new Vector4(1, 0.8f, 0.8f, 1));
                        break;
                    default:
                        break;
                }
            }

            if (showRowBgColor > 0)
            {
                ImGui.TableSetBgColor(ImGuiTableBgTarget.RowBg0, ImGui.GetColorU32(showRowBgColor));
            }

            return showRowBgColor;
        }
    

        private async void Compare()
        {
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
        }

        public void Setup(params object[] parameters)
        {
        }

        public void Dispose()
        {
        }
    }
}
