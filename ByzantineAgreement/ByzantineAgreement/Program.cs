using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ByzantineAgreement
{
    internal static class Program
    {
        public static int N;
        private static int _defaultValue;
        public static ActionBlock<Message>[] Byz;
        public static int CompletionCount;
        public static TaskCompletionSource<bool> Completion;

        private static void Main(string[] args)
        {
            string[] temp = Console.ReadLine()?.Split(' ');
            N = Convert.ToInt32(temp?[0]);
            _defaultValue = Convert.ToInt32(temp?[1]);
            Byz = new ActionBlock<Message>[N];

            for (var i = 0; i < N; i++)
            {
                var node = NodeFromInput(Console.ReadLine());
                //Console.WriteLine(node);

                // todo: i / node.index??
                Byz[node.Index - 1] = new ActionBlock<Message>((Action<Message>)node.ByzBody);
            }


            /* SETTING UP THE CRAZY TDF STUFF */

            var maxRounds = (N - 1) / 3 + 1;

            for (var i = 0; i < maxRounds; i++)
            {
                CompletionCount = N;
                Completion = new TaskCompletionSource<bool>();

                for (var j = 0; j < N; j++)
                    Byz[j].Post(new Message(0, Enumerable.Empty<MessageNode>()));

                Completion.Task.Wait();
            }

            Console.WriteLine("Finished rounds");
            Console.ReadLine();
        }


        /* HELPER METHODS */

        private static ByzNode NodeFromInput(string s)
        {
            string[] temp = s.Split(' ');

            ByzNode node = new ByzNode
            {
                Index = int.Parse(temp[0]),
                InitValue = int.Parse(temp[1])
            };


            if (temp[2] == "1")
            {
                node.Faulty = true;
                node.FaultyMessages = FaultyScriptFromInput(temp.Skip(3));
            }
            else
            {
                node.Faulty = false;
            }

            node.Root = new TreeNode(node.InitValue, node.Index);

            return node;
        }

        private static string[][] FaultyScriptFromInput(IEnumerable<string> s)
        {
            IList<string> listS = s as IList<string> ?? s.ToList();
            string[][] ret = new string[listS.Count/N][];

            for (var i = 0; i < ret.GetLength(0); i++)
            {
                ret[i] = new string[N];
                for (var j = 0; j < N; j++)
                {
                    ret[i][j] = listS.ElementAt(i * N + j);
                }
            }

            return ret;
        }
    }
}