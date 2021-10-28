using ImGuiNET;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace HoneyBee.Diff.Gui
{
    public class AboutTabWindow : ITabWindow
    {
        public string Name => "About";

        public string IconName => Icon.Get(Icon.Material_home);

        public bool Unsave => false;

        public AboutTabWindow()
        {
			DiffProgram.ComposeParts(this);

			
		}

        public void Setup(params object[] parameters)
        {
        }


        public void OnDraw()
        {
			
		}
        public string Serialize()
        {
            return "@";
        }

        public void Deserialize(string data)
        {
        }

		

        public void Dispose()
        {
        }

    }
}
