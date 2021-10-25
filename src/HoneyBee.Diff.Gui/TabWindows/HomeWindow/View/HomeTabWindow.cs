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
    public class HomeTabWindow : ITabWindow
    {
        public string Name => "Home";

        public string IconName => Icon.Get(Icon.Material_home);

       
        public HomeTabWindow()
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
