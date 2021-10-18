using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;

namespace HoneyBee.Diff.Gui
{
    public class DiffFolderWindow
    {
        private DiffFolder _leftDiffFolder;
        private DiffFolder _rightDiffFolder;

        private bool _showCompare = false;
 

        public DiffFolderWindow()
        {
            _leftDiffFolder = new DiffFolder();
            _rightDiffFolder = new DiffFolder();
        }

        public void OnDraw()
        {
            if (ImGui.BeginChild("Left",new Vector2(ImGui.GetContentRegionAvail().X*0.5f,0),true,ImGuiWindowFlags.HorizontalScrollbar))
            {
                ImGui.InputText("", _leftDiffFolder.PathBuffer,(uint)_leftDiffFolder.PathBuffer.Length);
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
                ImGui.InputText("", _rightDiffFolder.PathBuffer, (uint)_rightDiffFolder.PathBuffer.Length);
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
        }

        protected void OnDrawItem(DiffFolder diffFolde)
        {
            if (ImGui.BeginTable("DiffFolderTable", 3, ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders|ImGuiTableFlags.Resizable|ImGuiTableFlags.Reorderable))
            {
                ImGui.TableSetupColumn("名称", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("大小", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("修改时间", ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableHeadersRow();

                if (_showCompare)
                {
                    for (int i = 0; i < diffFolde.DiffNode.ChildrenNodes.Count; i++)
                    {
                        var item = diffFolde.DiffNode.ChildrenNodes[i];
                        ShowItemColumns(item);
                        //ImGui.TableNextRow();

                        //ImGui.TableSetColumnIndex(0);
                        //string itemName = item.IsEmpty?"---":item.Name;
                        ////if (ImGui.Selectable(itemName, i == diffFolde.SelectIndex, ImGuiSelectableFlags.SpanAllColumns | ImGuiSelectableFlags.AllowItemOverlap))
                        ////{
                        ////    diffFolde.SelectIndex = i;
                        ////}
                        //bool open = ImGui.TreeNodeEx(itemName, ImGuiTreeNodeFlags.SpanFullWidth);
                        //if (open)
                        //{

                        //}
                        //ImGui.TableSetColumnIndex(1);
                        //ImGui.Text(item.Size.ToString());
                        //ImGui.TableSetColumnIndex(2);
                        //ImGui.Text(item.UpdateTime);
                    }
                  
                }
                ImGui.EndTable();
            }
        }


        private void ShowItemColumns(DiffFolderNode node)
        {
            ImGui.TableNextRow();

            ImGui.TableSetColumnIndex(0);
            string itemName = node.IsEmpty ? "---" : node.Name;
          
            bool openFolder = !node.IsEmpty && node.IsFolder;
            if (openFolder)
            {
                openFolder = ImGui.TreeNodeEx(itemName, ImGuiTreeNodeFlags.SpanFullWidth);
            }
            else
            {
                ImGui.TreeNodeEx(itemName, ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.Bullet | ImGuiTreeNodeFlags.NoTreePushOnOpen | ImGuiTreeNodeFlags.SpanFullWidth);
            }
            ImGui.TableSetColumnIndex(1);
            ImGui.Text(node.SizeString);
            ImGui.TableSetColumnIndex(2);
            ImGui.Text(node.UpdateTime);

            if (openFolder)
            {
                foreach (var item in node.ChildrenNodes)
                {
                    ShowItemColumns(item);
                }
                ImGui.TreePop();
            }

        }
    

        private async void Compare()
        {
            Console.WriteLine(_leftDiffFolder.Path+"\n"+ _rightDiffFolder.Path);
            ImGui.OpenPopup("Compare wait");
            await Task.Run( () => {
                _showCompare = _leftDiffFolder.GetDiffFlag(_rightDiffFolder);
             });
            ImGui.CloseCurrentPopup();
        }

    }
}
