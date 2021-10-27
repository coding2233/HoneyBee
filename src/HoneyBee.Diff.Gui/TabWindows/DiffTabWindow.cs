using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace HoneyBee.Diff.Gui
{
    public abstract class DiffTabWindow : ITabWindow
    {
        private float _toolbarHeight = 35.0f;
        
        public abstract string Name { get; }

        public abstract string IconName { get; }

        private float _contentScrollY = 0;

        protected float _contentScrollYSpeed = 25.0f;

        protected bool _loading;

        public virtual void Setup(params object[] parameters)
        {
        }

        public abstract void Deserialize(string data);

        public abstract string Serialize();

        protected virtual void OnToolbarDraw()
        {
            if (ImGui.Button(Icon.Get(Icon.Material_compare) + "Compare"))
            {
                OnCompare();
            }
        }

        protected virtual void OnLeftToolbarDraw()
        {

        }

        protected virtual void OnRightToolbarDraw()
        {

        }

        protected virtual void OnLeftContentDraw()
        {

        }

        protected virtual void OnRightContentDraw()
        {

        }


        public virtual void OnDraw()
        {
            OnToolbarDraw();

            float halfWidth = ImGui.GetContentRegionAvail().X * 0.5f;
            float contentHeight = ImGui.GetContentRegionAvail().Y - _toolbarHeight;

            if (_loading)
            {
                string symbols = "|/-\\";
                int index = (int)(ImGui.GetTime() / 0.05f) & 3;
                ImGui.Text($"Loading {symbols[index]}");
            }

            if (ImGui.BeginChild($"Diff_Toolbar_{Name}", new Vector2(0, _toolbarHeight)))
            {
                if (ImGui.BeginChild($"Diff_Left_Toolbar_{Name}", new Vector2(halfWidth, 0), true))
                {
                    OnLeftToolbarDraw();
                    ImGui.EndChild();
                }
                ImGui.SameLine();
                if (ImGui.BeginChild($"Diff_Right_Toolbar_{Name}", Vector2.Zero, true))
                {
                    OnRightToolbarDraw();
                    ImGui.EndChild();
                }
                ImGui.EndChild();
            }
            if (ImGui.BeginChild($"Diff_Content_{Name}"))
            {
                float scrollMaxY = 0.0f;
                bool hoverLeftChild = false;
                bool hoverRightChild = false;

                ImGuiWindowFlags contentChildFlag = ImGuiWindowFlags.NoScrollWithMouse| ImGuiWindowFlags.NoCollapse;
                //ImGuiWindowFlags contentChildFlag = ImGuiWindowFlags.None;
                if (ImGui.BeginChild($"Diff_Left_Content_{Name}", new Vector2(halfWidth, 0), true, contentChildFlag))
                {
                    ImGui.SetScrollY(_contentScrollY);
                    OnLeftContentDraw();
                    if (ImGui.IsMouseDragging(ImGuiMouseButton.Left))
                    {
                        _contentScrollY = ImGui.GetScrollY();
                    }
                    scrollMaxY = ImGui.GetScrollMaxY();
                    ImGui.EndChild();
                }
                hoverLeftChild = ImGui.IsItemHovered();

                ImGui.SameLine();

                if(ImGui.BeginChild($"Diff_Right_Content_{Name}", Vector2.Zero, true, contentChildFlag))
                {
                    ImGui.SetScrollY(_contentScrollY);

                    OnRightContentDraw();
                    if (ImGui.IsMouseDragging(ImGuiMouseButton.Left))
                    {
                        _contentScrollY = ImGui.GetScrollY();
                    }
                    scrollMaxY = Math.Max(scrollMaxY, ImGui.GetScrollMaxY());
                    ImGui.EndChild();
                }
                hoverRightChild = ImGui.IsItemHovered();

                if (hoverLeftChild || hoverRightChild)
                {
                    var mouseWheel = ImGui.GetIO().MouseWheel;
                    if (mouseWheel != 0.0f)
                    {
                        _contentScrollY -= mouseWheel * _contentScrollYSpeed;
                        _contentScrollY = Math.Clamp(_contentScrollY, 0, scrollMaxY);
                    }
                }

                ImGui.EndChild();
            }
        }

        protected abstract void OnCompare();


        public virtual void Dispose()
        {
        }

    }
}