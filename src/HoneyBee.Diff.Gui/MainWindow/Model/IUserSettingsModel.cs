using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace HoneyBee.Diff.Gui
{
    public interface IUserSettingsModel
    {
        public int StyleColors { get; set; }
        public uint MarkBgColor { get; }
        public Vector4 MarkRedColor { get; }
        public Vector4 MarkGreenColor { get; }
        public uint[] TextStyleColors { get; set; }
    }
}
