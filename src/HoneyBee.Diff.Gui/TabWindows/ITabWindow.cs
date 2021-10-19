using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoneyBee.Diff.Gui
{
    public interface ITabWindow
    {
        string Name { get; }
        void OnDraw();
    }
}
