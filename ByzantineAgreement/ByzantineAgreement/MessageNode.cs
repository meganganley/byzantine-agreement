using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByzantineAgreement
{
    public class MessageNode
    {
        public IEnumerable<int> Path { get; set; }
        public int Value { get; set; }

        public MessageNode(IEnumerable<int> path, int value)
        {
            Path = path;
            Value = value;
        }

        public override string ToString()
        {
            string s = "";
            foreach (var i in Path)
                s = s + (i + "->");
            return " value " + Value + " (path " + s + ")";
        }
    }
}
