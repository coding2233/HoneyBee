using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoneyBee.Diff.Gui
{
    public class GitRepoWindow : ITabWindow
    {
        public string Name => "GitRepoWindow";

        public string IconName => Icon.Get(Icon.Material_gite);

        public bool Unsave => false;

        public bool ExitModal { get; set; }

        public void Deserialize(string data)
        {
        }

        public void Dispose()
        {
        }

        public void OnDraw()
        {
        }

        public void OnExitModalSure()
        {
        }

        public string Serialize()
        {
            return "";
        }

        public void Setup(params object[] parameters)
        {
        }
    }
}
