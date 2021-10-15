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
        int _styleIndex = 1;

        public MainWindow()
        {
            _folderWindow = new DiffFolderWindow();

            _styleIndex = UserSettings.GetInt("StyleColors",1);
            SetStyleColors();
        }
        
        public void OnDraw()
        {
            ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 3.0f);

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

                        if (ImGui.BeginMenu("Style"))
                        {
                            var styleIndex = _styleIndex;
                            if (ImGui.MenuItem("Light", "", _styleIndex == 0))
                            {
                                styleIndex = 0;
                            }
                            if (ImGui.MenuItem("Drak", "", _styleIndex == 1))
                            {
                                styleIndex = 1;
                            }
                            if (ImGui.MenuItem("Classic", "", _styleIndex == 2))
                            {
                                styleIndex = 2;
                            }
                            if (styleIndex != _styleIndex)
                            {
                                _styleIndex = styleIndex;
                                UserSettings.SetInt("StyleColors", _styleIndex);
                                SetStyleColors();
                            }
                            ImGui.EndMenu();
                        }
                        ImGui.Separator();
                        ImGui.MenuItem("xxx");
                    }
                    ImGui.EndMenu();

                }
                ImGui.EndMainMenuBar();

                Vector2 contentSize = viewport.WorkSize;
                contentSize.Y -= 20;
                Vector2 contentPos = new Vector2(0,20);

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

        //设置
        private void SetStyleColors()
        {
            switch (_styleIndex)
            {
                case 0:
                    ImGui.StyleColorsLight();
                    break;
                case 1:
                    ImGui.StyleColorsDark();
                    break;
                case 2:
                    ImGui.StyleColorsClassic();
                    break;
            }
        }
    }
}
