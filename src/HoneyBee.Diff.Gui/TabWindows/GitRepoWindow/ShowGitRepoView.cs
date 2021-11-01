using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoneyBee.Diff.Gui
{
    public class ShowGitRepoView:GitRepoView
    {
        public void SetRepoPath(string repoPath)
        {
            RepoPath = repoPath;
        }
    }
}
