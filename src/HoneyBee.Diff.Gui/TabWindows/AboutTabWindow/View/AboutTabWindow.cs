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

        public string IconName => Icon.Get(Icon.Material_info);

        public bool Unsave => false;

        public const string Version = "0.1.0";
        public bool ExitModal { get; set; }
        public AboutTabWindow()
        {
			DiffProgram.ComposeParts(this);
		}

        public void Setup(params object[] parameters)
        {
        }

        public void OnDraw()
        {
            ImGui.Text($"Version: {Version}");
            var tptr = DiffProgram.GetOrCreateTexture("bee.png");
            ImGui.Image(tptr, Vector2.One * 128);
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

        public void OnExitModalSure()
        {
        }
    }
}
