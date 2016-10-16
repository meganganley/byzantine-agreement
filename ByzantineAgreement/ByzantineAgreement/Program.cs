using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ByzantineAgreement
{
    internal static class Program
    {
        public static int N;
        public static int DefaultValue;
        public static int MaxRounds;

        public static ActionBlock<Message>[] Byz;
        public static int CompletionCount;
        public static TaskCompletionSource<bool> Completion;

        private static void Main(string[] args)
        {
            List<ByzNode> nodes;

            using (StreamReader reader = new StreamReader(args[0]))
            {
                string[] temp = reader.ReadLine()?.Split(' ');
                N = Convert.ToInt32(temp?[0]);
                DefaultValue = Convert.ToInt32(temp?[1]);
                Byz = new ActionBlock<Message>[N];

                nodes = new List<ByzNode>();
                
                for (var i = 0; i < N; i++)
                {
                    var node = NodeFromInput(reader.ReadLine());
                    //Console.WriteLine(node);
                    nodes.Add(node);
                    // todo: i / node.index??
                    Byz[node.Index - 1] = new ActionBlock<Message>((Action<Message>) node.ByzBody);
                }

            }
            /* SETTING UP THE CRAZY TDF STUFF */

            MaxRounds = (N - 1) / 3 + 1;

            for (var i = 1; i <= MaxRounds; i++)
            {
                CompletionCount = N;
                Completion = new TaskCompletionSource<bool>();

                for (var j = 0; j < N; j++)
                    Byz[j].Post(new Message(0, Enumerable.Empty<MessageNode>()));

                Completion.Task.Wait();
            }

           // Console.WriteLine("Finished rounds");

            for (int i = 0 ; i < N; i++)
            {
                Console.WriteLine(nodes[i].GetResult());
            }

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