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
        public List<DiffFolderNode> ChildrenNodes;
        public bool IsEmpty;
        public long Size=10000;
        public string UpdateTime="2021-10-18 15：36";

        public DiffFolderNode()
        {
            IsEmpty = true;
        }

        public DiffFolderNode(string name,string fullName,bool isFolder = false,bool expansion=false)
        {
            IsEmpty = false;
            Name = Path.GetFileName(name);
            FullName =string.IsNullOrEmpty(fullName)?".": $"{fullName}/{Name}";
            IsFolder = isFolder;
            Expansion = expansion;
            ChildrenNodes = new List<DiffFolderNode>();
        }

    }
}
