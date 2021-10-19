using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoneyBee.Diff.Gui
{
    public interface IMainWindowModel
    {
        List<ITabWindow> TabWindows {get;}

        void CreateTab<T>(params object[] objets) where T : ITabWindow,new();

        bool HasSameWindow(string name,ITabWindow tabWindow=null);

        void RemoveTab(int index);
        void RemoveTab(ITabWindow tabWindow);
    }
}
