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


            Completion = new TaskCompletionSource<bool>();
            _completionCount = _n;

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
                for (int j = 0; j < _n; j++)
                {
                    Byz[j].Post(new Message(0, 0));
                }

                Completion.Task.Wait();
            }
            
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
                RoundNumber = 1;

                Root = new TreeNode(new Message(0, _defaultValue));
              
            }

            public ByzNode(int index, int initValue, bool faulty)
            {
                Index = index;
                InitValue = initValue;
                Faulty = faulty;
                MessagesReceived = 0;
                RoundNumber = 1;

                Root = new TreeNode(new Message(0, _defaultValue));
                
            }
            public int Index { get; set; }
            public int InitValue { get; set; }
            public bool Faulty { get; set; }
            public int[,] FaultyMessages { get; set; }
            public TreeNode Root { get; set; }

            public int MessagesReceived;
            public int RoundNumber;

            public void ByzBody(Message msg)
            {

                Console.WriteLine("[{0}] {1} <- ({2}, {3})", tid(), Index, msg.Node, msg.Value);

                // indicates start of new round
                if (msg.Node == 0)
                {
                    RoundNumber++;
                    MessagesReceived = 0;


                    for (var i = 0; i < _n; i++)
                    {
                        TreeNode p = Root;
                        TreeNode c = Root;

                        // should find last completed level 
                        while (p.Children.Count > 0)
                        {
                            c = p;
                            p = p.Children.ElementAt(0);
                        }


                        // c.Children is messages to send 
                        // send messages for corresponding round SOMEHOW 

                        if (Faulty)
                        {
                            Byz[i].Post(new Message(Index, FaultyMessages[0, i]));
                        }
                        else
                        {
                            foreach (TreeNode t in c.Children)
                            {
                                // value append 
                                // index append 
                            }
                            Byz[i].Post(new Message(Index, InitValue));
                        }

                    }

                }
                // message receieved with information from other nodes
                else
                {
                    MessagesReceived += 1;

                    int index;
                    TreeNode p = Root;
                    TreeNode c;


                    for ( int i = 0 ; i < GetNumberDigits(msg.Node); i ++)
                    {
                        index = GetDigit(msg.Node, i);



                        c = p.Children.Find( x => x.Data.Node == index);

                        // node doesnt exist yet, create
                        if ( c == null)
                        {
                            p.Children.Add(new TreeNode(new Message(msg.Node, msg.Value)));
                            break;
                        }

                        c = p;
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
            public Message(int node, int value) { Node = node; Value = value; }

            // Node is the full path of information 
            public int Node { get; private set; }
            public int Value { get; private set; }

            public override string ToString()
            {
                return Node + " : " + Value;
            }
        }

        private static int GetDigit(int num, int digit)
        {
            return Convert.ToInt32(num.ToString().Substring(digit, digit + 1));
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
            if (Int32.TryParse(temp[1], out num))
            {
                node.InitValue = num;
            }
            if (temp[2] == "1")
            {
                node.Faulty = true;
                node.FaultyMessages = FaultyScriptFromInput(temp.Skip(3));
            }


            return node;
        }

        private static int[,] FaultyScriptFromInput(IEnumerable<string> s)
        {
            int[,] ret = new int[s.Count() / _n, _n];
            int num;

            for (int i = 0; i < ret.GetLength(0); i++)
            {
                for (int j = 0; j < _n; j++)
                {
                    if (Int32.TryParse(s.ElementAt(i * _n + j), out num))
                    {
                        ret[i, j] = num;
                    }
                }
            }
            return ret;
        }

        public static int CreateMessage()
        {
            return 0; 
        }

        public class TreeNode
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
        }

    }



}
