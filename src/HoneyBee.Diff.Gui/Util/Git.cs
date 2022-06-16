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
        public string RepoRootPath { get; private set; }
        public Repository Repository => _repository;
        public RepositoryStatus CurrentStatuses { get; private set; }
        public List<BranchNode> LocalBranchNodes { get; private set; } = new List<BranchNode>();
        public List<BranchNode> RemoteBranchNodes { get; private set; } = new List<BranchNode>();
        public List<Commit> HistoryCommits { get; private set; } = new List<Commit>();
        public TagCollection Tags => _repository.Tags;
        public SubmoduleCollection Submodules => _repository.Submodules;
        public LibGit2Sharp.Diff Diff => _repository.Diff;
        public Signature _signatureAuthor;
        public Signature SignatureAuthor
        {
            get {
                if (_signatureAuthor == null)
                {
                    _signatureAuthor = _repository.Config.BuildSignature(DateTimeOffset.Now);
                }
                return _signatureAuthor;
            }
        }

        public Git(string repoPath)
        {
            RepoPath = repoPath;
            RepoName = Path.GetFileNameWithoutExtension(repoPath);
            RepoRootPath = repoPath.EndsWith(".git") ? Path.GetDirectoryName(repoPath) : repoPath;
            _repository = new Repository(repoPath);

            UpdateStatus();
        }


        public void Stage(IEnumerable<string> files = null)
        {
            if (files == null)
            {
                Commands.Stage(_repository, "*");
            }
            else
            {
                if(files.Count()>0)
                    Commands.Stage(_repository, files);
            }
            Status();
        }

        public void Unstage(IEnumerable<string> files = null)
        {
            if (files == null)
            {
                Commands.Unstage(_repository, "*");
            }
            else
            {
                if (files.Count() > 0)
                    Commands.Unstage(_repository, files);
            }
            Status();
        }

        public void Restore(IEnumerable<string> files)
        {
            if (files == null || files.Count() == 0)
                return;

            var options = new CheckoutOptions { CheckoutModifiers = CheckoutModifiers.Force };
            _repository.CheckoutPaths(_repository.Head.FriendlyName, files, options);
        }

        public void Status()
        {
            CurrentStatuses = null;
            Task.Run(()=> {
                CurrentStatuses = _repository.RetrieveStatus();
            });
        }

      

        public void UpdateHistory()
        {
            Task.Run(() => {

                LocalBranchNodes.Clear();
                RemoteBranchNodes.Clear();
                
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
                HistoryCommits = _repository.Commits.ToList();
            });
        }

        public void UpdateStatus()
        {
            Status();
            UpdateHistory();
        }

        public void Commit(string message)
        {
            if (string.IsNullOrEmpty(message))
                return;

            _signatureAuthor = _repository.Config.BuildSignature(DateTimeOffset.Now);
            _repository.Commit(message, _signatureAuthor, _signatureAuthor);

            UpdateStatus();
        }

        public void Pull(Action<string> onLogCallback,Action<MergeResult> onCompleteCallback)
        {
            if (_repository == null)
                return;

            Task.Run(()=> {
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

                options.FetchOptions.OnProgress = (serverProgressOutput) => {
                    mainSyncContext.Post((state) => {
                        onLogCallback?.Invoke(serverProgressOutput);
                    }, null);
                    return true;
                };
                options.FetchOptions.OnTransferProgress = (progress) => {
                    int indexedProgress = (int)((progress.IndexedObjects / (float)progress.ReceivedObjects) * 100);
                    int receiveProgress = (int)((progress.ReceivedObjects / (float)progress.TotalObjects) * 100);
                    string transferProgressLog = $"Receiving objects: {receiveProgress}% ({progress.ReceivedObjects}/{progress.TotalObjects}), {progress.ReceivedBytes.ToSizeString()}" +
                        $", Resolving deltas: {indexedProgress}% ({progress.IndexedObjects}/{progress.ReceivedObjects})";
                    mainSyncContext.Post((state) =>
                    {
                        onLogCallback?.Invoke(transferProgressLog);
                    }, null);
                    return true;
                };
                options.FetchOptions.OnUpdateTips = (refName, oldId, newId) => {
                    mainSyncContext.Post((state) =>
                    {
                        onLogCallback?.Invoke($"{oldId}->{newId} , {refName}");
                    }, null);
                    return true; };

                // User information to create a merge commit
                var signature = new LibGit2Sharp.Signature(
                    new Identity("MERGE_USER_NAME", "MERGE_USER_EMAIL"), DateTimeOffset.Now);

                // Pull
                var mergeResult = Commands.Pull(_repository, signature, options);
                onCompleteCallback?.Invoke(mergeResult);
            });
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
               
                co.OnProgress = (serverProgressOutput) => {
                    mainSyncContext.Post((state) =>
                    {
                        logCallback?.Invoke(serverProgressOutput);
                    }, null);
                    return true;
                };
                co.OnTransferProgress = (progress) => {
                    int indexedProgress = (int)((progress.IndexedObjects / (float)progress.ReceivedObjects) * 100);
                    int receiveProgress = (int)((progress.ReceivedObjects / (float)progress.TotalObjects) * 100);
                    string transferProgressLog = $"Receiving objects: {receiveProgress}% ({progress.ReceivedObjects}/{progress.TotalObjects}), {progress.ReceivedBytes.ToSizeString()}" +
                    $", Resolving deltas: {indexedProgress}% ({progress.IndexedObjects}/{progress.ReceivedObjects})";
                    mainSyncContext.Post((state) =>
                    {
                        logCallback?.Invoke(transferProgressLog);
                    }, null);
                    return true; };
                co.OnCheckoutProgress = (path, completedSteps, totalSteps) => {
                    int checkProgress = (int)((completedSteps / (float)totalSteps) * 100);
                    string checkLog = $"Checkout files: {checkProgress}% ({completedSteps}/{totalSteps}), {path}";
                    mainSyncContext.Post((state) =>
                    {
                        logCallback?.Invoke(checkLog);
                    }, null);
                };
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
