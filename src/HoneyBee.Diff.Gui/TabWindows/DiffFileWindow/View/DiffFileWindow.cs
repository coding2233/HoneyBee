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
    public class DiffFileWindow : DiffTabWindow
    {
        private string _name;
        public override string Name
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

        public override string IconName =>Icon.Get(Icon.Material_file_copy);

        private string _leftDiffFilePath="";
        private string _rightDiffFilePath ="";
        private TextEditor _leftTextEditor;
        private TextEditor _rightTextEditor;

        private bool _showCompare;
        private bool _readOnly=true;

        private SideBySideDiffModel _sideModel;

        [Import]
        public IMainWindowModel mainModel { get; set; }

        [Import]
        public IUserSettingsModel userSettings { get; set; }

        public DiffFileWindow()
        {
            DiffProgram.ComposeParts(this);

            string userPath = Environment.GetEnvironmentVariable("USERPROFILE");
            string folderPath = $"{userPath}\\Documents";

            _leftDiffFilePath = _rightDiffFilePath = folderPath;

            _leftTextEditor = new TextEditor();
            _leftTextEditor.ignoreChildWindow = true;

            _rightTextEditor = new TextEditor();
            _rightTextEditor.ignoreChildWindow = true;
        }

        public override void Setup(params object[] parameters)
        {
            _leftDiffFilePath = (string)parameters[0];
            _rightDiffFilePath = (string)parameters[1];
            OnCompare();
        }

        public override string Serialize()
        {
            string path = $"{_name}|{_leftDiffFilePath}|{_rightDiffFilePath}|{_readOnly}";
            return path;
        }

        public override void Deserialize(string data)
        {
            string[] args = data.Split('|');
            _name = args[0];
            _leftDiffFilePath = args[1];
            _rightDiffFilePath = args[2];
            if (args.Length == 4)
            {
                _readOnly = bool.Parse(args[3]);
                SetTextEditorStatus();
            }
            OnCompare();
        }

        protected override void OnToolbarDraw()
        {
            base.OnToolbarDraw();
            ImGui.SameLine();
            if (ImGui.Checkbox("Read Only",ref _readOnly))
            {
                SetTextEditorStatus();
            }
        }


        protected override void OnLeftToolbarDraw()
        {
            ImGui.InputText("", ref _leftDiffFilePath, 500);
            ImGui.SameLine();
            if (ImGui.Button("Select"))
            {
                string openPath = string.IsNullOrEmpty(_leftDiffFilePath) ? "./" : Path.GetDirectoryName(_leftDiffFilePath);
                ImGuiFileDialog.OpenFile((selectPath) => {
                    if (!string.IsNullOrEmpty(selectPath))
                    {
                        _leftDiffFilePath = selectPath;
                    }
                }, openPath);
            }
        }

        protected override void OnRightToolbarDraw()
        {
            ImGui.InputText("", ref _rightDiffFilePath, 500);
            ImGui.SameLine();
            if (ImGui.Button("Select"))
            {
                string openPath = string.IsNullOrEmpty(_rightDiffFilePath) ? "./" : Path.GetDirectoryName(_rightDiffFilePath);
                ImGuiFileDialog.OpenFile((selectPath) => {
                    if (!string.IsNullOrEmpty(selectPath))
                    {
                        _rightDiffFilePath = selectPath;
                    }
                }, openPath);
            }
        }

        protected override void OnLeftContentDraw()
        {
            _leftTextEditor?.Render("Left-TextEditor", ImGui.GetWindowSize());
        }

        protected override void OnRightContentDraw()
        {
            _rightTextEditor?.Render("Right-TextEditor", ImGui.GetWindowSize());
        }


        protected override async void OnCompare()
        {
            if (_loading)
                return;

            if (string.IsNullOrEmpty(_leftDiffFilePath) || string.IsNullOrEmpty(_rightDiffFilePath)
                || !File.Exists(_leftDiffFilePath) || !File.Exists(_rightDiffFilePath))
                return;
            _loading = true;
            _showCompare = false;
            await Task.Run(() => {
                _sideModel = SideBySideDiffBuilder.Diff(File.ReadAllText(_leftDiffFilePath), File.ReadAllText(_rightDiffFilePath));
                _showCompare = _sideModel!=null;
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

                var leftResult = BuilderShowText(_sideModel.OldText);
                _leftTextEditor.text = leftResult.Text;
                _leftTextEditor.flagLines = leftResult.FlagLines;

                var rightResult = BuilderShowText(_sideModel.NewText);
                _rightTextEditor.text = rightResult.Text;
                _rightTextEditor.flagLines = rightResult.FlagLines;
            }
            _loading = false;
        }

        private struct SideModelTextResult
        {
            public string Text;
            public int[] FlagLines;
        }

        private SideModelTextResult BuilderShowText(DiffPaneModel diffModel)
        {
            SideModelTextResult result = new SideModelTextResult();
            List<int> flagLines = new List<int>();
            StringBuilder stringBuilder = new StringBuilder();
            if (_showCompare && diffModel != null && diffModel.Lines != null)
            {
                for (int i = 0; i < diffModel.Lines.Count; i++)
                {
                    var item = diffModel.Lines[i];
                    string showText = item.Text;
                    if (item.Text == null)
                        showText = "";

                    if (item.Type != ChangeType.Unchanged)
                    {
                        flagLines.Add(i);
                    }
                    //switch (item.Type)
                    //{
                    //    case ChangeType.Inserted:
                    //        //showText = "+\t" + showText;
                    //        flagLines.Add(i);
                    //        //ImGui.TextColored(userSettings.MarkGreenColor, showText);
                    //        break;
                    //    case ChangeType.Deleted:
                    //        //showText = "-\t" + showText;
                    //        flagLines.Add(i);
                    //        //ImGui.TextColored(userSettings.MarkRedColor, showText);
                    //        break;
                    //    default:
                    //        //ImGui.Text(showText);
                    //        break;
                    //}
                    stringBuilder.AppendLine(showText);
                }
            }
            result.Text = stringBuilder.ToString();
            result.FlagLines = flagLines.ToArray();
            return result;
        }

        //设置文本的状态
        private void SetTextEditorStatus()
        {
            _leftTextEditor.readOnly = _readOnly;
            _rightTextEditor.readOnly = _readOnly;
        }


        /// <summary>
        /// Determines a text file's encoding by analyzing its byte order mark (BOM).
        /// Defaults to ASCII when detection of the text file's endianness fails.
        /// </summary>
        /// <param name="filename">The text file to analyze.</param>
        /// <returns>The detected encoding.</returns>
        public static Encoding GetEncoding(string filename)
        {
            // Read the BOM
            var bom = new byte[4];
            using (var file = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                file.Read(bom, 0, 4);
            }

            // Analyze the BOM
            if (bom[0] == 0x2b && bom[1] == 0x2f && bom[2] == 0x76) return Encoding.UTF7;
            if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf) return Encoding.UTF8;
            if (bom[0] == 0xff && bom[1] == 0xfe && bom[2] == 0 && bom[3] == 0) return Encoding.UTF32; //UTF-32LE
            if (bom[0] == 0xff && bom[1] == 0xfe) return Encoding.Unicode; //UTF-16LE
            if (bom[0] == 0xfe && bom[1] == 0xff) return Encoding.BigEndianUnicode; //UTF-16BE
            if (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff) return new UTF32Encoding(true, true);  //UTF-32BE

            // We actually have no idea what the encoding is if we reach this point, so
            // you may wish to return null instead of defaulting to ASCII
            return Encoding.ASCII;
        }

        /// <summary>
        /// Determines a text file's encoding by analyzing its byte order mark (BOM).
        /// Defaults to ASCII when detection of the text file's endianness fails.
        /// </summary>
        /// <param name="filename">The text file to analyze.</param>
        /// <returns>The detected encoding.</returns>
        public static Encoding GetEncoding(byte[] bom)
        {
            // Analyze the BOM
            if (bom[0] == 0x2b && bom[1] == 0x2f && bom[2] == 0x76) return Encoding.UTF7;
            if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf) return Encoding.UTF8;
            if (bom[0] == 0xff && bom[1] == 0xfe && bom[2] == 0 && bom[3] == 0) return Encoding.UTF32; //UTF-32LE
            if (bom[0] == 0xff && bom[1] == 0xfe) return Encoding.Unicode; //UTF-16LE
            if (bom[0] == 0xfe && bom[1] == 0xff) return Encoding.BigEndianUnicode; //UTF-16BE
            if (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff) return new UTF32Encoding(true, true);  //UTF-32BE

            // We actually have no idea what the encoding is if we reach this point, so
            // you may wish to return null instead of defaulting to ASCII
            return Encoding.ASCII;
        }

        public override void Dispose()
        {
            _leftTextEditor?.Dispose();
            _rightTextEditor?.Dispose();
        }


    }
}
