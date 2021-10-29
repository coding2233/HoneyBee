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

        public override string IconName =>Icon.Get(Icon.Material_article);

        private SideBySideDiffModel _sideModel;

        private DiffFile _leftDiffFile;
        private DiffFile _rightDiffFile;

        private bool _showCompare;
        private bool _readOnly=true;

        public DiffFileWindow()
        {
            string userPath = Environment.GetEnvironmentVariable("USERPROFILE");
            string folderPath = $"{userPath}\\Documents";

            _toolbarHeight = 55.0f;

            _leftDiffFile = new DiffFile("right-arrow.png", "Copy the section to the right.");
            _rightDiffFile = new DiffFile("left-arrow.png", "Copy the section to the left.");

            _leftDiffFile.FilePath = _rightDiffFile.FilePath = folderPath;

        }

        public override void Setup(params object[] parameters)
        {
            _leftDiffFile.FilePath = (string)parameters[0];
            _rightDiffFile.FilePath = (string)parameters[1];
            OnCompare();
        }

        public override string Serialize()
        {
            string path = $"{_name}|{_leftDiffFile.FilePath}|{_rightDiffFile.FilePath}|{_readOnly}";
            return path;
        }

        public override void Deserialize(string data)
        {
            string[] args = data.Split('|');
            _name = args[0];
            _leftDiffFile.FilePath = args[1];
            _rightDiffFile.FilePath = args[2];
            if (args.Length == 4)
            {
                _readOnly = bool.Parse(args[3]);
                SetTextEditorStatus();
            }
            OnCompare();
        }

        public override void OnDraw()
        {
            base.OnDraw();

            Unsave = _leftDiffFile.IsTextChanged || _rightDiffFile.IsTextChanged;
        }

        protected override void OnToolbarDraw()
        {
            base.OnToolbarDraw();
            ImGui.SameLine();
            if (ImGui.Checkbox("Read Only",ref _readOnly))
            {
                _readOnly = true;
                SetTextEditorStatus();
            }
        }


        protected override void OnLeftToolbarDraw()
        {
            ToolbarDraw(_leftDiffFile);
        }

        protected override void OnRightToolbarDraw()
        {
            ToolbarDraw(_rightDiffFile);
        }

        protected override void OnLeftContentDraw()
        {
            TextContentDraw("Diff_File_TextEditor_Left",_leftDiffFile);
        }

        protected override void OnRightContentDraw()
        {
            TextContentDraw("Diff_File_TextEditor_Right",_rightDiffFile);
        }

        private void ToolbarDraw(DiffFile diffFile)
        {
            ImGui.InputText("", ref diffFile.FilePath, 500);
            ImGui.SameLine();
            if (ImGui.Button(Icon.Get(Icon.Material_open_in_browser)))
            {
                string openPath = string.IsNullOrEmpty(diffFile.FilePath) ? "./" : Path.GetDirectoryName(diffFile.FilePath);
                ImGuiFileDialog.OpenFile((selectPath) => {
                    if (!string.IsNullOrEmpty(selectPath))
                    {
                        diffFile.FilePath = selectPath;
                    }
                }, openPath);
            }
            ImGui.SameLine();
            if (ImGui.Button(Icon.Get(Icon.Material_save)))
            {
                //diffFile.SaveFile();
            }

            var cpos = diffFile.TextEditor.CursorPosition;
            string linePos = ((int)cpos.X+1).ToString("D4");
            string columnPos = ((int)cpos.Y+1).ToString("D4");
            string overWrite = diffFile.TextEditor.IsOverwrite ? "Over" : "Ins";
            string canUndo = diffFile.TextEditor.CanUndo ? "*" : " ";
            ImGui.Text($"{linePos}/{columnPos} lines {diffFile.TextEditor.TotalLines} | {overWrite} | {canUndo} | {diffFile.IsTextChanged} ");
        }

        private void TextContentDraw(string title,DiffFile diffFile)
        {
            diffFile.TextEditor.Render(title, ImGui.GetWindowSize());

            var textResult = diffFile.TextResult;
            if (textResult.FlagPoints != null)
            {
                for (int i = 0; i < textResult.FlagPoints.Length; i++)
                {
                    int lineNo = textResult.FlagPoints[i];
                    var rect = diffFile.TextEditor.GetFlagPointRect(lineNo);
                    if (rect != Vector4.Zero)
                    {
                        Vector2 minRect = new Vector2(rect.X - (rect.Z - rect.X), rect.Y);
                        Vector2 maxRect = minRect + Vector2.One * (rect.W - minRect.Y);
                        ImGui.GetWindowDrawList().AddImage(diffFile.IconIntPtr, minRect, maxRect);
                        if (ImGui.IsMouseHoveringRect(minRect, maxRect))
                        {
                            ImGui.BeginTooltip();
                            ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35.0f);
                            ImGui.TextUnformatted(diffFile.IconTip);
                            ImGui.PopTextWrapPos();
                            ImGui.EndTooltip();

                            if (ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                            {
                                CopySection(lineNo, diffFile);
                            }
                        }

                    }
                }
            }
        }


        protected override async void OnCompare()
        {
            try
            {
                if (_loading)
                return;

                string leftPath = _leftDiffFile.FilePath;
                string rightPath = _rightDiffFile.FilePath;

                string leftContent = _leftDiffFile.ReadFromFile();
                string rightContent = _rightDiffFile.ReadFromFile();

                bool result=  await CompareTextContent(leftContent, rightContent);
                if (result)
                {
                    string leftName = Path.GetFileName(leftPath);
                    string rightName = Path.GetFileName(rightPath);
                    _name = leftName.Equals(rightName) ? leftName : $"{leftName}/{rightName}";
                    string oldName = _name;
                    while (mainModel.HasSameWindow(_name, this))
                    {
                        _name = $"{oldName} - {Guid.NewGuid().ToString().Substring(0, 6)}";
                    }
                }
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.ToString());
                mainModel.RemoveTab(this);
            }
        }

        //对比文本
        private Task<bool> CompareTextContent(string leftContent, string rightContent)
        {
            var taskResult = new TaskCompletionSource<bool>();

            Task.Run(() => {
                _loading = true;
                _showCompare = false;
                if (!string.IsNullOrEmpty(leftContent) && !string.IsNullOrEmpty(rightContent))
                {
                    _sideModel = SideBySideDiffBuilder.Diff(leftContent, rightContent);
                    _showCompare = _sideModel != null;
                    if (_showCompare)
                    {
                        _leftDiffFile.Setup(_sideModel.OldText);
                        _rightDiffFile.Setup(_sideModel.NewText);
                    }
                }
                _loading = false;
                taskResult.SetResult(_showCompare);
            });

            return taskResult.Task;
        }

        //复制文本
        private void CopySection(int lineNo,DiffFile srcDiffFile)
        {
            Console.WriteLine($"{lineNo} {srcDiffFile.FilePath}");
            DiffFile targetDiffFile = srcDiffFile == _leftDiffFile ? _rightDiffFile : _leftDiffFile;
            Task.Run(()=> {
                targetDiffFile.SetSectionLines(lineNo, srcDiffFile.GetSectionLines(lineNo));
                CompareTextContent(_leftDiffFile.TextResult.ToString(), _rightDiffFile.TextResult.ToString());
            });
        }

        //设置文本的状态
        private void SetTextEditorStatus()
        {
            _leftDiffFile.TextEditor.readOnly = _readOnly;
            _rightDiffFile.TextEditor.readOnly = _readOnly;
        }

        public override void Dispose()
        {
            _leftDiffFile?.Dispose();
            _rightDiffFile?.Dispose();
        }


    }
}
