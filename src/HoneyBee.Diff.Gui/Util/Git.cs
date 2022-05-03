using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
        private static SynchronizationContext s_mainSynchronizationContext;

        private static SynchronizationContext mainSyncContext
        {
            get
            {
                if(s_mainSynchronizationContext==null)
                {
                    s_mainSynchronizationContext = SynchronizationContext.Current;
                    if (s_mainSynchronizationContext == null)
                    {
                        s_mainSynchronizationContext = new SynchronizationContext();
                    }
                }
                return s_mainSynchronizationContext;
            }
        }

        private static GitCredentials s_credentials;
        public static GitCredentials Credentials
        {
            get
            {
                if (s_credentials == null)
                {
                    s_credentials=new GitCredentials();
                }
                return s_credentials;
            }
        }

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

        public void Pull()
        {
            if (_repository == null)
                return;

            // Credential information to fetch
            LibGit2Sharp.PullOptions options = new LibGit2Sharp.PullOptions();
            options.FetchOptions = new FetchOptions();
            //if (Credentials.Has(remoteUrl))
            //{
            //    var cred = Credentials.Get(remoteUrl);
            //    options.FetchOptions.CredentialsProvider = new CredentialsHandler(
            //    (url, usernameFromUrl, types) =>
            //        new UsernamePasswordCredentials()
            //        {
            //            Username = cred.UserName,
            //            Password = cred.Password
            //        });
            //}

            // User information to create a merge commit
            var signature = new LibGit2Sharp.Signature(
                new Identity("MERGE_USER_NAME", "MERGE_USER_EMAIL"), DateTimeOffset.Now);

            // Pull
            Commands.Pull(_repository, signature, options);
        }

        public static void Clone(string remoteUrl,string localDirectory,Action<string> logCallback,Action<string> complete)
        {
            Task.Run(() =>
            {
                var co = new CloneOptions();
                co.RecurseSubmodules = true;
                if (Credentials.Has(remoteUrl))
                {
                    var cred = Credentials.Get(remoteUrl);
                    co.CredentialsProvider = (_url, _user, _cred) => new UsernamePasswordCredentials { Username = cred.UserName, Password = cred.Password };
                }
                co.OnProgress = (serverProgressOutput)=> {
                    //mainSyncContext.Post((state)=> { 
                        logCallback?.Invoke(serverProgressOutput);
                    //},null);
                    return true; };
                string result = Repository.Clone(remoteUrl, localDirectory, co);
                mainSyncContext.Post((state)=> { 
                    complete?.Invoke(result);
                }, null);
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


    public class GitCredentials
    {

        [Import]
        public IUserSettingsModel UserSettings { get; set; }

        public GitCredentials()
        {
            DiffProgram.ComposeParts(this);
        }

        public bool Has(string url)
        {
            url = Escape(url);
            return UserSettings.Has<Credentials>(url);
        }

        public void Set(string url, string userName, string password)
        {
            url = Escape(url);
            Credentials credentials = new Credentials() { UserName =userName,Password=password};
            UserSettings.Set<Credentials>(url, credentials);
        }

        public Credentials Get(string url)
        {
            url = Escape(url);
            return UserSettings.Get<Credentials>(url);
        }

        private string Escape(string url)
        {
            return System.Web.HttpUtility.HtmlEncode(url);
        }

        public struct Credentials
        {
            public string UserName { get; set; }
            public string Password { get; set; }
        }
    }
}
