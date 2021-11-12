using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoneyBee.Diff.Gui
{
    
    public class DiffFolderNode
    {
        public string Name;
        public string FullName;
        public string FullPath;
        public bool Expansion;
        public bool IsFolder;
        public DiffFolderNode Parent;
        public List<DiffFolderNode> ChildrenNodes;
        public bool IsEmpty;
        public long Size=0;
        public string SizeString = "--";
        public string UpdateTime="--";
        public string MD5="";
        public DiffStatus Status;
        public bool ChildrenHasDiff;

        public bool FindChildren => ChildrenNodes != null;


        public DiffFolderNode(DiffFolderNode parent,string name,string fullName,bool isFolder,bool isEmpty=true)
        {
            IsEmpty = isEmpty;
            FullPath = name;
            Name = Path.GetFileName(name);
            FullName =(string.IsNullOrEmpty(fullName)?".": $"{fullName}/{Name}").Replace("\\","/");
            IsFolder = isFolder;
            Expansion = false;
            Parent = parent;
            if (parent == null)
            {
                GetChildren();
            }
            else
            {
                if (isFolder)
                    Status = DiffStatus.Unknown;
            }
        }

        public void CopyToEmptyNodeWithChildren(DiffFolderNode parent)
        {
            parent.ChildrenNodes = new List<DiffFolderNode>();
            foreach (var item in ChildrenNodes)
            {
                var newEmptyChild = item.CopyToEmptyNode(parent);
                parent.ChildrenNodes.Add(newEmptyChild);
            }
        }

        public DiffFolderNode CopyToEmptyNode(DiffFolderNode parent)
        {
            DiffFolderNode emptyNode = new DiffFolderNode(parent, Name,Path.GetDirectoryName(FullName),IsFolder,true);
            return emptyNode;
        }


        public void GetChildren()
        {
            if (!IsEmpty && IsFolder)
            {
                ChildrenNodes = new List<DiffFolderNode>();
                string dirPath = FullPath;

                var dirs = Directory.GetDirectories(dirPath);
                if (dirs != null)
                {
                    List<DiffFolderNode> folderNodes = new List<DiffFolderNode>();
                    foreach (var item in dirs)
                    {
                        DiffFolderNode dirNode = new DiffFolderNode(this,item, this.FullName, true, false);
                        folderNodes.Add(dirNode);
                        this.Size += dirNode.Size;
                        dirNode.UpdateTime = Directory.GetLastWriteTime(item).ToString("yyyy-MM-dd HH:mm");
                    }
                    this.ChildrenNodes.AddRange(folderNodes.OrderBy(x => x.Name));
                }

                var files = Directory.GetFiles(dirPath);
                if (files != null)
                {
                    List<DiffFolderNode> filesNodes = new List<DiffFolderNode>();
                    foreach (var item in files)
                    {
                        DiffFolderNode fileNode = new DiffFolderNode(this,item, this.FullName,false,false);
                        fileNode.Size = GetFileLength(item);
                        fileNode.SizeString = ToSizeString(fileNode.Size);
                        fileNode.MD5 = GetFileMD5(item);
                        fileNode.UpdateTime = File.GetLastWriteTime(item).ToString("yyyy-MM-dd HH:mm");
                        filesNodes.Add(fileNode);
                        this.Size += fileNode.Size;
                    }
                    this.ChildrenNodes.AddRange(filesNodes.OrderBy(x => x.Name));
                }
                this.SizeString = ToSizeString(this.Size);

                Parent?.UpdateStatus();
            }
            
        }

        public void UpdateStatus()
        {
            HashSet<DiffStatus> childrenStatus = new HashSet<DiffStatus>();
            Size = 0;
            foreach (var item in ChildrenNodes)
            {
                if (!item.IsEmpty)
                {
                    Size += item.Size;
                    childrenStatus.Add(item.Status);
                }
            }

            Status = DiffStatus.Same;
            ChildrenHasDiff = false;

            if (childrenStatus.Contains(DiffStatus.Unknown))
            {
                SizeString = $"{ToSizeString(Size)}+";
                Status = DiffStatus.Unknown;
                ChildrenHasDiff = true;
            }
            else
            {
                SizeString = $"{ToSizeString(Size)}";
                childrenStatus.Remove(DiffStatus.Same);
                if (childrenStatus.Count > 0)
                {
                    Status = DiffStatus.Modified;
                    ChildrenHasDiff = true;
                }
            }

            if (Parent != null)
            {
                Parent.UpdateStatus();
            }
        }

        private long GetFileLength(string filePath)
        {
            try
            {
                using (var fileStream = File.OpenRead(filePath))
                {
                    return fileStream.Length;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"{filePath}  {e}");
                return 0;
            }
        }

        //获取文件的md5值
        public string GetFileMD5(string filePath)
        {
            try
            {
                using (var fileStream = File.OpenRead(filePath))
                {
                    System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                    byte[] toData = md5.ComputeHash(fileStream);
                    string fileMD5 = BitConverter.ToString(toData).Replace("-", "").ToLower();
                    return fileMD5;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"{filePath}  {e}");
                return filePath;
            }
        }

        //获取文件大小的显示
        public string ToSizeString(long size)
        {
            if (size < 1024)
            {
                return $"{size} Byte";
            }
            else if (size < 1024 * 1024)
            {
                return $"{(size / 1024.0f).ToString("f2")} KB";
            }
            else if (size < 1024 * 1024 * 1024)
            {
                return $"{(size / 1024.0f / 1024.0f).ToString("f2")} MB";
            }
            else
            {
                return $"{(size / 1024.0f / 1024.0f / 1024.0f).ToString("f2")} GB";
            }
        }

    }
}
