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
        void Setup(params object[] parameters);
        void OnDraw();

        string Serialize();

        void Deserialize(string data);
    }
}
