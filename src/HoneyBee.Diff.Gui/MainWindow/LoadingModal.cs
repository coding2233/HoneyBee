using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoneyBee.Diff.Gui
{
    public class LoadingModal
    {
		private const string _popueModalName= "LoadingModal";

		public static void Draw(bool show)
        {
			if (show)
			{
				ImGui.OpenPopup(_popueModalName);
				if (ImGui.BeginPopupModal(_popueModalName, ref show, ImGuiWindowFlags.NoResize|ImGuiWindowFlags.NoTitleBar))
				{
					string symbols = "|/-\\";
					int index = (int)(ImGui.GetTime() / 0.05f) & 3;
					ImGui.Text($"Loading {symbols[index]}");
					ImGui.EndPopup();
				}
			}
		}


    }
}
