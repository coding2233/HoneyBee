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
        int _styleIndex = 1;
        private List<ITabWindow> _tabWindows;

        [Import]
        public IUserSettingsModel UserSettings { get; set; }

        public MainWindow()
        {
            //Ioc entrance.
            var catalog = new AssemblyCatalog(Assembly.GetExecutingAssembly());
            CompositionContainer container = new CompositionContainer(catalog);
            container.ComposeParts(this);

            //logic...
            _styleIndex = UserSettings.StyleColors;
            SetStyleColors();

            _tabWindows = new List<ITabWindow>();

            _tabWindows.Add(new HomeTabWindow());
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
                                var diffFolderWindow = new DiffFolderWindow();
                                if (_tabWindows.Find(x => x.Name.Equals(diffFolderWindow.Name)) == null)
                                {   
                                    _tabWindows.Add(diffFolderWindow);
                                }
                            }
                            if (ImGui.MenuItem("文件比较"))
                            {
                            }
                            if (ImGui.MenuItem("Git仓库"))
                            {
                            }
                            ImGui.EndMenu();
                        }

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
                                UserSettings.StyleColors=_styleIndex;
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
                    if (_tabWindows.Count > 0)
                    {
                        for (int i = 0; i < _tabWindows.Count; i++)
                        {
                            var findWindows = _tabWindows.FindAll(x => x.Name.Equals(_tabWindows[i].Name));
                            if (findWindows != null && findWindows.Count > 1)
                            {
                                _tabWindows.RemoveAt(i);
                                i--;
                            }
                        }

                        if (ImGui.BeginTabBar("Diff window tabs", ImGuiTabBarFlags.FittingPolicyDefault|ImGuiTabBarFlags.TabListPopupButton|ImGuiTabBarFlags.AutoSelectNewTabs))
                        {
                            for (int i = 0; i < _tabWindows.Count; i++)
                            {
                                var tabWindow = _tabWindows[i];
                                bool showTab = true;
                                bool visible = ImGui.BeginTabItem(tabWindow.Name,ref showTab,ImGuiTabItemFlags.Trailing);
                                if (visible)
                                {
                                    tabWindow.OnDraw();
                                    ImGui.EndTabItem();
                                }
                                if (!showTab)
                                {
                                    _tabWindows.RemoveAt(i);
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
