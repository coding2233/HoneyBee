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
        private StatusEntry _selectStatusEntry;
        private string _statusContent;
        public TextEditor _statusTextEditor = new TextEditor();

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
                if (ImGui.RadioButton($"{statusIcon} {item.FilePath}", _selectStatusEntry == item))
                {
                    _selectStatusEntry = item;
                    var diffContent = diff.Compare<Patch>(new List<string> { item.FilePath },true);
                    _statusContent = diffContent.Content;
                    _statusTextEditor.text = _statusContent;
                }
            }
        }

        private void DrawDiff()
        {
            _statusTextEditor.Render("Diff",ImGui.GetWindowSize());
            //if (!string.IsNullOrEmpty(_statusContent))
            //{
            //    ImGui.Text(_statusContent);
            //}
        }
    }
}
