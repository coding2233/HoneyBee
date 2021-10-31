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

        private TextStyleSettingModal _textStyleModal;

        private ImGuiWindowFlags _defaultWindowFlag;

        private bool _isLaunchWindow;

        public MainWindow(bool isLaunchWindow)
        {
            _isLaunchWindow = isLaunchWindow;

            //Ioc entrance.
            DiffProgram.ComposeParts(this);

            //logic...
            SetStyleColors();

            //初始化
            mainModel.Init();

            _textStyleModal = new TextStyleSettingModal();

            _defaultWindowFlag = ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize 
                | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse ;
        }

        public void OnDraw()
        {
            ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 3.0f);

            var viewport = ImGui.GetMainViewport();
            ImGui.SetNextWindowPos(viewport.WorkPos);
            ImGui.SetNextWindowSize(viewport.WorkSize);
            ImGui.SetNextWindowViewport(viewport.ID);
          
            if (ImGui.Begin("Diff", _defaultWindowFlag|ImGuiWindowFlags.NoBringToFrontOnFocus))
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
                Vector2 contentPos = new Vector2(0,20);

                ImGui.SetNextWindowPos(contentPos);
                ImGui.SetNextWindowSize(contentSize);

                if (ImGui.Begin("Compare", _defaultWindowFlag))
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
                                ImGuiTabItemFlags tabItemFlag = ImGuiTabItemFlags.Trailing;
                                if (tabWindow.Unsave)
                                {
                                    tabItemFlag |= ImGuiTabItemFlags.UnsavedDocument;
                                }
                                bool visible = ImGui.BeginTabItem(tabWindow.IconName+tabWindow.Name,ref showTab, tabItemFlag);
                                if (visible)
                                {
                                    tabWindow.OnDraw();

                                    if (tabWindow.ExitModal)
                                    {
                                        ImGui.OpenPopup("Exit Modal");
                                        bool openExitModal = true;
                                        if (ImGui.BeginPopupModal("Exit Modal",ref openExitModal,ImGuiWindowFlags.AlwaysAutoResize))
                                        {

                                            ImGui.Text("Check whether the current TAB window is closed?");

                                            if (ImGui.Button("Sure"))
                                            {
                                                mainModel.RemoveTab(i);
                                                i--;
                                                break;
                                            }
                                            ImGui.SameLine();
                                            if (ImGui.Button("Cancel"))
                                            {
                                                tabWindow.ExitModal = false;
                                            }
                                            ImGui.EndPopup();
                                        }
                                        if (!openExitModal)
                                        {
                                            tabWindow.ExitModal = false;
                                        }
                                    }
                                   
                                    ImGui.EndTabItem();
                                }
                                if (!showTab)
                                {
                                    tabWindow.ExitModal = true;
                                    //mainModel.RemoveTab(i);
                                    //i--;
                                }
                            }



                            //Text style colors settings.
                            _textStyleModal.Draw();
                            //LoadingModal.Draw(mainModel.ShowLoading.Count>0);
                            ImGuiFileDialog.Display();

                            ImGui.EndTabBar();
                        }
                    }
                    ImGui.End();
                }
                ImGui.End();
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
