using System.Collections.Generic;
using System.Linq;

namespace ByzantineAgreement
{
    public class TreeTraverser
    {
        private List<MessageNode> _list;

        public List<MessageNode> GenerateMessages(TreeNode root)
        {
            _list = new List<MessageNode>();

            TraverseTree(root, new List<int>());

            return _list;
        }

        private void TraverseTree(TreeNode node, IEnumerable<int> path)
        {
            if (!node.Children.Any())
            {
                _list.Add(new MessageNode(path, node.Value));
                return;
            }

            foreach (var child in node.Children)
            {
                TraverseTree(child, path.Concat(new List<int> { child.Index }));
            }
        }
    }
}

