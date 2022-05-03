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
        int StyleColors { get; set; }
        uint MarkBgColor { get; }
        Vector4 MarkRedColor { get; }
        Vector4 MarkGreenColor { get; }
        uint[] TextStyleColors { get; set; }
        bool Has<T>(string key);
        void Set<T>(string key,T value);
        T Get<T>(string key,T defaultValue = default(T));
    }
}
