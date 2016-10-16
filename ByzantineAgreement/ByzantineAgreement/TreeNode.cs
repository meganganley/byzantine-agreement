using System;
using System.Collections;
using System.Collections.Generic;

namespace ByzantineAgreement
{
    public class TreeNode : IComparable
    {
        public List<TreeNode> Children { get; set; }
        public int Value { get; set; }
        public int Index { get; set; }
        public int Evaluation { get; set; }

        public TreeNode(int value, int index)
        {
            Value = value;
            Index = index;

            Children = new List<TreeNode>();
        }


        public override string ToString()
        {
            return $"Index {Index} with value {Value}";
        }

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            TreeNode other = obj as TreeNode;
            if (other != null)
                // todo fix
                return Index.CompareTo(other.Index);
            throw new ArgumentException("Object is not a TreeNode");
        }
    }
}