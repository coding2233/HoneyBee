using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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
        {
            ProgressBarGlobalControl progressBar;
            if (Show)
            {
                progressBar = _globalControl._globalControlImplement as ProgressBarGlobalControl;
                if (progressBar != null)
                {
                    if (progress > 0)
                    {
                        progressBar.SetProgress(progress);
                    }
                    if (!string.IsNullOrEmpty(info))
                    {
                        progressBar.SetInfo(info);
                    }
                }
            }
            else
            {
                DisplayPopup(new ProgressBarGlobalControl(title, cancel));
            }
        }


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
            bool clear = true;
            //var center = ImGui.GetMainViewport().GetCenter();
            //ImGui.SetNextWindowPos(center, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));
            ImGui.PushStyleColor(ImGuiCol.PopupBg, ImGui.GetColorU32(ImGuiCol.ChildBg));
            ImGui.OpenPopup("DialogGlobalControl");

            if (ImGui.BeginPopupModal("DialogGlobalControl"))
            {
                ImGui.Text(_title);
                ImGui.Text(_message);

                if (ImGui.Button(_ok))
                {
                    _callback?.Invoke(true);
                    clear = false;
                }
                if (!string.IsNullOrEmpty(_cancel))
                {
                    ImGui.SameLine();
                    if (ImGui.Button(_cancel))
                    {
                        _callback?.Invoke(false);
                        clear = false;
                    }
                }
                ImGui.EndPopup();
            }
            ImGui.PopStyleColor();

            return clear;
        }
    }

    public class ProgressBarGlobalControl: IGlobalControl
    {
        private string _title;
        private bool _cancel;
        private float _progress;
        private List<string> _infos = new List<string>();

        public ProgressBarGlobalControl(string title, bool cancel = false)
        {
            _title = title;
            _cancel = cancel;
        }

        public bool Draw()
        {
            bool clear = true;
            //var center = ImGui.GetMainViewport().GetCenter();
            //ImGui.SetNextWindowPos(center, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));
            //ImGui.PushStyleColor(ImGuiCol.PopupBg, ImGui.GetColorU32(ImGuiCol.ChildBg));
            ImGui.OpenPopup("ProgressBarGlobalControl");
            bool show = true;
            ImGui.SetNextWindowPos(new Vector2(5, 10));
            if (ImGui.BeginPopupModal("ProgressBarGlobalControl",ref show, ImGuiWindowFlags.ChildWindow | ImGuiWindowFlags.AlwaysAutoResize|ImGuiWindowFlags.NoTitleBar| ImGuiWindowFlags.NoMove| ImGuiWindowFlags.NoResize|ImGuiWindowFlags.NoInputs))
            {
                ImGui.Text(_title);
                ImGui.SetNextItemWidth(ImGui.GetWindowViewport().WorkSize.X*0.99f);
                float ss = (float)(ImGui.GetTime() % 5.0f)*20.0f;
                ImGui.SliderFloat("", ref ss,0,100);


                if (_cancel)
                {
                    ImGui.SameLine();
                    if (ImGui.Button("cancel"))
                    {
                        clear = false;
                    }
                }

                int index = _infos.Count > 30 ? _infos.Count - 30 : 0;
                for (int i = index; i < _infos.Count; i++)
                {
                    ImGui.Text(_infos[i]);
                }

                //if (_checkShowInfo && !string.IsNullOrEmpty(_showInfo))
                //{
                //    ImGui.Text(_showInfo);
                //}

                ImGui.EndPopup();
            }
            //ImGui.PopStyleColor();
            if (_progress <= -1.0f)
            {
                clear = false;
            }
            return clear;
        }

        public void SetProgress(float progress)
        {
            _progress = progress;
        }

        public void SetInfo(string info)
        {
            _infos.Add(info);
        }
    }
}
