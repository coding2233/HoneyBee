using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace HoneyBee.Diff.Gui
{
    public class MainWindow:IDisposable
    {
        private DiffFolderWindow _folderWindow;

        public MainWindow()
        {
            _folderWindow = new DiffFolderWindow();
        }

        public void OnDraw()
        {
            var viewport= ImGui.GetMainViewport();
            ImGui.SetNextWindowPos(viewport.WorkPos);
            ImGui.SetNextWindowSize(viewport.WorkSize);
            ImGui.SetNextWindowViewport(viewport.ID);

            if (ImGui.Begin("Diff", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize))
            {
                if (ImGui.BeginMainMenuBar())
                {
                    if (ImGui.BeginMenu("File"))
                    {
                        ImGui.MenuItem("中文测试");
                        ImGui.MenuItem("xxx");
                        ImGui.Separator();
                        ImGui.MenuItem("xxx");
                    }
                    ImGui.EndMenu();

                }
                ImGui.EndMainMenuBar();

                var itemSize = ImGui.GetItemRectSize();
                Vector2 contentSize = viewport.WorkSize;
                contentSize.Y -= itemSize.Y;
                Vector2 contentPos = new Vector2(0, itemSize.Y);

                ImGui.SetNextWindowPos(contentPos);
                ImGui.SetNextWindowSize(contentSize);
                if (ImGui.Begin("Compare", ImGuiWindowFlags.NoResize|ImGuiWindowFlags.NoCollapse|ImGuiWindowFlags.NoTitleBar))
                {
                    _folderWindow?.OnDraw();
                }
                ImGui.End();
            }
            ImGui.End();
        }

        public void Dispose()
        {
        }
    }
}
