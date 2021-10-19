using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoneyBee.Diff.Gui
{ 
    public class DiffFile
    {
        public string FilePath="";
        public List<DiffFileLine> Lines = new List<DiffFileLine>();

        public List<DiffFileLine> GetLines()
        {
            Lines.Clear();
            if (!string.IsNullOrEmpty(FilePath) && File.Exists(FilePath))
            {
                var lines = File.ReadAllLines(FilePath);
                for (int i = 0; i < lines.Length; i++)
                {
                    var newDiffLine = new DiffFileLine(lines[i]);
                    newDiffLine.Number = i + 1;
                    Lines.Add(newDiffLine);
                }
            }
            return Lines;
        }

        public bool GetDiffFlag(DiffFile other)
        {
            var thisLines = this.GetLines();
            var otherLines = other.GetLines();

            SetDiffFileLineStatue(thisLines, otherLines);
            SetDiffFileLineStatue(otherLines, thisLines);

            AddDiffFileLineEmpty(thisLines, otherLines);
            AddDiffFileLineEmpty(otherLines, thisLines);

            return true;
        }


        private void SetDiffFileLineStatue(List<DiffFileLine> thisLines, List<DiffFileLine> otherLines)
        {
            int otherIndex = 0;
            List<DiffFileLine> modifiedLines = new List<DiffFileLine>();
            for (int i = 0; i < thisLines.Count; i++)
            {
                for (int j = otherIndex; j < otherLines.Count; j++)
                {
                    var tl = thisLines[i];
                    var ol = otherLines[j];
                    if (tl.Content.Equals(ol.Content))
                    {
                        tl.Status = DiffStatus.Same;
                        if (modifiedLines.Count > 0)
                        {
                            bool modified = j > otherIndex;
                            foreach (var item in modifiedLines)
                            {
                                item.Status = modified ? DiffStatus.Modified : DiffStatus.Add;
                            }
                            modifiedLines.Clear();
                        }
                        otherIndex = i + 1;
                        break;
                    }
                    else
                    {
                        if (j == otherLines.Count - 1)
                        {
                            modifiedLines.Add(tl);
                            //tl.Status = DiffStatus.Add;
                        }
                    }
                }
            }

            if (modifiedLines.Count > 0)
            {
                foreach (var item in modifiedLines)
                {
                    item.Status = DiffStatus.Add;
                }
                modifiedLines.Clear();
            }
        }

        private void AddDiffFileLineEmpty(List<DiffFileLine> thisLines, List<DiffFileLine> otherLines)
        {
            for (int i = 0; i < otherLines.Count; i++)
            {
                if (otherLines[i].Status == DiffStatus.Add)
                {
                    if (i < thisLines.Count)
                    {
                        thisLines.Insert(i, new DiffFileLine());
                    }
                    else
                    {
                        thisLines.Add(new DiffFileLine());
                    }
                }
            }
        }
    }


   
}
