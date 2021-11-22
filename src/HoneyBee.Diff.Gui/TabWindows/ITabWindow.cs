using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoneyBee.Diff.Gui
{
    public interface ITabWindow : IDisposable
    {
        string Name { get; }
        public  string IconName {get;}

        public bool Unsave { get; }

        public bool ExitModal { get; set; }

        void OnExitModalSure();

        void Setup(params object[] parameters);

        void OnDraw();

        string Serialize();

        bool Deserialize(string data);
    }
}
