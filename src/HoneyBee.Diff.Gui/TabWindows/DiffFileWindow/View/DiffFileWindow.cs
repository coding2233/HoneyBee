using ImGuiNET;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace HoneyBee.Diff.Gui
{
    public class DiffFileWindow : ITabWindow
    {
        private string _name;
        public string Name
        {
            get
            {
                if (string.IsNullOrEmpty(_name))
                {
                    _name = "Diff File Window - " + Guid.NewGuid().ToString().Substring(0, 6);
                }
                return _name;
            }
        }

        private DiffFile _leftDiffFile;
        private DiffFile _rightDiffFile;

        private bool _showCompare;

        [Import]
        public IMainWindowModel mainModel { get; set; }

        public DiffFileWindow()
        {
            DiffProgram.ComposeParts(this);

            _leftDiffFile = new DiffFile();
            _rightDiffFile = new DiffFile();
        }

        public void Setup(params object[] parameters)
        {
            _leftDiffFile.FilePath = (string)parameters[0];
            _rightDiffFile.FilePath = (string)parameters[1];
            Compare();
        }

        public void OnDraw()
        {
            if (ImGui.BeginChild("Left", new Vector2(ImGui.GetContentRegionAvail().X * 0.5f, 0), true, ImGuiWindowFlags.HorizontalScrollbar))
            {
                ImGui.InputText("", ref _leftDiffFile.FilePath, 500);
                ImGui.SameLine();
                if (ImGui.Button("Select"))
                {
                }
                ImGui.SameLine();
                if (ImGui.Button("OK"))
                {
                    Compare();
                }

                OnDrawItem(_leftDiffFile);
            }
            ImGui.EndChild();

            ImGui.SameLine();

            //ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, 5.0f);
            if (ImGui.BeginChild("Right", new Vector2(0, 0), true))
            {
                ImGui.InputText("", ref _rightDiffFile.FilePath, 500);
                ImGui.SameLine();
                if (ImGui.Button("Select"))
                {
                }
                ImGui.SameLine();
                if (ImGui.Button("OK"))
                {
                    Compare();
                }
                OnDrawItem(_rightDiffFile);
            }
            ImGui.EndChild();
        }

        private void OnDrawItem(DiffFile diffFile)
        {
            if (_showCompare)
            {
                foreach (var item in diffFile.Lines)
                {
                    if (item.IsEmpty)
                    {
                        ImGui.Text("");
                    }
                    else
                    {
                        switch (item.Status)
                        {
                            case DiffStatus.Add:
                                ImGui.TextColored(new Vector4(0.8f, 1.0f, 0.8f, 1.0f), item.Content);
                                break;
                            case DiffStatus.Modified:
                                ImGui.TextColored(new Vector4(1.0f, 0.8f, 0.8f, 1.0f), item.Content);
                                break;
                            default:
                                ImGui.Text(item.Content);
                                break;
                        }
                    }
                }
            }
        }

  
        private async void Compare()
        {
            _showCompare = false;
            await Task.Run(() => {
                _showCompare = _rightDiffFile.GetDiffFlag(_leftDiffFile);
            });

            if (_showCompare)
            {
                string leftName = Path.GetFileName(_leftDiffFile.FilePath);
                string rightName = Path.GetFileName(_rightDiffFile.FilePath);
                _name = leftName.Equals(rightName) ? leftName : $"{leftName}/{rightName}";
                string oldName = _name;
                while (mainModel.HasSameWindow(_name, this))
                {
                    _name = $"{oldName} - {Guid.NewGuid().ToString().Substring(0, 6)}";
                }
            }
        }

        public void Dispose()
        {
        }


    }
}
