using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ByzantineAgreement
{
    public class TreeTraverser
    {
        private List<MessageNode> _list;
        private string _output;
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
            _output = "";


            BottomUpEvaluation(root);
            Consensus(root);

            levels = new List<string>();


            BFS(root);


            return levels;
        }




        public void BFS(TreeNode root)
        {
           
            Queue<TreeNode> q = new Queue<TreeNode>();
            q.Enqueue(root);//You don't need to write the root here, it will be written in the loop
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
                _output += " " + node.Index + " " + node.Value + " ";
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

    }
}

