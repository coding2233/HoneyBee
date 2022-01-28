using ImGuiNET;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace HoneyBee.Diff.Gui
{
    public class WorkTreeView
    {
        private SplitView _horizontalSplitView = new SplitView(SplitView.SplitType.Horizontal, 2, 600, 0.8f);
        private SplitView _verticalSplitView = new SplitView(SplitView.SplitType.Vertical);
        public TextEditor _statusTextEditor = new TextEditor();
        private HashSet<string> _selectFilePaths = new HashSet<string>();

        public void OnDraw(RepositoryStatus statuses, LibGit2Sharp.Diff diff)
        {
            _horizontalSplitView.Begin();
            DrawStatus(statuses, diff);
            _horizontalSplitView.Separate();
            DrawDiff();
            _horizontalSplitView.End();
           
        }

        private void DrawStatus(RepositoryStatus statuses, LibGit2Sharp.Diff diff)
        {
            if (statuses == null)
                return;
            foreach (var item in statuses)
            {
                if (item.State == FileStatus.Ignored)
                    continue;

                string statusIcon = Icon.Get(Icon.Material_change_circle);
                switch (item.State)
                {
                    case FileStatus.NewInIndex:
                    case FileStatus.NewInWorkdir:
                        statusIcon = Icon.Get(Icon.Material_fiber_new);
                        break;
                    case FileStatus.DeletedFromIndex:
                    case FileStatus.DeletedFromWorkdir:
                        statusIcon = Icon.Get(Icon.Material_delete);
                        break;
                    default:
                        break;
                }

                bool active = _selectFilePaths.Contains(item.FilePath);
                if (ImGui.Checkbox($"{statusIcon} {item.FilePath}", ref active))
                {
                    if (active)
                    {
                        _selectFilePaths.Add(item.FilePath);
                    }
                    else
                    {
                        _selectFilePaths.Remove(item.FilePath);
                    }

                    string statusContent = "";
                    if (_selectFilePaths.Count > 0)
                    {
                        var diffContent = diff.Compare<Patch>(_selectFilePaths, true);
                        statusContent = diffContent.Content;
                    }
                    _statusTextEditor.text = statusContent;
                }
            }
        }

        private void DrawDiff()
        {
            _statusTextEditor.Render("Diff",ImGui.GetWindowSize());
        }
    }
}
