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

                ImGui.Text("Left");
                ImGui.Text("Left");
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
                ImGui.Text("Right");
            }
            ImGui.EndChild();
        }


        protected void OnDrawLeft()
        { 

        }

        protected void OnDrawRight()
        {
            
        }

        private void Compare()
        {
            Console.WriteLine(_leftDiffFolder.Path+"\n"+ _rightDiffFolder.Path);
        }

    }
}
