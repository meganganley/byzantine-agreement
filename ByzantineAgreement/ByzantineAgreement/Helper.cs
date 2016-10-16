#region

using System.Collections.Generic;
using System.Linq;

#endregion

namespace ByzantineAgreement
{
    public class Helper
    {
        private List<MessageNode> _list;
        private List<string> levels;

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

        public List<string> GenerateOutput(TreeNode root)
        {
            BottomUpEvaluation(root);
            Consensus(root);

            levels = new List<string>();


            BFS(root);


            return levels;
        }

        public void BFS(TreeNode root)
        {
            Queue<TreeNode> q = new Queue<TreeNode>();
            q.Enqueue(root); 
            while (q.Count > 0)
            {
                string s = "";
                int levelNodes = q.Count();
                while (levelNodes > 0)
                {
                    TreeNode n = q.Dequeue();
                    s += n.Evaluation;
                    foreach (TreeNode child in n.Children)
                    {
                        q.Enqueue(child);
                    }

                    levelNodes--;
                }
                levels.Add(s);
            }
        }

        private void BottomUpEvaluation(TreeNode node)
        {
            if (!node.Children.Any())
            {
                node.Evaluation = node.Value;
                return;
            }

            foreach (var child in node.Children)
            {
                BottomUpEvaluation(child);
            }
        }


        private int Consensus(TreeNode node)
        {
            if (!node.Children.Any())
            {
                return node.Value;
            }

            int sum = 0;
            foreach (var child in node.Children)
            {
                sum += Consensus(child);
            }

            if ((double)sum / node.Children.Count > 0.5)
            {
                node.Evaluation = 1;
                return 1;
            }
            if ((double)sum / node.Children.Count >= 0.5)
            {
                node.Evaluation = Program.DefaultValue;
                return Program.DefaultValue;
            }
            node.Evaluation = 0;
            return 0;
        }

        // todo 
        // create one message to be sent to one node
        public Message FaultyMessageParser(string value, int index, int roundNumber)
        {
            // each character in string has to manufacture own path
            List<MessageNode> m = new List<MessageNode>();

            int count = 1;

            if (roundNumber == 1)
            {
                List<int> path = new List<int>();
                m.Add(new MessageNode(path, int.Parse(value)));
            }
            else if (roundNumber == 2)
            {
                foreach (char c in value)
                {
                    List<int> path = new List<int>();

                    if (count == index)
                    {
                        count += 1;
                    }

                    //     path.Add(sender);
                    path.Add(count);
                    m.Add(new MessageNode(path, c - '0'));

                    count++;
                }
            }
            else if (roundNumber == 3)
            {
                foreach (char c in value)
                {
                    List<int> path = new List<int>();

                    if (count == index)
                    {
                        count += 1;
                    }

                    //     path.Add(sender);
                    path.Add(count);
                    m.Add(new MessageNode(path, c - '0'));

                    count++;
                }
            }

            return new Message(index, m);
        }
    }
}