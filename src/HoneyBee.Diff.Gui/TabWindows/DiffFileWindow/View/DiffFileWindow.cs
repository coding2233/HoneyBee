using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace HoneyBee.Diff.Gui
{
    public class DiffFileWindow : ITabWindow
    {
        private string _name;
        public string Name
        {
            get
            {
                if (string.IsNullOrEmpty(_name))
                {
                    _name = "Diff File Window - " + Guid.NewGuid().ToString().Substring(0, 6);
                }
                return _name;
            }
        }


        public void OnDraw()
        {
            if (ImGui.BeginChild("Left", new Vector2(ImGui.GetContentRegionAvail().X * 0.5f, 0), true, ImGuiWindowFlags.HorizontalScrollbar))
            {
                //ImGui.InputText("", _leftDiffFolder.PathBuffer, (uint)_leftDiffFolder.PathBuffer.Length);
                ImGui.SameLine();
                if (ImGui.Button("Select"))
                {
                }
                ImGui.SameLine();
                if (ImGui.Button("OK"))
                {
                    //Compare();
                }

                //OnDrawItem(_leftDiffFolder);
            }
            ImGui.EndChild();

            ImGui.SameLine();

            //ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, 5.0f);
            if (ImGui.BeginChild("Right", new Vector2(0, 0), true))
            {
                //ImGui.InputText("", _rightDiffFolder.PathBuffer, (uint)_rightDiffFolder.PathBuffer.Length);
                ImGui.SameLine();
                if (ImGui.Button("Select"))
                {
                }
                ImGui.SameLine();
                if (ImGui.Button("OK"))
                {
                    //Compare();
                }
                //OnDrawItem(_rightDiffFolder);
            }
            ImGui.EndChild();
        }
    }
}
