using ImGuiNET;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoneyBee.Diff.Gui
{
    public class ShowCommitView
    {
        private SplitView _horizontalSplitView = new SplitView(SplitView.SplitType.Horizontal,2,600,0.6f);
        private SplitView _verticalSplitView = new SplitView(SplitView.SplitType.Vertical);
        StringBuilder _tempStringBuilder = new StringBuilder();
        private TreeEntry _selectTreeEntry;

        public void DrawSelectCommit(LibGit2Sharp.Diff diff, Commit commit,Commit parentCommit)
        {
            _horizontalSplitView.Begin();
            _verticalSplitView.Begin();
            OnDrawCommitInfo(commit);
            _verticalSplitView.Separate();
            OnDrawCommitTree(diff,commit.Tree, parentCommit==null?null: parentCommit.Tree);
            _verticalSplitView.End();
            _horizontalSplitView.Separate();
            OnDrawDiff();
            _horizontalSplitView.End();
        }

        private void OnDrawCommitInfo(Commit commit)
        {
            ImGui.Text($"Sha: {commit.Sha}");
            _tempStringBuilder.Clear();
            _tempStringBuilder.Append("Parents:");
            if (commit.Parents != null)
            {
                foreach (var item in commit.Parents)
                {
                    _tempStringBuilder.Append($" {item.Sha.Substring(0,10)}");
                }
            }
            ImGui.Text(_tempStringBuilder.ToString());
            ImGui.Text($"Author: {commit.Author.Name} {commit.Author.Email}");
            ImGui.Text($"DateTime: {commit.Author.When.ToString()}");
            ImGui.Text($"Committer: {commit.Committer.Name} {commit.Committer.Email}\n");

            ImGui.Text(commit.Message);
        }

        private void OnDrawCommitTree(LibGit2Sharp.Diff diff,Tree trees,Tree parentTrees)
        {
           var result=  diff.Compare<TreeChanges>(parentTrees, trees);
            foreach (TreeEntryChanges c in result)
            {
                //Console.WriteLine(c);
            }
            //需要对比两个提交的差异
            foreach (var item in trees)
            {
                if (ImGui.RadioButton(item.Path, _selectTreeEntry == item))
                {
                    _selectTreeEntry = item;
                }
            }
        }

        private void OnDrawDiff()
        {
            if (_selectTreeEntry != null)
            {
                ImGui.Text(_selectTreeEntry.Mode.ToString());
                ImGui.Text(_selectTreeEntry.Name);
                ImGui.Text(_selectTreeEntry.Path);
                ImGui.Text(_selectTreeEntry.Target.ToString());
                ImGui.Text(_selectTreeEntry.TargetType.ToString());
            }
        }

    }
}
