using ImGuiNET;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoneyBee.Diff.Gui
{
    public class MainTabWindow : ITabWindow
    {
        public string Name => "Home";

        public string IconName => Icon.Get(Icon.Material_home);

        public bool Unsave => false;

        public bool ExitModal { get; set; }

        [Import]
        public IMainWindowModel mainModel { get; set; }
        [Import]
        public IUserSettingsModel userSettings { get; set; }

        private List<MainTabData> _mainTabDatas = new List<MainTabData>();

        public MainTabWindow()
        {
			DiffProgram.ComposeParts(this);

            var tabList = mainModel.TabWindowDataList;
            if (tabList != null && tabList.Count>0)
            {
                try
                {
                    foreach (var item in tabList)
                    {
                        MainTabData mainTabData = new MainTabData();
                        string data = item;
                        int index = data.IndexOf('|');
                        string typeFullName = data.Substring(0, index);
                        index++;
                        data = data.Substring(index, data.Length - index);
                        index = data.IndexOf('|');
                        string dateTime = data.Substring(0, index);
                        index++;
                        if (index < data.Length)
                        {
                            data = data.Substring(index, data.Length - index);
                        }

                        mainTabData.FullName = typeFullName;
                        mainTabData.Folder = typeFullName.Equals(typeof(DiffFolderWindow).FullName);
                        mainTabData.Time = dateTime;
                        mainTabData.Data = data;

                        _mainTabDatas.Add(mainTabData);
                    }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                mainModel.DeleteDatabase();
            }
        }
        }

        public bool Deserialize(string data)
        {
            return true;
        }

        public void Dispose()
        {
        }

        public void OnDraw()
        {
            if (ImGui.BeginTable("DiffFolderTable", 2, ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders | ImGuiTableFlags.Resizable | ImGuiTableFlags.Reorderable))
            {
                ImGui.TableSetupColumn("Data", ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableSetupColumn("Time", ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoResize);
                ImGui.TableHeadersRow();

                foreach (var item in _mainTabDatas)
                {
                    ImGui.TableNextRow();
                   
                    ImGui.TableSetColumnIndex(0);
                    ImGui.Text($"{Icon.Get(item.Folder ? Icon.Material_folder : Icon.Material_text_snippet)}{item.Data}");
                    var rectMin = ImGui.GetItemRectMin();
                    var rectMax = ImGui.GetItemRectMax();
                    rectMax.X = ImGui.GetColumnWidth();
                    if (ImGui.IsMouseHoveringRect(rectMin, rectMax))
                    {
                        ImGui.GetWindowDrawList().AddRectFilled(rectMin, rectMax, ImGui.GetColorU32(ImGuiCol.TextSelectedBg));
                        if (ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                        {
                            mainModel.CreateTabInstance(item.FullName,item.Data);
                        }
                    }
                    ImGui.TableSetColumnIndex(1);
                    ImGui.Text(item.Time);
                }

                ImGui.EndTable();
            }
        }

        public void OnExitModalSure()
        {
        }

        public string Serialize()
        {
            return "@";
        }

        public void Setup(params object[] parameters)
        {
        }

        struct MainTabData
        {
            internal bool Folder;
            internal string FullName;
            internal string Data;
            internal string Time;
        }
    }
}
