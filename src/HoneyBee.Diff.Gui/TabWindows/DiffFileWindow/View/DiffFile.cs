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
                flagPointList.Clear();

                flagPointList.Add(flagLines[i]);
                if (i == flagLines.Count - 1)
                {
                    flagPoints.Add(flagPointList[0]);
                    flagPointSequence.Add(flagPointList[0], flagPointList.ToArray());
                }
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
            if (!string.IsNullOrEmpty(TextEditor.text)
                && !string.IsNullOrEmpty(FilePath) && File.Exists(FilePath))
            {
                File.WriteAllText(FilePath,TextEditor.text);
            }
        }

        public void Dispose()
        {
            TextEditor?.Dispose();
        }

        public struct SideModelTextResult
        {
            //public string Text;
            public int[] FlagLines;
            public int[] FlagPoints;
            public Dictionary<int, int[]> FlagPointSequence;
        }
    }
}
