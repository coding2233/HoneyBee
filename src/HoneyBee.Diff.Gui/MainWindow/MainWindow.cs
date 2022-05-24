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
using System.IO;

namespace HoneyBee.Diff.Gui
{
    public class MainWindow : IDisposable
    {
        [Import]
        public IUserSettingsModel userSettings { get; set; }

        [Import]
        public IMainWindowModel mainModel { get; set; }

        private TextStyleSettingModal _textStyleModal;

        private ImGuiWindowFlags _defaultWindowFlag;

        private bool _isLaunchWindow;

        private HashSet<int> _waitCloseTabIndexs = new HashSet<int>();

        private GlobalControl _globalControl;

        public MainWindow(bool isLaunchWindow)
        {
            _isLaunchWindow = isLaunchWindow;

            if (_isLaunchWindow)
                return;
            
            //Ioc entrance.
            DiffProgram.ComposeParts(this);


            //logic...
            SetStyleColors();

            var args = System.Environment.GetCommandLineArgs();

            //初始化
            mainModel.Init(args.Length != 3);

            _textStyleModal = new TextStyleSettingModal();
            _globalControl = new GlobalControl();

            _defaultWindowFlag = ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize
                | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse ;

            if (args.Length == 3)
            {
                string leftArg = args[1].Trim();
                string rightArg = args[2].Trim();
                if (!string.IsNullOrEmpty(leftArg) && !string.IsNullOrEmpty(rightArg))
                {
                    if (File.Exists(leftArg) || File.Exists(rightArg))
                    {
                        mainModel.CreateTab<DiffFileWindow>(leftArg, rightArg);
                    }
                    else if (Directory.Exists(leftArg) && Directory.Exists(rightArg))
                    {
                        mainModel.CreateTab<DiffFolderWindow>(leftArg, rightArg);
                    }
                }
            }

            if (mainModel.TabWindows.Count == 0)
            {
                mainModel.CreateTab<MainTabWindow>();
            }
        }

        public void OnDraw()
        {
            if (_isLaunchWindow)
            {
                var viewport = ImGui.GetMainViewport();
                ImGui.SetNextWindowPos(viewport.WorkPos);
                ImGui.SetNextWindowSize(viewport.WorkSize);
                ImGui.SetNextWindowViewport(viewport.ID);

                var workSize = viewport.WorkSize;
                if (ImGui.Begin("Diff", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize
                | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoBringToFrontOnFocus))
                {
                    int iconSize = 128;
                    float offset = ImGui.GetStyle().ItemSpacing.Y * 10;
                    ImGui.SetCursorPos(new Vector2((workSize.X - iconSize) * 0.5f, offset));
                    var tptr = DiffProgram.GetOrCreateTexture("bee.png");
                    ImGui.Image(tptr, Vector2.One * 128);

                    ImGui.SetCursorPosY(ImGui.GetCursorPosY() + offset);

                    string text = "Honeybee - Diff";
                    var textSize = ImGui.CalcTextSize(text);
                    ImGui.SetCursorPosX((workSize.X - textSize.X)*0.5f);
                    ImGui.Text(text);

                    text = "Lightweight comparison tool.";
                    textSize = ImGui.CalcTextSize(text);
                    ImGui.SetCursorPosX((workSize.X - textSize.X) * 0.5f);
                    ImGui.TextDisabled(text);

                    ImGui.End();
                }
            }
            else
            {
         
                //ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 3.0f);

                var viewport = ImGui.GetMainViewport();
                ImGui.SetNextWindowPos(viewport.WorkPos);
                ImGui.SetNextWindowSize(viewport.WorkSize);
                ImGui.SetNextWindowViewport(viewport.ID);

                bool showGlobalControl = GlobalControl.Show;

                if (showGlobalControl)
                {
                    ImGui.BeginDisabled();
                }

                var windorFlag = _defaultWindowFlag;
                if (ImGui.Begin("Diff", windorFlag))
                {
                    if (ImGui.BeginMainMenuBar())
                    {
                        if (ImGui.BeginMenu("File"))
                        {
                            if (ImGui.BeginMenu("New"))
                            {
                                if (ImGui.MenuItem("Folder Diff"))
                                {
                                    mainModel.CreateTab<DiffFolderWindow>();
                                }
                                if (ImGui.MenuItem("File Diff"))
                                {
                                    mainModel.CreateTab<DiffFileWindow>();
                                }
                                if (ImGui.MenuItem("Git Repository"))
                                {
                                    mainModel.CreateTab<GitRepoWindow>();
                                }
                                ImGui.EndMenu();
                            }

                            ImGui.Separator();
                            if (ImGui.MenuItem("Exit"))
                            {
                                Environment.Exit(0);
                            }
                            ImGui.EndMenu();
                        }

                        if (ImGui.BeginMenu("Edit"))
                        {
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

                            if (ImGui.MenuItem("Text Style"))
                            {
                                _textStyleModal.Popup();
                            }
                            ImGui.EndMenu();
                        }

                        if (ImGui.BeginMenu("Window"))
                        {
                            if (ImGui.MenuItem("Main Window"))
                            {
                                mainModel.CreateTab<MainTabWindow>();
                            }
                            if (ImGui.MenuItem("Terminal Window"))
                            {
                                mainModel.CreateTab<TerminalWindow>();
                            }
                            ImGui.EndMenu();
                        }

                        if (ImGui.BeginMenu("Help"))
                        {
                            if (ImGui.MenuItem("About"))
                            {
                                mainModel.CreateTab<AboutTabWindow>();
                            }
                            ImGui.EndMenu();
                        }

                        ImGui.EndMainMenuBar();
                    }

                    Vector2 contentSize = viewport.WorkSize;
                    contentSize.Y -= 20;
                    Vector2 contentPos = new Vector2(0, 20);

                    ImGui.SetNextWindowPos(contentPos);
                    ImGui.SetNextWindowSize(contentSize);

                    if (ImGui.Begin("Compare", windorFlag))
                    {
                        //_folderWindow?.OnDraw();
                        var tabWindows = mainModel.TabWindows;
                        if (tabWindows.Count > 0)
                        {
                            if (ImGui.BeginTabBar("Diff window tabs", ImGuiTabBarFlags.FittingPolicyDefault | ImGuiTabBarFlags.TabListPopupButton | ImGuiTabBarFlags.AutoSelectNewTabs))
                            {
                                for (int i = 0; i < tabWindows.Count; i++)
                                {
                                    var tabWindow = tabWindows[i];
                                    bool showTab = true;
                                    ImGuiTabItemFlags tabItemFlag = ImGuiTabItemFlags.Trailing | ImGuiTabItemFlags.NoCloseWithMiddleMouseButton;
                                    if (tabWindow.Unsave)
                                    {
                                        tabItemFlag |= ImGuiTabItemFlags.UnsavedDocument;
                                    }
                                    bool visible = ImGui.BeginTabItem(tabWindow.IconName + tabWindow.Name, ref showTab, tabItemFlag);
                                    if (ImGui.BeginPopupContextItem("TabItem MenuPopup"))
                                    {
                                        if (ImGui.MenuItem("Close"))
                                        {
                                            showTab = false;
                                        }

                                        if (ImGui.MenuItem("Close the other"))
                                        {
                                            for (int m = 0; m < tabWindows.Count; m++)
                                            {
                                                if (m != i)
                                                    _waitCloseTabIndexs.Add(m);
                                            }
                                        }
                                        if (ImGui.MenuItem("Close to the right"))
                                        {
                                            for (int m = i + 1; m < tabWindows.Count; m++)
                                            {
                                                _waitCloseTabIndexs.Add(m);
                                            }
                                        }
                                        if (ImGui.MenuItem("Close all"))
                                        {
                                            for (int m = 0; m < tabWindows.Count; m++)
                                            {
                                                _waitCloseTabIndexs.Add(m);
                                            }
                                        }
                                        ImGui.EndPopup();
                                    }
                                    if (ImGui.IsItemHovered())
                                    {
                                        if (ImGui.IsMouseClicked(ImGuiMouseButton.Right))
                                        {
                                            ImGui.OpenPopup("TabItem MenuPopup");
                                        }
                                    }
                                    if (visible)
                                    {
                                        tabWindow.OnDraw(!showGlobalControl);
                                        ImGui.EndTabItem();
                                    }
                                    if (!showTab)
                                    {
                                        //tabWindow.ExitModal = true;
                                        _waitCloseTabIndexs.Add(i);
                                    }
                                }


                                ImGui.EndTabBar();
                            }
                        }

                        //Text style colors settings.
                        _textStyleModal.Draw();
                        ImGuiFileDialog.Display();

                        //确认删掉多个tab
                        if (_waitCloseTabIndexs.Count > 0)
                        {
                            string multi = _waitCloseTabIndexs.Count > 1 ? "multi" : "current";
                            GlobalControl.DisplayDialog("Delete Tab Window", $"Check whether the {multi} TAB window is closed?", "ok", (isExit) => {
                                if (isExit)
                                {
                                    Queue<ITabWindow> removeTabs = new Queue<ITabWindow>();
                                    foreach (var tabIndex in _waitCloseTabIndexs)
                                    {
                                        removeTabs.Enqueue(mainModel.TabWindows[tabIndex]);
                                    }
                                    while (removeTabs.Count > 0)
                                    {
                                        var removeTab = removeTabs.Dequeue();
                                        mainModel.RemoveTab(removeTab);
                                    }
                                    _waitCloseTabIndexs.Clear();
                                }
                                else
                                {
                                    _waitCloseTabIndexs.Clear();
                                }
                            }, "cancel");
                        }

                        ImGui.End();
                    }
                    ImGui.End();
                }

                if (showGlobalControl)
                {
                    ImGui.EndDisabled();

                    ImGui.SetNextWindowPos(viewport.WorkPos);
                    ImGui.SetNextWindowSize(viewport.WorkSize);
                    ImGui.SetNextWindowViewport(viewport.ID);
                    if (ImGui.Begin("Global-Control", _defaultWindowFlag|ImGuiWindowFlags.NoBackground))
                    {
                        _globalControl.Draw();
                    }
                    ImGui.End();
                }
            }
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

            TextEditor.SetStyle(userSettings.TextStyleColors);
        }
    }
}
