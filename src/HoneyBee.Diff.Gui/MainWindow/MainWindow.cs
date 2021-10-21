using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;

namespace HoneyBee.Diff.Gui
{
    public class MainWindow : IDisposable
    {
        [Import]
        public IUserSettingsModel userSettings { get; set; }

        [Import]
        public IMainWindowModel mainModel { get; set; }

        public MainWindow()
        {
            //Ioc entrance.
            DiffProgram.ComposeParts(this);

            //logic...
            SetStyleColors();

            mainModel.CreateTab<HomeTabWindow>();
        }

        public void OnDraw()
        {
            ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 3.0f);

            var viewport = ImGui.GetMainViewport();
            ImGui.SetNextWindowPos(viewport.WorkPos);
            ImGui.SetNextWindowSize(viewport.WorkSize);
            ImGui.SetNextWindowViewport(viewport.ID);
          
            if (ImGui.Begin("Diff", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize))
            {
                if (ImGui.BeginMainMenuBar())
                {
                    if (ImGui.BeginMenu("File"))
                    {
                        if (ImGui.BeginMenu("新建"))
                        {
                            if (ImGui.MenuItem("文件夹比较"))
                            {
                                mainModel.CreateTab<DiffFolderWindow>();
                            }
                            if (ImGui.MenuItem("文件比较"))
                            {
                                mainModel.CreateTab<DiffFileWindow>();
                            }
                            if (ImGui.MenuItem("Git仓库"))
                            {
                            }
                            ImGui.EndMenu();
                        }

                        if (ImGui.BeginMenu("Style"))
                        {
                            var styleIndex = userSettings.StyleColors;
                            if (ImGui.MenuItem("Light", "", styleIndex == 0))
                            {
                                styleIndex = 0;
                            }
                            if (ImGui.MenuItem("Drak", "", styleIndex == 1))
                            {
                                styleIndex = 1;
                            }
                            if (ImGui.MenuItem("Classic", "", styleIndex == 2))
                            {
                                styleIndex = 2;
                            }
                            if (styleIndex != userSettings.StyleColors)
                            {
                                userSettings.StyleColors = styleIndex;
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
                    //_folderWindow?.OnDraw();
                    var tabWindows = mainModel.TabWindows;
                    if (tabWindows.Count > 0)
                    {
                        if (ImGui.BeginTabBar("Diff window tabs", ImGuiTabBarFlags.FittingPolicyDefault|ImGuiTabBarFlags.TabListPopupButton|ImGuiTabBarFlags.AutoSelectNewTabs))
                        {
                            for (int i = 0; i < tabWindows.Count; i++)
                            {
                                var tabWindow = tabWindows[i];
                                bool showTab = true;
                                bool visible = ImGui.BeginTabItem(tabWindow.Name,ref showTab,ImGuiTabItemFlags.Trailing);
                                if (visible)
                                {
                                    tabWindow.OnDraw();
                                    ImGui.EndTabItem();
                                }
                                if (!showTab)
                                {
                                    mainModel.RemoveTab(i);
                                    i--;
                                }
                            }
                            ImGui.EndTabBar();
                        }
                    }
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
            switch (userSettings.StyleColors)
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

            TextEditor.SetStyle(userSettings.StyleColors);
        }
    }
}
