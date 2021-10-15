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
                    }
                }
                string path = Encoding.UTF8.GetString(PathBuffer, 0, count);
                return path;
            } 
        }
        
        public byte[] PathBuffer { get; private set; } = new byte[1024];


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
            return node;
        }


        private DiffFolderNode GetDirectoryNodes(string dirPath,string fullName,bool expansion=false)
        {
            DiffFolderNode node = new DiffFolderNode(dirPath, fullName,true, expansion);
            var dirs = Directory.GetDirectories(dirPath);
            if (dirs != null)
            {
                List<DiffFolderNode> folderNodes = new List<DiffFolderNode>();
                foreach (var item in dirs)
                {
                    DiffFolderNode dirNode = GetDirectoryNodes(item, fullName, true);
                    folderNodes.Add(dirNode);
                }
                node.ChildrenNodes.AddRange( folderNodes.OrderBy(x => x.Name));
            }

            var files = Directory.GetFiles(dirPath);
            if (files != null)
            {
                List<DiffFolderNode> filesNodes = new List<DiffFolderNode>();
                foreach (var item in files)
                {
                    DiffFolderNode fileNode = new DiffFolderNode(item,fullName);
                    filesNodes.Add(fileNode);
                }
                node.ChildrenNodes.AddRange(filesNodes.OrderBy(x => x.Name));
            }
            return node;
        }

        public bool GetDiffFlag(DiffFolder other)
        {
            var thisNode = GetNode();
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
            HashSet<string> allNodeNames = new HashSet<string>();
            foreach (var item in thisNode.ChildrenNodes)
            {
                allNodeNames.Add(item.Name);
            }

            foreach (var item in otherNode.ChildrenNodes)
            {
                allNodeNames.Add(item.Name);
            }

            List<string> nodeNames = new List<string>();
            nodeNames.AddRange(allNodeNames);
            nodeNames.Sort();

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

                if (folderIndex == 2)
                {
                    SetDiffNodes(thisNode.ChildrenNodes[i],otherNode.ChildrenNodes[i]);
                }
            }

        }

    }




}
