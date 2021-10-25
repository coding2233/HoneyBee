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
                    markBgColor = ImGui.GetColorU32(new Vector4(0.8f, 0.2f, 0.2f, 1));
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


        public uint[] TextStyleColors 
        {
            get
            {
                return GetCustomerData<uint[]>($"{_styleColors}_TextStyleColors",null);
            }
            set
            {
                SetCustomerData<uint[]>($"{_styleColors}_TextStyleColors", value);
            }
        }


    }
}
