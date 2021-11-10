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

        public DiffFolderNode(DiffFolderNode parent)
        {
            IsEmpty = true;
            Parent = parent;
        }

        public DiffFolderNode(DiffFolderNode parent,string name,string fullName,bool isFolder = false,bool expansion=false)
        {
            IsEmpty = false;
            Name = Path.GetFileName(name);
            FullName =string.IsNullOrEmpty(fullName)?".": $"{fullName}/{Name}";
            IsFolder = isFolder;
            Expansion = expansion;
            Parent = parent;
            if (parent == null)
            {
                GetChildren();
            }
        }

        public void GetChildren()
        {
            if (!IsEmpty && IsFolder)
            {
                ChildrenNodes = new List<DiffFolderNode>();
                string dirPath = FullName;

                var dirs = Directory.GetDirectories(dirPath);
                if (dirs != null)
                {
                    List<DiffFolderNode> folderNodes = new List<DiffFolderNode>();
                    foreach (var item in dirs)
                    {
                        DiffFolderNode dirNode = new DiffFolderNode(this,item, this.FullName, true, false);
                        folderNodes.Add(dirNode);
                        this.Size += dirNode.Size;
                    }
                    this.ChildrenNodes.AddRange(folderNodes.OrderBy(x => x.Name));
                }

                var files = Directory.GetFiles(dirPath);
                if (files != null)
                {
                    List<DiffFolderNode> filesNodes = new List<DiffFolderNode>();
                    foreach (var item in files)
                    {
                        DiffFolderNode fileNode = new DiffFolderNode(this,item, this.FullName);
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
            }
            
        }

        public void UpdateStatus()
        {
            foreach (var item in ChildrenNodes)
            {
                if (item.Status == DiffStatus.Unknown)
                {
                    this.Status = DiffStatus.Unknown;
                }
            }
            if (Parent != null)
            {
                Parent.UpdateStatus();
            }
        }

        private long GetFileLength(string filePath)
        {
            using (var fileStream = File.OpenRead(filePath))
            {
                return fileStream.Length;
            }
        }

        //获取文件的md5值
        public string GetFileMD5(string filePath)
        {
            using (var fileStream = File.OpenRead(filePath))
            {
                System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                byte[] toData = md5.ComputeHash(fileStream);
                string fileMD5 = BitConverter.ToString(toData).Replace("-", "").ToLower();
                return fileMD5;
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
