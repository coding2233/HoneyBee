using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoneyBee.Diff.Gui
{
    public class BranchNode
    {
        public string Name;
        public string FullName;
        public Branch Branch;
        public List<BranchNode> Children;
        public int BehindBy;
        public int AheadBy;

        public void UpdateByIndex()
        {
            AheadBy = 0;
            BehindBy = 0;
            if (Children != null)
            {
                foreach (var item in Children)
                {
                    item.UpdateByIndex();
                    AheadBy += item.AheadBy;
                    BehindBy += item.BehindBy;
                }
            }
            if (Branch != null && Branch.IsTracking)
            {
                var trackingDetails = Branch.TrackingDetails;
                BehindBy += (int)trackingDetails.BehindBy;
                AheadBy += (int)trackingDetails.AheadBy;
            }
        }
    }

    public class Git
    {
        private Repository _repository;

        public string RepoName { get; private set; }
        public string RepoPath { get; private set; }
        public RepositoryStatus CurrentStatuses { get; private set; }
        public List<BranchNode> LocalBranchNodes { get; private set; } = new List<BranchNode>();
        public List<BranchNode> RemoteBranchNodes { get; private set; } = new List<BranchNode>();
        public List<Commit> HistoryCommits { get; private set; } = new List<Commit>();
        public TagCollection Tags => _repository.Tags;
        public SubmoduleCollection Submodules => _repository.Submodules;
        public LibGit2Sharp.Diff Diff => _repository.Diff;

        public Git(string repoPath)
        {
            Task.Run(()=> {
                RepoPath = repoPath;
                RepoName = Path.GetFileNameWithoutExtension(repoPath);
                _repository = new Repository(repoPath);

                foreach (var branch in _repository.Branches)
                {
                    string[] nameArgs = branch.FriendlyName.Split('/');
                    Queue<string> nameTree = new Queue<string>();
                    foreach (var item in nameArgs)
                    {
                        nameTree.Enqueue(item);
                    }
                    if (branch.IsRemote)
                    {
                        JointBranchNode(RemoteBranchNodes, nameTree, branch);
                    }
                    else
                    {
                        JointBranchNode(LocalBranchNodes, nameTree, branch);
                    }
                }
                foreach (var item in LocalBranchNodes)
                {
                    item.UpdateByIndex();
                }
                CurrentStatuses = _repository.RetrieveStatus();
                HistoryCommits = _repository.Commits.ToList();

            });
        }

        public void Status()
        {
            CurrentStatuses = null;
            Task.Run(()=> {
                CurrentStatuses = _repository.RetrieveStatus();
            });
        }

        private void JointBranchNode(List<BranchNode> branchNodes, Queue<string> nameTree, Branch branch)
        {
            if (nameTree.Count == 1)
            {
                BranchNode branchNode = new BranchNode();
                branchNode.Name = nameTree.Dequeue();
                branchNode.FullName = branch.FriendlyName;
                branchNode.Branch = branch;
                branchNodes.Add(branchNode);
            }
            else
            {
                string name = nameTree.Dequeue();
                var findNode = branchNodes.Find(x => x.Name.Equals(name));
                if (findNode == null)
                {
                    findNode = new BranchNode();
                    findNode.Name = name;
                    findNode.Children = new List<BranchNode>();
                    branchNodes.Add(findNode);
                }
                JointBranchNode(findNode.Children, nameTree, branch);
            }
        }
    }
}
