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
    public class TextStyleSettingModal
    {
		private string[] _colorNames = new string[] { "None", "Keyword", "Number", "String", "Char literal",
			"Punctuation", "Preprocessor","Identifier","Known identifier","Preproc identifier","Comment (single line)"
			,"Comment (multi line)","Background","Cursor","Selection","ErrorMarker","Breakpoint","Line number"
			,"Current line fill","Current line fill (inactive)", "Current line edge","Flag line"};


		//自定义颜色
		private Vector4[] _customColors = new Vector4[22];

		[Import]
		public IUserSettingsModel userSettings { get; set; }

		private const string _popueModalName = "TextStyleSettingModal";

		public TextStyleSettingModal()
		{
			DiffProgram.ComposeParts(this);
		}

		public void Popup()
		{
			var customColors = userSettings.TextStyleColors;

			for (int i = 0; i < customColors.Length; i++)
			{
				_customColors[i] = ImGui.ColorConvertU32ToFloat4(customColors[i]);
			}

			ImGui.OpenPopup(_popueModalName);
		}

		public void Draw()
		{
			if (ImGui.BeginPopupModal(_popueModalName))
			{
				if (ImGui.Button("Save"))
				{
					userSettings.TextStyleColors = GetCustomColors();
				}
				ImGui.SameLine();
				if (ImGui.Button("Close"))
				{
					ImGui.CloseCurrentPopup();
				}

				for (int i = 0; i < _colorNames.Length; i++)
				{
					ImGui.ColorEdit4(_colorNames[i], ref _customColors[i]);
				}
				ImGui.EndPopup();
			}
		}

		private uint[] GetCustomColors()
		{
			var ccs = new uint[_customColors.Length];
			for (int i = 0; i < _customColors.Length; i++)
			{
				ccs[i] = ImGui.ColorConvertFloat4ToU32(_customColors[i]);
			}
			return ccs;
		}

	}
}
