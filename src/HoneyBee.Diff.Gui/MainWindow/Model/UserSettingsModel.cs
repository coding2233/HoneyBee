using ImGuiNET;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using LiteDB;

namespace HoneyBee.Diff.Gui
{
    [Export(typeof(IUserSettingsModel))]
    public class UserSettingsModel : CustomerDataSave,IUserSettingsModel
    {

        

        private int _styleColors = -1;
        public int StyleColors 
        {
            get
            {
                if (_styleColors == -1)
                {
                    _styleColors = GetCustomerData<int>("StyleColors", 0);
                }
                return _styleColors;
            }
            set
            {
                _styleColors = value;
                SetCustomerData<int>("StyleColors", value);
            }
        }
        public uint MarkBgColor
        {
            get
            {
                uint markBgColor = 0;
                //light模式
                if (_styleColors == 0)
                {
                    markBgColor = ImGui.GetColorU32(new Vector4(1, 0.8f, 0.8f, 1));
                }
                else
                {
                   markBgColor = ImGui.GetColorU32(new Vector4(0.5f, 0.2f, 0.2f, 1));
                }
                return markBgColor;
            }
        }
        public Vector4 MarkRedColor
        {
            get
            {
                //light模式
                if (_styleColors == 0)
                {
                    return new Vector4(0.8f, 0.2f, 0.2f, 1);
                }
                else
                {
                    return new Vector4(1, 0.8f, 0.8f, 1);
                }
            }
        }
        public Vector4 MarkGreenColor
        {
            get
            {
                //light模式
                if (_styleColors == 0)
                {
                    return new Vector4(0.2f, 0.8f, 0.2f, 1);
                }
                else
                {
                    return new Vector4(0.8f, 1.0f, 0.8f, 1);
                }
            }
        }

        private uint[] _textStyleDrakColors = new uint[] {
   //         0xff7f7f7f,	// Default
			//0xffd69c56,	// Keyword	
            0xffb0b0b0,	// Default
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

        private uint[] _textStyleLightColors = new uint[] {
            //0xff7f7f7f,	// None
            0xff404040,	// None
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
			//0x80600000, // Selection
            0x40600000, // Selection
			0xa00010ff, // ErrorMarker
			0x80f08000, // Breakpoint
			0xff505000, // Line number
			0x40000000, // Current line fill
			0x40808080, // Current line fill (inactive)
			0x40000000, // Current line edge
			0x333333cc, // Flag line
        };

        public uint[] TextStyleColors 
        {
            get
            {
                uint[] defaultColors = null;
                if (_styleColors == 0)
                {
                    defaultColors = _textStyleLightColors;
                }
                else
                {
                    defaultColors = _textStyleDrakColors;
                }
                return GetCustomerData<uint[]>($"{_styleColors}_TextStyleColors", defaultColors);
            }
            set
            {
                SetCustomerData<uint[]>($"{_styleColors}_TextStyleColors", value);
            }
        }
        public void Set<T>(string key, T value)
        {
            SetCustomerData<T>(key, value);
           
        }
        public T Get<T>(string key,T defaultValue = default(T))
        {
            return GetCustomerData<T>(key,defaultValue);
        }

    }
}
