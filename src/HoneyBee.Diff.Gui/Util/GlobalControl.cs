using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoneyBee.Diff.Gui
{
    public class GlobalControl
    {
        private static GlobalControl _globalControl;
        private IGlobalControl _globalControlImplement;
        public static bool Show => _globalControl!=null && _globalControl._globalControlImplement != null;

        public GlobalControl()
        {
            _globalControl = this;
        }

        public static void DisplayDialog(string title, string message, string ok,Action<bool> callback=null, string cancel=null)
        {
            if (!Show)
            {
                DisplayPopup(new DialogGlobalControl(title, message, ok, callback, cancel));
            }
        }
        
        public static void DisplayProgressBar(string title, string info, float progress, bool cancel = false)
        { }

        public static void DisplayPopup(IGlobalControl globalControlImplement)
        {
            if (_globalControl !=null )
            {
                _globalControl._globalControlImplement = globalControlImplement;
            }
        }
    

        public void Draw()
        {
            if (_globalControlImplement != null)
            {
                if (!_globalControlImplement.Draw())
                {
                    _globalControlImplement = null;
                }
            }
        }
    }

    public interface IGlobalControl
    {
        bool Draw();
    }

    public class DialogGlobalControl : IGlobalControl
    {
        private string _title;
        private string _message;
        private string _ok;
        private string _cancel;
        private Action<bool> _callback;

        public DialogGlobalControl(string title, string message, string ok, Action<bool> callback, string cancel = null)
        {
            _title = title;
            _message = message;
            _ok = string.IsNullOrEmpty(ok) ? "ok" : ok;
            _cancel = cancel;
            _callback = callback;
        }

        public bool Draw()
        {
            ImGui.OpenPopup("DialogGlobalControl");
            bool opePopupModal = true;
            if (ImGui.BeginPopupModal("DialogGlobalControl", ref opePopupModal, ImGuiWindowFlags.AlwaysAutoResize))
            {
                ImGui.Text(_title);
                ImGui.Text(_message);

                if (ImGui.Button(_ok))
                {
                    _callback?.Invoke(true);
                    return false;
                }
                if (!string.IsNullOrEmpty(_cancel))
                {
                    ImGui.SameLine();
                    if (ImGui.Button(_cancel))
                    {
                        _callback?.Invoke(false);
                        return false;
                    }
                }
                ImGui.EndPopup();
            }
            return true;
        }
    }
}
