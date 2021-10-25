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

		private uint[] _drakColors = new uint[] {
			0xff7f7f7f,	// Default
			0xffd69c56,	// Keyword	
			0xff00ff00,	// Number
			0xff7070e0,	// String
			0xff70a0e0, // Char literal
			0xffffffff, // Punctuation
			0xff408080,	// Preprocessor
			0xffaaaaaa, // Identifier
			0xff9bc64d, // Known identifier
			0xffc040a0, // Preproc identifier
			0xff206020, // Comment (single line)
			0xff406020, // Comment (multi line)
			0xff101010, // Background
			0xffe0e0e0, // Cursor
			0x80a06020, // Selection
			0x800020ff, // ErrorMarker
			0x40f08000, // Breakpoint
			0xff707000, // Line number
			0x40000000, // Current line fill
			0x40808080, // Current line fill (inactive)
			0x40a0a0a0, // Current line edge
			0x33ccffcc, // Flag line
        };

		private uint[] _lightColors = new uint[] {
			0xff7f7f7f,	// None
			0xffff0c06,	// Keyword	
			0xff008000,	// Number
			0xff2020a0,	// String
			0xff304070, // Char literal
			0xff000000, // Punctuation
			0xff406060,	// Preprocessor
			0xff404040, // Identifier
			0xff606010, // Known identifier
			0xffc040a0, // Preproc identifier
			0xff205020, // Comment (single line)
			0xff405020, // Comment (multi line)
			0xffffffff, // Background
			0xff000000, // Cursor
			0x80600000, // Selection
			0xa00010ff, // ErrorMarker
			0x80f08000, // Breakpoint
			0xff505000, // Line number
			0x40000000, // Current line fill
			0x40808080, // Current line fill (inactive)
			0x40000000, // Current line edge
			0x333333cc, // Flag line
        };

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
			if (customColors == null)
			{
				customColors = userSettings.StyleColors == 0 ? _lightColors : _drakColors;
			}

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
