using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoneyBee.Diff.Gui
{
    public class DiffFileLine
    {
        public int Number;
        public string Content;
        public bool IsEmpty;
        public DiffStatus Status;
        public DiffFileLine()
        {
            IsEmpty = true;
        }

        public DiffFileLine(string content)
        {
            Content = content;
        }
    }
}
