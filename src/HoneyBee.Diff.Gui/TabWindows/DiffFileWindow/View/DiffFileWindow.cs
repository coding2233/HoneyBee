using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
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
                return $"{_name}";
            }
        }

        public string IconName =>Icon.Get(Icon.Material_file_copy);

        private string _leftDiffFilePath="";
        private string _rightDiffFilePath ="";
        private TextEditor _leftTextEditor;
        private TextEditor _rightTextEditor;

        private bool _showCompare;

        private SideBySideDiffModel _sideModel;

        [Import]
        public IMainWindowModel mainModel { get; set; }

        [Import]
        public IUserSettingsModel userSettings { get; set; }

        public DiffFileWindow()
        {
            DiffProgram.ComposeParts(this);

            _leftTextEditor = new TextEditor();
            _rightTextEditor = new TextEditor();
        }

        public void Setup(params object[] parameters)
        {
            _leftDiffFilePath = (string)parameters[0];
            _rightDiffFilePath = (string)parameters[1];
            Compare();
        }


        public void OnDraw()
        {
            if (ImGui.BeginChild("Left", new Vector2(ImGui.GetContentRegionAvail().X * 0.5f, 0), true, ImGuiWindowFlags.HorizontalScrollbar))
            {
                ImGui.InputText("", ref _leftDiffFilePath, 500);
                ImGui.SameLine();
                if (ImGui.Button("Select"))
                {
                }
                ImGui.SameLine();
                if (ImGui.Button("OK"))
                {
                    Compare();
                }
                if (ImGui.BeginChild("Left-Content"))
                {
                    _leftTextEditor?.Render("Left-TextEditor",ImGui.GetWindowSize());
                    //if (_sideModel != null)
                    //    OnDrawItem(_sideModel.OldText);
                }
                ImGui.EndChild();
            }
            ImGui.EndChild();

            ImGui.SameLine();

            //ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, 5.0f);
            if (ImGui.BeginChild("Right", new Vector2(0, 0), true))
            {
                ImGui.InputText("", ref _rightDiffFilePath, 500);
                ImGui.SameLine();
                if (ImGui.Button("Select"))
                {
                }
                ImGui.SameLine();
                if (ImGui.Button("OK"))
                {
                    Compare();
                }
                
                if (ImGui.BeginChild("Right-Content"))
                {
                    //if (_sideModel != null)
                    //    OnDrawItem(_sideModel.NewText);
                    _rightTextEditor?.Render("Right-TextEditor", ImGui.GetWindowSize());
                }
                ImGui.EndChild();
            }
            ImGui.EndChild();
        }

        private void OnDrawItem(DiffPaneModel diffModel)
        {
            if (_showCompare&& diffModel != null && diffModel.Lines !=null)
            {
                foreach (var item in diffModel.Lines)
                {
                    string showText = item.Text;
                    if (item.Text == null)
                        showText = "";
                    switch (item.Type)
                    {
                        case ChangeType.Inserted:
                            showText = "+\t" + showText;
                            ImGui.TextColored(userSettings.MarkGreenColor, showText);
                            break;
                        case ChangeType.Deleted:
                            showText = "-\t" + showText;
                            ImGui.TextColored(userSettings.MarkRedColor, showText);
                            break;
                        default:
                            ImGui.Text(showText);
                            break;
                    }
                }
            }
        }

  
        private async void Compare()
        {
            _showCompare = false;
            await Task.Run(() => {
                _sideModel = SideBySideDiffBuilder.Diff(File.ReadAllText(_leftDiffFilePath), File.ReadAllText(_rightDiffFilePath));
                _showCompare = true;
            });

            if (_showCompare)
            {
                string leftName = Path.GetFileName(_leftDiffFilePath);
                string rightName = Path.GetFileName(_rightDiffFilePath);
                _name = leftName.Equals(rightName) ? leftName : $"{leftName}/{rightName}";
                string oldName = _name;
                while (mainModel.HasSameWindow(_name, this))
                {
                    _name = $"{oldName} - {Guid.NewGuid().ToString().Substring(0, 6)}";
                }

                _leftTextEditor.text=BuilderShowText(_sideModel.OldText);
                _rightTextEditor.text = BuilderShowText(_sideModel.NewText);
            }
        }


        private string BuilderShowText(DiffPaneModel diffModel)
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (_showCompare && diffModel != null && diffModel.Lines != null)
            {
                foreach (var item in diffModel.Lines)
                {
                    string showText = item.Text;
                    if (item.Text == null)
                        showText = "";
                    switch (item.Type)
                    {
                        case ChangeType.Inserted:
                            showText = "+\t" + showText;
                            //ImGui.TextColored(userSettings.MarkGreenColor, showText);
                            break;
                        case ChangeType.Deleted:
                            showText = "-\t" + showText;
                            //ImGui.TextColored(userSettings.MarkRedColor, showText);
                            break;
                        default:
                            //ImGui.Text(showText);
                            break;
                    }

                    stringBuilder.AppendLine(showText);
                }
            }
            return stringBuilder.ToString();
        }


        public string Serialize()
        {
            string path = $"{_name}|{_leftDiffFilePath}|{_rightDiffFilePath}";
            return path;
        }

        public void Deserialize(string data)
        {
            string[] args = data.Split('|');
            _name = args[0];
            _leftDiffFilePath = args[1];
            _rightDiffFilePath = args[2];
            Compare();
        }

    public void Dispose()
        {
            _leftTextEditor?.Dispose();
            _rightTextEditor?.Dispose();
        }


    }
}
