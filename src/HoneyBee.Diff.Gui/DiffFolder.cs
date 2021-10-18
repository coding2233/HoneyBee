using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoneyBee.Diff.Gui
{
    public class DiffFolder
    {

        public DiffFolder()
        {
        }

        public string Path 
        { 
            get 
            {
                int count = PathBuffer.Length;
                for (int i = 0; i < PathBuffer.Length; i++)
                {
                    if (PathBuffer[i] == '\0')
                    {
                        count = i;
                        break;
                    }
                }
                string path = Encoding.UTF8.GetString(PathBuffer, 0, count);
                return path;
            } 
        }
        
        public byte[] PathBuffer { get; private set; } = new byte[1024];

        public DiffFolderNode DiffNode { get; private set; }

        public int SelectIndex { get; set; } = -1;

        public DiffFolderNode GetNode()
        {
            DiffFolderNode node;
            string dirPath = Path;
            if (Directory.Exists(dirPath))
            {
                node = GetDirectoryNodes(dirPath,null,true);
            }
            else
            {
                node = new DiffFolderNode();
            }

            DiffNode = node;

            return node;
        }


        private DiffFolderNode GetDirectoryNodes(string dirPath,string fullName,bool expansion=false)
        {
            DiffFolderNode node = new DiffFolderNode(dirPath, fullName,true, expansion);
            node.UpdateTime=Directory.GetLastWriteTime(dirPath).ToString("yyyy-MM-dd HH:mm"); ;
            var dirs = Directory.GetDirectories(dirPath);
            if (dirs != null)
            {
                List<DiffFolderNode> folderNodes = new List<DiffFolderNode>();
                foreach (var item in dirs)
                {
                    DiffFolderNode dirNode = GetDirectoryNodes(item, node.FullName, true);
                    folderNodes.Add(dirNode);
                    node.Size += dirNode.Size;
                }
                node.ChildrenNodes.AddRange( folderNodes.OrderBy(x => x.Name));
            }

            var files = Directory.GetFiles(dirPath);
            if (files != null)
            {
                List<DiffFolderNode> filesNodes = new List<DiffFolderNode>();
                foreach (var item in files)
                {
                    byte[] buffer = GetFileBuffer(item);
                    DiffFolderNode fileNode = new DiffFolderNode(item, node.FullName);
                    fileNode.Size = buffer.Length;
                    fileNode.SizeString = ToSizeString(fileNode.Size);
                    fileNode.MD5 = GetFileMD5(buffer);
                    fileNode.UpdateTime = File.GetLastWriteTime(item).ToString("yyyy-MM-dd HH:mm");
                    filesNodes.Add(fileNode);
                    node.Size += fileNode.Size;
                }
                node.ChildrenNodes.AddRange(filesNodes.OrderBy(x => x.Name));
            }
            node.SizeString = ToSizeString(node.Size);
            return node;
        }

        public bool GetDiffFlag(DiffFolder other)
        {
            var thisNode = this.GetNode();
            var otherNode = other.GetNode();

            bool thisFlag = !string.IsNullOrEmpty(thisNode.Name) && thisNode.Expansion;
            bool otherFlag = !string.IsNullOrEmpty(otherNode.Name) && otherNode.Expansion;

            if (!thisFlag || !otherFlag)
                return false;

            SetDiffNodes(thisNode, otherNode);

            return true;
        }


        public void SetDiffNodes(DiffFolderNode thisNode, DiffFolderNode otherNode)
        {
            HashSet<string> dirNodeNames = new HashSet<string>();
            HashSet<string> fileNodeNames = new HashSet<string>();
            foreach (var item in thisNode.ChildrenNodes)
            {
                if (!item.IsEmpty)
                {
                    if (item.IsFolder)
                        dirNodeNames.Add(item.Name);
                    else
                        fileNodeNames.Add(item.Name);
                }
            }

         
            foreach (var item in otherNode.ChildrenNodes)
            {
                if (!item.IsEmpty)
                {
                    if (item.IsFolder)
                        dirNodeNames.Add(item.Name);
                    else
                        fileNodeNames.Add(item.Name);
                }
            }
            var dirList = dirNodeNames.ToList();
            dirList.Sort();
            var fileList = fileNodeNames.ToList();
            fileList.Sort();

            List<string> nodeNames = new List<string>();
            nodeNames.AddRange(dirList);
            nodeNames.AddRange(fileList);

            for (int i = 0; i < nodeNames.Count; i++)
            {
                var item = nodeNames[i];
                int folderIndex = 0;
                if (i < thisNode.ChildrenNodes.Count)
                {
                    var child = thisNode.ChildrenNodes[i];
                    if (!item.Equals(child.Name))
                    {
                        thisNode.ChildrenNodes.Insert(i, new DiffFolderNode());
                    }
                    else
                    {
                        if (child.IsFolder)
                        {
                            folderIndex++;
                        }
                    }
                }
                else
                {
                    thisNode.ChildrenNodes.Add(new DiffFolderNode());
                }

                if (i < otherNode.ChildrenNodes.Count)
                {
                    var child = otherNode.ChildrenNodes[i];
                    if (!item.Equals(child.Name))
                    {
                        otherNode.ChildrenNodes.Insert(i, new DiffFolderNode());
                    }
                    else
                    {
                        folderIndex++;
                    }
                }
                else
                {
                    otherNode.ChildrenNodes.Add(new DiffFolderNode());
                }

                if (folderIndex == 2)
                {
                    SetDiffNodes(thisNode.ChildrenNodes[i],otherNode.ChildrenNodes[i]);
                }
            }

        }


        private byte[] GetFileBuffer(string filePath)
        {
            return File.ReadAllBytes(filePath);
        }

        //获取文件的md5值
        public string GetFileMD5(byte[] data)
        {
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] toData = md5.ComputeHash(data);
            string fileMD5 = BitConverter.ToString(toData).Replace("-", "").ToLower();
            return fileMD5;
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
            else
            {
                return $"{(size / 1024.0f / 1024.0f).ToString("f2")} MB";
            }
        }
    }




}
