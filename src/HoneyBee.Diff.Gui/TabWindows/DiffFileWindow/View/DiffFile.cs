using DiffPlex.DiffBuilder.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoneyBee.Diff.Gui
{
    public class DiffFile:IDisposable
    {
        public string FilePath;
        public SideModelTextResult TextResult { get; private set; } 

        public IntPtr IconIntPtr { get; private set; }

        public string IconTip { get; private set; } = "Copy the section to the other side.";

        public TextEditor TextEditor { get; private set; } = new TextEditor();

        public DiffPaneModel DiffModel { get; private set; }

        public bool IsTextChanged { get; private set; } = false;

        public DiffFile(string icon, string copyTip ="")
        {
            IconIntPtr = DiffProgram.GetOrCreateTexture(icon);
            if (!string.IsNullOrEmpty(copyTip))
            {
                IconTip = copyTip;
            }

            TextEditor.ignoreChildWindow = true;
        }

        public void Setup(DiffPaneModel diffModel)
        {
            DiffModel = diffModel;

            SideModelTextResult result = new SideModelTextResult();

            List<int> flagLines = new List<int>();
            List<int> flagPoints = new List<int>();
            StringBuilder stringBuilder = new StringBuilder();
            if (diffModel != null && diffModel.Lines != null)
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
                    stringBuilder.AppendLine(showText);
                }
            }
            result.FlagLines = flagLines.ToArray();

            List<int> flagPointList = new List<int>();
            var flagPointSequence = new Dictionary<int, int[]>();
            for (int i = 0; i < flagLines.Count; i++)
            {
                if (flagPointList.Count == 0 || flagPointList[flagPointList.Count - 1] + 1 == flagLines[i])
                {
                    flagPointList.Add(flagLines[i]);
                    if (i < flagLines.Count - 1)
                        continue;
                }

                flagPoints.Add(flagPointList[0]);
                flagPointSequence.Add(flagPointList[0], flagPointList.ToArray());

                if (i == flagLines.Count - 1)
                {
                    int lastNo = flagLines[flagLines.Count - 1];
                    if (!flagPointList.Contains(lastNo))
                    {
                        flagPoints.Add(lastNo);
                        flagPointSequence.Add(lastNo, new int[] { lastNo });
                    }
                }

                flagPointList.Clear();
                flagPointList.Add(flagLines[i]);
            }

            result.FlagPoints = flagPoints.ToArray();
            result.FlagPointSequence = flagPointSequence;
            TextResult = result;

            TextEditor.text = stringBuilder.ToString();
            TextEditor.flagLines = result.FlagLines;
            TextEditor.SetFlagPoints(result.FlagPoints);
        }

        public string ReadFromFile()
        {
            if (!string.IsNullOrEmpty(FilePath) && File.Exists(FilePath))
            {
                return File.ReadAllText(FilePath);
            }
            return string.Empty;
        }

        public void SaveFile()
        {
            if (!string.IsNullOrEmpty(FilePath) && File.Exists(FilePath))
            {
                if (DiffModel != null && DiffModel.Lines != null)
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    foreach (var item in DiffModel.Lines)
                    {
                        if(item.Type!=ChangeType.Deleted && item.Type!=ChangeType.Imaginary)
                            stringBuilder.AppendLine(item.Text);
                    }
                    File.WriteAllText(FilePath, stringBuilder.ToString());
                    IsTextChanged = false;
                }
            }
        }

        public DiffPiece[] GetSectionLines(int lineNo)
        {
            DiffPiece[] lines = null;
            if (TextResult.FlagPointSequence.TryGetValue(lineNo, out int[] lineNos))
            {
                lines = new DiffPiece[lineNos.Length];
                for (int i = 0; i < lineNos.Length; i++)
                {
                    lines[i] = DiffModel.Lines[lineNos[i]];
                }
            }
            return lines;
        }

        public bool SetSectionLines(int lineNo, DiffPiece[] lines)
        {
            if (TextResult.FlagPointSequence.TryGetValue(lineNo, out int[] lineNos))
            {
                for (int i = 0; i < lineNos.Length; i++)
                {
                    if (lineNos[i] < DiffModel.Lines.Count)
                    {
                        var targetLine = DiffModel.Lines[lineNos[i]];
                        targetLine.Text = lines[i].Text;
                        targetLine.Type = lines[i].Type;
                    }
                }
                IsTextChanged = true;
                return true;
            }
            return false;
        }

        public void Dispose()
        {
            TextEditor?.Dispose();
        }

        public struct SideModelTextResult
        {
            public int[] FlagLines;
            public int[] FlagPoints;
            public Dictionary<int, int[]> FlagPointSequence;
        }
    }
}
