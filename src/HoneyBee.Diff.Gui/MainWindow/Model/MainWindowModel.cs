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

        public MainWindowModel()
        {
            
        }

        public void Init()
        {
            var tabLists = GetCustomerData<List<string>>(TABWINDOWSKEY, null);
            if (tabLists != null)
            {
                foreach (var item in tabLists)
                {
                    int index = item.IndexOf('|');
                    string typeFullName = item.Substring(0, index);
                    string data = item.Substring(index + 1, item.Length - index - 1);
                    ITabWindow tabWindow = (ITabWindow)GetType().Assembly.CreateInstance(typeFullName);
                    tabWindow.Deserialize(data);
                    _tabWindows.Add(tabWindow);
                }

            }
            else
            {
                CreateTab<HomeTabWindow>();
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
            if (findTabWindow == tabWindow)
                return false;
            return  true;
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
            List<string> data = new List<string>();
            foreach (var item in _tabWindows)
            {
                string itemData = $"{item.GetType().FullName}|{item.Serialize()}";
                data.Add(itemData);
            }
            if(data.Count>0)
                SetCustomerData<List<string>>(TABWINDOWSKEY, data);
        }

    }
}
