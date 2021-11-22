using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoneyBee.Diff.Gui
{
    public interface IMainWindowModel
    {
        public HashSet<string> ShowLoading { get; set; }

        List<ITabWindow> TabWindows {get;}
        List<string> TabWindowDataList { get; }

        void Init(bool restore);
        void CreateTabInstance(string typeFullName, string data);
        void CreateTab<T>(params object[] objets) where T : ITabWindow,new();

        bool HasSameWindow(string name,ITabWindow tabWindow=null);
        void SaveWindow(ITabWindow tabWindow);
        void RemoveTab(int index);
        void RemoveTab(ITabWindow tabWindow);

        void DeleteDatabase();
    }
}
