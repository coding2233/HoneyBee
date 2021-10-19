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
    public class MainWindowModel : IMainWindowModel
    {
        private List<ITabWindow> _tabWindows = new List<ITabWindow>();
        public List<ITabWindow> TabWindows => _tabWindows;

        public void CreateTab<T>(params object[] parameters) where T : ITabWindow, new()
        {
            T t = new T();
            if (parameters != null)
            {
                t.Setup(parameters);
            }
            if (_tabWindows.Find(x => x.Name.Equals(t.Name)) == null)
            {
                _tabWindows.Add(t);
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
            }
        }

        public void RemoveTab(ITabWindow tabWindow)
        {
            int index = _tabWindows.IndexOf(tabWindow);
            RemoveTab(index);
        }

    }
}
