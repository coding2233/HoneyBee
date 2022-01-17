using ImGuiNET;
using LibGit2Sharp;
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
    public class GitRepoWindow : ITabWindow
    {
        public string Name
        {
            get
            {
                if (string.IsNullOrEmpty(_repoName))
                {
                    _repoName = Guid.NewGuid().ToString().Substring(0, 6);
                }
                return _repoName;
            }
        }

        public string IconName => Icon.Get(Icon.Material_gite);

        public bool Unsave => false;

        public bool ExitModal { get; set; }

        private bool _isGitRepo;
        private string _repoName;
        private string _repoPath="";
        private string repoPath
        {
            get
            {
                return _repoPath;
            }
            set
            {
                _repoPath = value;
                _repoName = string.Empty;
                if (!string.IsNullOrEmpty(_repoPath))
                {
                    _repoName = Path.GetFileName(Path.GetDirectoryName(_repoPath));
                }
                //检查是否为正常的Git仓库
                _isGitRepo = IsGitRepo();
                //显示View
                if (_gitRepoView != null)
                {
                    if (_isGitRepo.GetType() != (_isGitRepo ? typeof(GetGitRepoView) : typeof(ShowGitRepoView)))
                    {
                        _gitRepoView.Dispose();
                        _gitRepoView = null;
                    }
                }
                if (_gitRepoView == null)
                {
                    _gitRepoView = _isGitRepo ? new ShowGitRepoView() : new GetGitRepoView(GetGitRepoPath);
                }
                if (_isGitRepo)
                {
                    (_gitRepoView as ShowGitRepoView).SetRepoPath(_repoPath);
                }
            }
        }


        private GitRepoView _gitRepoView;

        [Import]
        public IMainWindowModel mainModel { get; set; }

        public GitRepoWindow()
        {
            DiffProgram.ComposeParts(this);
        }

        public void OnDraw()
        {
            if (_gitRepoView == null)
            {
                _gitRepoView = new GetGitRepoView(GetGitRepoPath);
            }
            else
            {
                _gitRepoView.Draw();
            }
        }

        public void OnExitModalSure()
        {
        }

        public string Serialize()
        {
            return repoPath;
        }

        public bool Deserialize(string data)
        {
            repoPath = data;
            return true;
        }


        private bool IsGitRepo()
        {
            return !string.IsNullOrEmpty(_repoPath) && Repository.IsValid(_repoPath);
        }

        private void GetGitRepoPath(string gitRepoPath)
        {
            repoPath = gitRepoPath;
            mainModel.SaveWindow(this);
        }

        public void Setup(params object[] parameters)
        {
        }
        public void Dispose()
        {
        }

    }
}
