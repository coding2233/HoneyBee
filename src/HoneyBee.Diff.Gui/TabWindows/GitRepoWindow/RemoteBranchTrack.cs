using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoneyBee.Diff.Gui
{
    public class RemoteBranchTrack
    {
        public string[] Remotes { get; private set; }
        public int RemoteIndex= -1;
        public string[] RemoteBranchs { get; private set; }
        public int RemoteBranchIndex= -1;
        public string RemoteUrl { get; private set; } = "";
        public string[] LocalBranchs { get; private set; }
        public int LocalBranchIndex = -1;

        private string CurrentRepositoryHeadBranch;
        //Names
        public string Remote => Remotes[RemoteIndex];
        public string RemoteBranch => RemoteBranchs[RemoteBranchIndex];
        public string LocalBranch => LocalBranchs[LocalBranchIndex];

        public void GetData(Git git)
        {
            RemoteIndex = -1;
            RemoteBranchIndex = -1;
            LocalBranchIndex = -1;

            //Local branchs
            List<string> localBranchs = new List<string>();
            foreach (var item in git.LocalBranchNodes)
            {
                localBranchs.AddRange(GetBranchCombo(item));
            }
            LocalBranchs = localBranchs.ToArray();
            for (int i = 0; i < LocalBranchs.Length; i++)
            {
                if (LocalBranchs[i].Equals(CurrentRepositoryHeadBranch))
                {
                    LocalBranchIndex = i;
                    break;
                }
            }

            //Remotes
            List<string> remoteNames = new List<string>();
            foreach (var item in git.Remotes)
            {
                remoteNames.Add(item.Name);
                if (RemoteBranchIndex == -1)
                {
                    if (item.Name.Equals("origin"))
                    {
                        RemoteIndex = remoteNames.Count - 1;
                        RemoteUrl = item.PushUrl;
                    }
                }
            }
            Remotes = remoteNames.ToArray();

            //Remote branchs
            UpdateRemote(git);
        }

        public void UpdateRemote(Git git)
        {
            RemoteUrl = git.Remotes.ToArray()[RemoteIndex].PushUrl;

            //Remote branchs
            string remoteName = Remotes[RemoteIndex];
            var remoteBranchNode = git.RemoteBranchNodes.Find(x => x.Name.Equals(remoteName));
            List<string> remoteBranchs = GetBranchCombo(remoteBranchNode);
            RemoteBranchs = remoteBranchs.ToArray();

            UpdateRemoteBranchIndex();
        }

        public void UpdateRemoteBranchIndex()
        {
            if (RemoteBranchIndex==-1 && LocalBranchIndex >= 0)
            {
                string localBranchFullName = $"{Remotes[RemoteIndex]}/{LocalBranchs[LocalBranchIndex]}";
                for (int i = 0; i < RemoteBranchs.Length; i++)
                {
                    if (RemoteBranchs[i].Equals(localBranchFullName))
                    {
                        RemoteBranchIndex = i;
                        break;
                    }
                }
            }
        }

        public bool Check()
        {
            return RemoteIndex >= 0 && RemoteBranchIndex >= 0 && LocalBranchIndex >= 0;
        }

        private List<string> GetBranchCombo(BranchNode branchNode)
        {
            List<string> branchs = new List<string>();
            if (branchNode.Branch != null)
            {
                branchs.Add(branchNode.FullName);
                if (branchNode.Branch.IsCurrentRepositoryHead)
                {
                    CurrentRepositoryHeadBranch = branchNode.FullName;
                }
            }
            else
            {
                if (branchNode.Children != null)
                {
                    foreach (var item in branchNode.Children)
                    {
                        branchs.AddRange(GetBranchCombo(item));
                    }
                }
            }
            return branchs;
        }
    }
}
