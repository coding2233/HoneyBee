using ImGuiNET;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoneyBee.Diff.Gui
{
    public class WorkTreeView
    {
        private SplitView _horizontalSplitView = new SplitView(SplitView.SplitType.Horizontal, 2, 600, 0.8f);
        private SplitView _verticalSplitView = new SplitView(SplitView.SplitType.Vertical);

        public void OnDraw(RepositoryStatus statuses)
        {
            if (statuses != null)
            {
                foreach (var item in statuses)
                {
                    ImGui.Text(item.FilePath);
                    //ImGui.Text(item.HeadToIndexRenameDetails.ToString());
                    //ImGui.Text(item.IndexToWorkDirRenameDetails.ToString());
                    ImGui.Text(item.State.ToString());
                    ImGui.Text("-------------------------------");
                }
            }
        }
    }
}
