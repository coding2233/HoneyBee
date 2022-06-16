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
        private HashSet<string> _selectStageFiles = new HashSet<string>();
        private HashSet<string> _selectUnstageFiles = new HashSet<string>();
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
                if (!string.IsNullOrEmpty(_commit))
                {
                    git.Commit(_commit);
                }
                _commit = "";
            }
        }

        private void DrawStatus(Git git, RepositoryStatus statuses, LibGit2Sharp.Diff diff)
        {
            IEnumerable<StatusEntry> stageStatusEntries=null;

            if (statuses != null)
            {
                stageStatusEntries = statuses.Staged;
            }

            _verticalSplitView.Begin();
            DrawStageFilesStatus(git, stageStatusEntries,diff);
            _verticalSplitView.Separate();
            DrawUnstageFileStatus(git, statuses, diff);
            _verticalSplitView.End();
      
        }

        private void DrawStageFilesStatus(Git git, IEnumerable<StatusEntry> statuses, LibGit2Sharp.Diff diff)
        {
            if (ImGui.Button("Unstage All"))
            {
                git.Restore();
                ClearSelectFiles();
            }
            ImGui.SameLine();
            if (ImGui.Button("Unstage Selected"))
            {
                git.Restore(_selectStageFiles.ToList());
                ClearSelectFiles();
            }

            if (statuses != null)
            {
                foreach (var item in statuses)
                {
                    DrawStatusFile(item, _selectStageFiles, diff);
                }
            }
        }

        private void DrawUnstageFileStatus(Git git, RepositoryStatus statuses, LibGit2Sharp.Diff diff)
        {
            if (ImGui.Button("Stage All"))
            {
                git.Add();
                ClearSelectFiles();
            }
            ImGui.SameLine();
            if (ImGui.Button("Stage Selected"))
            {
                git.Add(_selectUnstageFiles.ToList());
                ClearSelectFiles();
            }

            //files
            if (statuses!=null)
            {
                foreach (var item in statuses)
                {
                    if (statuses.Staged.Contains(item))
                        continue;
                    DrawStatusFile(item, _selectUnstageFiles, diff);
                }
            }
        }
        private void DrawDiff()
        {
            _statusTextEditor.Render("Diff",ImGui.GetWindowSize());
        }

        //绘制单独的文件
        private void DrawStatusFile(StatusEntry statusEntry, HashSet<string> selectFiles, LibGit2Sharp.Diff diff)
        {
            if (statusEntry == null || statusEntry.State==FileStatus.Ignored)
                return;

            //?
            string statusIcon = Icon.Get(Icon.Material_question_mark);
            switch (statusEntry.State)
            {
                case FileStatus.NewInIndex:
                case FileStatus.NewInWorkdir:
                    statusIcon = Icon.Get(Icon.Material_fiber_new);
                    break;
                case FileStatus.DeletedFromIndex:
                case FileStatus.DeletedFromWorkdir:
                    statusIcon = Icon.Get(Icon.Material_delete);
                    break;
                case FileStatus.RenamedInIndex:
                case FileStatus.RenamedInWorkdir:
                    statusIcon = Icon.Get(Icon.Material_edit_note);
                    break;
                case FileStatus.ModifiedInIndex:
                case FileStatus.ModifiedInWorkdir:
                    statusIcon = Icon.Get(Icon.Material_update);
                    break;
                case FileStatus.TypeChangeInIndex:
                case FileStatus.TypeChangeInWorkdir:
                    statusIcon = Icon.Get(Icon.Material_change_circle);
                    break;
                case FileStatus.Conflicted:
                    statusIcon = Icon.Get(Icon.Material_warning);
                    break;
                default:
                    break;
            }

            //checkbox 
            bool active = selectFiles.Contains(statusEntry.FilePath);
            if (ImGui.Checkbox($"{statusIcon} {statusEntry.FilePath}", ref active))
            {
                if (active)
                {
                    selectFiles.Add(statusEntry.FilePath);
                }
                else
                {
                    selectFiles.Remove(statusEntry.FilePath);
                }

                string statusContent = "";
                if (selectFiles.Count > 0)
                {
                    var diffContent = diff.Compare<Patch>(selectFiles, true);
                    statusContent = diffContent.Content;
                }
                _statusTextEditor.text = statusContent;
            }
        }

        private void ClearSelectFiles()
        {
            _selectStageFiles.Clear();
            _selectUnstageFiles.Clear();
        }
    }
}
