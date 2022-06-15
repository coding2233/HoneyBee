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
        private string _commit="";

        public void OnDraw(Git git, RepositoryStatus statuses, LibGit2Sharp.Diff diff)
        {
            ImGui.BeginChild("WorkTreeView_Content",ImGui.GetWindowSize()-new Vector2(0,100));
            _horizontalSplitView.Begin();
            DrawStatus(git,statuses,diff);
            _horizontalSplitView.Separate();
            DrawDiff();
            _horizontalSplitView.End();
            ImGui.EndChild();

            ImGui.BeginChild("WorkTreeView_Commit");
            DrawCommit(git);
            ImGui.EndChild();
        }

        private void DrawCommit(Git git)
        {
            //ImGui.SetNextItemWidth(ImGui.GetWindowWidth());
            ImGui.InputTextMultiline("", ref _commit, 500,new Vector2(ImGui.GetWindowWidth(),70));
            ImGui.Text($"{git.SignatureAuthor.Name}<{git.SignatureAuthor.Email}>");
            ImGui.SameLine();
            if (ImGui.Button("Commit"))
            {
                if (string.IsNullOrEmpty(_commit))
                {
                    git.Commit(_commit);
                }
                _commit = "";
            }
        }

        private void DrawStatus(Git git, RepositoryStatus statuses, LibGit2Sharp.Diff diff)
        {
            HashSet<StatusEntry> stageEntries = new HashSet<StatusEntry>();
            HashSet<StatusEntry> unstageEntries = new HashSet<StatusEntry>();

            if (statuses != null)
            {
                foreach (StatusEntry item in statuses)
                {
                    if (item.State == FileStatus.Ignored)
                        continue;
                    if (item.State == FileStatus.ModifiedInIndex
                        || item.State == FileStatus.ModifiedInWorkdir)
                    {
                        stageEntries.Add(item);
                    }
                    else
                    {
                        unstageEntries.Add(item);
                    }
                }
            }

            _verticalSplitView.Begin();
            DrawStageFilesStatus(stageEntries);
            _verticalSplitView.Separate();
            DrawUnstageFileStatus(git, unstageEntries, diff);
            _verticalSplitView.End();
      
        }

        private void DrawStageFilesStatus(HashSet<StatusEntry> statuses)
        {
            //ImGui.Text("Stage files");
            foreach (var item in statuses)
            {
                ImGui.Text(item.FilePath);
            }
        }

        private void DrawUnstageFileStatus(Git git, HashSet<StatusEntry> statuses, LibGit2Sharp.Diff diff)
        {
                if (ImGui.Button("Stage All"))
                {
                    git.Add();
                }
                ImGui.SameLine();
                if (ImGui.Button("Stage Selected"))
                {
                    git.Add(_selectFilePaths.ToList());
                }
                //files
                foreach (StatusEntry item in statuses)
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
