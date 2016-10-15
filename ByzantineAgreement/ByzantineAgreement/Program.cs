using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;


namespace ByzantineAgreement
{
    class Program
    {
        private static int _n;
        private static int _defaultValue;

        public static ActionBlock<Message>[] Byz;
        static int _completionCount;

        public static TaskCompletionSource<bool> Completion;


        static void Main(string[] args)
        {
            string[] temp = Console.ReadLine().Split(' ');
            _n = Convert.ToInt32(temp[0]);
            _defaultValue = Convert.ToInt32(temp[1]);

            List<ByzNode> nodes = new List<ByzNode>();
            Byz = new ActionBlock<Message>[_n];


       //     Completion = new TaskCompletionSource<bool>();
      //      _completionCount = _n;

            for (int i = 0; i < _n; i++)
            {
                var node = NodeFromInput(Console.ReadLine());
                nodes.Add(node);

                Console.WriteLine(node);

                // todo: i / node.index??
                Byz[node.Index - 1] = new ActionBlock<Message>((Action<Message>)node.ByzBody);
            }


            /* SETTING UP THE CRAZY TDF STUFF */

            int maxRounds = (_n - 1)/3 + 1;

            for (int i = 0 ; i < maxRounds; i++)
            {
                _completionCount = _n;
                Completion = new TaskCompletionSource<bool>();

                for (int j = 0; j < _n; j++)
                {
                    Byz[j].Post(new Message("0", "0"));
                }

                Completion.Task.Wait();
            }
            
            Console.WriteLine("Finished rounds");
            Console.ReadLine();

        }


        public static int tid()
        {
            return System.Threading.Thread.CurrentThread.ManagedThreadId;
        }


        public class ByzNode
        {
            public ByzNode()
            {
                MessagesReceived = 0;
                RoundNumber = 0;
            }
            /* 
            public ByzNode(int index, int initValue, bool faulty)
            {
                Index = index;
                InitValue = initValue;
                Faulty = faulty;
                MessagesReceived = 0;
                RoundNumber = 1;

                Root = new TreeNode(new Message(Index, InitValue));
                
            }*/
            public int Index { get; set; }
            public string InitValue { get; set; }
            public bool Faulty { get; set; }
            public string[,] FaultyMessages { get; set; }
            public TreeNode Root { get; set; }

            public int MessagesReceived;
            public int RoundNumber;

            public void ByzBody(Message msg)
            {

                Console.WriteLine("[{0}] {1} <- ({2}, {3})", tid(), Index, msg.NodePath, msg.Value);

                // indicates start of new round
                if (msg.NodePath.Equals("0"))
                {

                    RoundNumber++;
                    MessagesReceived = 0;

                    Console.WriteLine("Node " + Index + " is starting round " + RoundNumber);


                    TreeNode p = Root;
                    List<TreeNode> current = new List<TreeNode> { Root };

                    // should find last completed level 
                    while (p.Children.Count > 0)
                    {
                        current = p.Children;
                        p = p.Children.ElementAt(0);
                    }

                    if (current.Count > 1)
                    {
                        current.Sort();
                    }
                    


                    for (var i = 0; i < _n; i++)
                    {
                        if (Faulty)
                        {
                            Byz[i].Post(new Message(""+Index, FaultyMessages[RoundNumber - 1, i]));
                        }
                        else
                        {
                            string value = "";
                            foreach (TreeNode t in current)
                            {
                                value += t.Data.Value;
                                
                            }
                            Byz[i].Post(new Message(""+Index, value));
                        }

                    }

                }
                // message receieved with information from other nodes
                else
                {
                    MessagesReceived += 1;

                    string index;


                    TreeNode p = Root.Children.Find(x => x.Data.NodePath.Equals(msg.NodePath.Substring(0,1)));
                    
                    //special case for first round
                    if (p == null)
                    {
                        Root.Children.Add(new TreeNode(new Message(msg.NodePath + "", msg.Value)));
                    }
                    else
                    {
                        TreeNode c;


                        for (int i = 1; i < msg.NodePath.Length; i++)
                        {
                            index = msg.NodePath.Substring(0, i + 1);


                            c = p.Children.Find(x => x.Data.NodePath.Equals(index));

                            // node doesnt exist yet, create
                            if (c == null)
                            {
                                p.Children.Add(new TreeNode(new Message(msg.NodePath + "", msg.Value)));
                                break;
                            }

                            c = p;
                        }
                    }


                   

             // add children at level round number 
                //    Root.Children.Add(new Message());

                    // TODO 
                    // add to some data structure 

                    if (MessagesReceived == _n && Interlocked.Decrement(ref _completionCount) == 0)
                    {
                        Completion.SetResult(true);
                    }
                }
            }

            // debug
            public override string ToString()
            {
                if (Faulty)
                {
                    string s = Index + " is faulty \n";
                    for (int i = 0; i < FaultyMessages.GetLength(0); i++)
                    {
                        for (int j = 0; j < _n; j++)
                        {
                            s += FaultyMessages[i, j] + " ";
                        }
                        s += "\n";
                    }
                    return s;
                }
                return Index + " is not faulty";
            }
        }

        public class Message
        {
            public Message(string nodePath, string value) { NodePath = nodePath; Value = value; }

            // Node is the full path of information 
            public string NodePath { get; private set; }
            public string Value { get; private set; }

            public override string ToString()
            {
                return NodePath + " : " + Value;
            }
        }

        private static int GetNDigits(int num, int digit)
        {
            return Convert.ToInt32(num.ToString().Substring(0, digit + 1));
        }

        private static int GetNumberDigits(int num)
        {
            return num.ToString().Length;
        }
        
        /* HELPER METHODS */
        private static ByzNode NodeFromInput(string s)
        {
            string[] temp = s.Split(' ');

            ByzNode node = new ByzNode();

            int num;

            if (Int32.TryParse(temp[0], out num))
            {
                node.Index = num;
            }

            node.InitValue = temp[1];

            if (temp[2] == "1")
            {
                node.Faulty = true;
                node.FaultyMessages = FaultyScriptFromInput(temp.Skip(3));
            }
            else
            {
                node.Faulty = false;
            }


            node.Root = new TreeNode(new Message("" + node.Index, node.InitValue));

            return node;
        }

        private static string[,] FaultyScriptFromInput(IEnumerable<string> s)
        {
            string[,] ret = new string[s.Count() / _n, _n];
           
            for (int i = 0; i < ret.GetLength(0); i++)
            {
                for (int j = 0; j < _n; j++)
                {
                    ret[i, j] = s.ElementAt(i * _n + j);
                }
            }
            return ret;
        }

        public static int CreateMessage()
        {
            return 0; 
        }

        public class TreeNode : IComparable
        {
            //public List<TreeNode> _childrenAtLevel;
            public TreeNode Parent { get; set; }
            public List<TreeNode> Children { get; set; }

//            public List<TreeNode> ChildrenAtLevel
//            {
//                get { return _childrenAtLevel; }
//                set { _childrenAtLevel = value; }
//            }


            public Message Data { get; set; }
            public int Consensus { get; set; }

            // maybe level? count of children?

            public TreeNode(Message data)
            {
                Data = data;
                Children = new List<TreeNode>();
            }


            public override string ToString()
            {
                return Data.ToString();
            }

            public int CompareTo(object obj)
            {
                if (obj == null) return 1;

                TreeNode other = obj as TreeNode;
                if (other != null)
                    // todo fix
                    return Data.NodePath.CompareTo(other.Data.NodePath);
                else
                    throw new ArgumentException("Object is not a Temperature");
            }
        }

    }



}
