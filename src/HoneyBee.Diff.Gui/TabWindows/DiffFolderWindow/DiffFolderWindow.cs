using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
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


        private unsafe void ShowItemColumns(DiffFolderNode node,ref string selectPath)
        {
            ImGui.TableNextRow();

            if (node.Status != DiffNodeStatus.Same)
            {
                Vector4 rowBgColor;
                switch (node.Status)
                {
                    case DiffNodeStatus.Same:
                        break;
                    case DiffNodeStatus.Add:
                        rowBgColor = new Vector4(0.8f, 1, 0.8f, 1);
                        ImGui.TableSetBgColor(ImGuiTableBgTarget.RowBg0, ImGui.GetColorU32(rowBgColor));
                        break;
                    case DiffNodeStatus.Delete:
                        break;
                    case DiffNodeStatus.Modified:
                        rowBgColor = new Vector4(1, 0.8f, 0.8f, 1);
                        ImGui.TableSetBgColor(ImGuiTableBgTarget.RowBg0, ImGui.GetColorU32(rowBgColor));
                        break;
                    default:
                        break;
                }
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
                openFolder = ImGui.TreeNodeEx(itemName, flag);
            }
            else
            {
                ImGui.TreeNodeEx(itemName, flag | ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.Bullet | ImGuiTreeNodeFlags.NoTreePushOnOpen );
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
            throw new NotImplementedException();
        }

        public void Dispose()
        {
        }
    }
}
