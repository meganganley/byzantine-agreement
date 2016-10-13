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
        
        public static ActionBlock<Message>[] Byz;
        static int _completionCount;
        
        public static TaskCompletionSource<bool> Completion;


        static void Main(string[] args)
        {
            string[] temp = Console.ReadLine().Split(' ');
            _n = Convert.ToInt32(temp[0]);
            int defaultValue = Convert.ToInt32(temp[1]);

            List<ByzNode> nodes = new List<ByzNode>();

            for (int i = 0; i < _n; i++)
            {
                var n = NodeFromInput(Console.ReadLine());
                nodes.Add(n);

                Console.WriteLine(n);
                
                // todo: rename 
                Byz[i] = new ActionBlock<Message>((Action<Message>)n.ByzBody);
            }


            /* SETTING UP THE CRAZY TDF STUFF */

            for (int i = 0; i < _n; i++)
            {
                Byz[i].Post(new Message(0, 0));
            }

            //  MainSecond();

            Completion.Task.Wait();

            Console.ReadLine();

        }


        public static int tid()
        {
            return System.Threading.Thread.CurrentThread.ManagedThreadId;
        }

        
        // todo : get rid of this 
        public static void MainSecond()
        {
            Console.WriteLine("[{0}] main start", tid());

            _completionCount = _n = 4;
            Completion = new TaskCompletionSource<bool>();
            Byz = new ActionBlock<Message>[_n];

            for (int i = 0; i < _n; i++)
            {
                var bp = new ByzProcess(i + 1, (i + 1) * 10);
                Byz[i] = new ActionBlock<Message>((Action<Message>)bp.ByzBody);
            }

            Console.WriteLine("[{0}] main body", tid());

            for (int i = 0; i < _n; i++)
            {
                Byz[i].Post(new Message(0, 0));
            }

            Console.WriteLine("[{0}] main waiting", tid());
            Completion.Task.Wait();
            Console.WriteLine("[{0}] main end", tid());
        }


        public class ByzNode
        {
            public ByzNode() { }

            public ByzNode(int index, int initValue, bool faulty)
            {
                Index = index;
                InitValue = initValue;
                Faulty = faulty;
                MessagesReceived = 0;
            }

            public int  n = 4;
            public int Index { get; set; }
            public int InitValue { get; set; }
            public bool Faulty { get; set; }
            public int[,] FaultyMessages { get; set; }
            public int MessagesReceived { get; set; }

            public void ByzBody(Message msg)
            {
                // indicates start of new round
                if (msg.From == 0)
                {
                    for (var i = 0; i < _n; i++)
                    {
                        Byz[i].Post(new Message(Index, InitValue));
                    }

                }
                // message receieved with information from other nodes
                else
                {
                    MessagesReceived += 1;

                    // add to some data structure 

                    if (MessagesReceived == _n && Interlocked.Decrement(ref _completionCount) == 0)
                    {
                        Completion.SetResult(true);
                    }
                }
            }

            public override string ToString()
            {
                if (Faulty)
                {
                    string s = Index + " is faulty \n";
                    for (int i = 0; i < FaultyMessages.GetLength(0); i++)
                    {
                        for (int j = 0; j < n; j++)
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
            public Message(int from, int value) { From = from; Value = value; }
            public int From { get; private set; }
            public int Value { get; private set; }
        }

        // todo: get rid of this 
        public class ByzProcess
        {
            public ByzProcess(int index, int init)
            {
                Index = index;
                Init = init;
            }

            public int Index { get; private set; }
            public int Init { get; private set; }

            int rcount;

            public void ByzBody(Message msg)
            {
                Console.WriteLine("[{0}] {1} <- ({2}, {3})", tid(), Index, msg.From, msg.Value);
                // start indicator
                if (msg.From == 0)
                {
                    for (var i = 0; i < _n; i++) Byz[i].Post(new Message(Index, Init));

                }
                else
                {
                    rcount += 1;
                    if (rcount == _n && Interlocked.Decrement(ref _completionCount) == 0)
                        Completion.SetResult(true);
                }
            }
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


    }



}
