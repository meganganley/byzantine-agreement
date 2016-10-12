using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;


namespace ByzantineAgreement
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] temp = Console.ReadLine().Split(' ');
            int N = Convert.ToInt32(temp[0]);
            int defaultValue = Convert.ToInt32(temp[1]);

            List<ByzNode> nodes = new List<ByzNode>();

            for (int i = 0; i < N; i++)
            {
                nodes.Add(NodeFromInput(Console.ReadLine()));
            }



            MainSecond();

            Console.ReadLine();

        }

        private static ByzNode NodeFromInput(string s)
        {
            string[] temp = s.Split(' ');

            ByzNode node = new ByzNode();

            int num;
            bool b;

            if (Int32.TryParse(temp[0], out num))
            {
                node.Index = num;
            }
            if (Int32.TryParse(temp[1], out num))
            {
                node.InitValue = num;
            }
            if (Boolean.TryParse(temp[2], out b))
            {
                node.Faulty = b;
            }

            if (node.Faulty)
            {
                for (int i = 3; i < temp.Length; i++)
                {

                }
            }

            return node;
        }

        public static int tid()
        {
            return System.Threading.Thread.CurrentThread.ManagedThreadId;
        }



        public static ActionBlock<Message>[] Byz;

        public static int N;
        public static TaskCompletionSource<bool> Completion;
        static int CompletionCount;


        public static void MainSecond()
        {
            Console.WriteLine("[{0}] main start", tid());

            CompletionCount = N = 4;
            Completion = new TaskCompletionSource<bool>();
            Byz = new ActionBlock<Message>[N];

            for (int i = 0; i < N; i++)
            {
                var bp = new ByzProcess(i + 1, (i + 1) * 10);
                Byz[i] = new ActionBlock<Message>((Action<Message>)bp.ByzBody);
            }

            Console.WriteLine("[{0}] main body", tid());

            for (int i = 0; i < N; i++)
            {
                Byz[i].Post(new Message(0, 0));
            }

            Console.WriteLine("[{0}] main waiting", tid());
            Completion.Task.Wait();
            Console.WriteLine("[{0}] main end", tid());
        }

        public class FaultyScript
        {


        }

        public class ByzNode
        {
            public ByzNode() { }

            public ByzNode(int index, int initValue, bool faulty)
            {
                Index = index;
                InitValue = initValue;
                Faulty = faulty;
            }

            public int Index { get; set; }
            public int InitValue { get; set; }
            public bool Faulty { get; set; }
        }

        public class Message
        {
            public Message(int from, int value) { From = from; Value = value; }
            public int From { get; private set; }
            public int Value { get; private set; }
        }

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

                if (msg.From == 0)
                {
                             for (var i = 0; i < N; i++) Byz[i].Post(new Message(Index, Init));

                }
                else
                {
                    rcount += 1;
                             if (rcount == N && Interlocked.Decrement(ref CompletionCount) == 0)
                                Completion.SetResult(true);
                }
            }
        }
    }

    
   
}
