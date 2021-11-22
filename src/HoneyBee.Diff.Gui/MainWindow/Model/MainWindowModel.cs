using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace HoneyBee.Diff.Gui
{
    [Export(typeof(IMainWindowModel))]
    public class MainWindowModel : CustomerDataSave, IMainWindowModel
    {
        private const string TABWINDOWSKEY= "TabWindows";
        private List<ITabWindow> _tabWindows = new List<ITabWindow>();
        public List<ITabWindow> TabWindows => _tabWindows;

        public HashSet<string> ShowLoading { get; set; } = new HashSet<string>();

        public List<string> TabWindowDataList => GetCustomerData<List<string>>(TABWINDOWSKEY, null);

        public MainWindowModel()
        {

        }

        public void Init(bool restore)
        {
            //var tabLists = GetCustomerData<List<string>>(TABWINDOWSKEY, null);
            //if (restore && tabLists != null)
            //{
            //    foreach (var item in tabLists)
            //    {
            //        int index = item.IndexOf('|');
            //        string typeFullName = item.Substring(0, index);
            //        string data = item.Substring(index + 1, item.Length - index - 1);
            //        ITabWindow tabWindow = (ITabWindow)GetType().Assembly.CreateInstance(typeFullName);
            //        if (tabWindow.Deserialize(data))
            //        {
            //            _tabWindows.Add(tabWindow);
            //        }
            //    }
            //    SaveData();
            //}
            //else
            //{
            //    CreateTab<AboutTabWindow>();
            //}
        }

        public void CreateTabInstance(string typeFullName, string data)
        {
            ITabWindow tabWindow = (ITabWindow)GetType().Assembly.CreateInstance(typeFullName);
            if (tabWindow.Deserialize(data))
            {
                if (_tabWindows.Find(x => x.Name.Equals(tabWindow.Name)) == null)
                {
                    _tabWindows.Add(tabWindow);
                    SaveData();
                }
            }
            else
            {
                var tabDataList = TabWindowDataList;
                var findData = tabDataList.Find(x => x.StartsWith(typeFullName) && x.EndsWith(data));
                if (findData != null)
                {
                    tabDataList.Remove(findData);
                    SetCustomerData<List<string>>(TABWINDOWSKEY, tabDataList);
                }
            }
        }

        public void CreateTab<T>(params object[] parameters) where T : ITabWindow, new()
        {
            T t = new T();
            if (parameters != null && parameters.Length>0)
            {
                t.Setup(parameters);
            }
            if (_tabWindows.Find(x => x.Name.Equals(t.Name)) == null)
            {
                _tabWindows.Add(t);
                SaveData();
            }
        }

        public bool HasSameWindow(string name,ITabWindow tabWindow = null)
        {
            var findTabWindow = _tabWindows.Find(x => x.Name.Equals(name));
            if (findTabWindow==null || findTabWindow == tabWindow)
                return false;
            return  true;
        }

        public void SaveWindow(ITabWindow tabWindow)
        {
            SaveData();
        }

        public void RemoveTab(int index)
        {
            if (index>=0 && index < _tabWindows.Count)
            {
                var tabWindow = _tabWindows[index];
                _tabWindows.RemoveAt(index);
                tabWindow.Dispose();

                SaveData();
            }
        }

        public void RemoveTab(ITabWindow tabWindow)
        {
            int index = _tabWindows.IndexOf(tabWindow);
            RemoveTab(index);
        }


        private void SaveData()
        {
            List<string> data = TabWindowDataList;
            if (data == null)
            {
                data = new List<string>();
            }
            foreach (var item in _tabWindows)
            {
                if (item.GetType() == typeof(MainTabWindow)
                    || item.GetType() == typeof(AboutTabWindow))
                    continue;
                string typeFullName = item.GetType().FullName;
                string serializeData = item.Serialize();
                var findData = data.Find(x => x.StartsWith(typeFullName) && x.EndsWith(serializeData));
                if (findData != null)
                {
                    data.Remove(findData);
                }
                string itemData = $"{typeFullName}|{DateTime.Now.ToString()}|{serializeData}";
                data.Add(itemData);
            }
            if(data.Count>0)
                SetCustomerData<List<string>>(TABWINDOWSKEY, data);
        }

        public void DeleteDatabase()
        {
            DeleteDatabaseFile();
        }
         

    }
}
