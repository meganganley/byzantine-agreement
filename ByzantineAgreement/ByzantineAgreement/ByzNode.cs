using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ByzantineAgreement
{
    public class ByzNode
    {
        public ByzNode()
        {
            MessagesReceived = 0;
            RoundNumber = 0;
        }

        public int Index { get; set; }
        public int InitValue { get; set; }
        public bool Faulty { get; set; }
        public string[][] FaultyMessages { get; set; }
        public TreeNode Root { get; set; }

        public int MessagesReceived;
        public int RoundNumber;

        public void ByzBody(Message msg)
        {

            //   Console.WriteLine("[{0}] {1} <- ({2}, {3})", tid(), Index, msg.NodePath, msg.Value);

            // indicates start of new round
            if (msg.Sender == 0)
            {
                RoundNumber++;
                MessagesReceived = 0;

            //    Console.WriteLine("Node " + Index + " is starting round " + RoundNumber);

                string output = RoundNumber + " " + Index + " > ";

                for (var i = 0; i < Program.N; i++)
                {
                    if (Faulty)
                    {
                        Message message = FaultyMessageParser(FaultyMessages[RoundNumber - 1][i], Index, RoundNumber);
                        
                        foreach (MessageNode m in message.Data)
                        {
                            output += m.Value;
                        }
                        output += " ";


                        Program.Byz[i].Post(message);
                    }
                    else
                    {

                        List<MessageNode> messageNodes = new TreeTraverser().GenerateMessages(Root);
                        messageNodes.RemoveAll(x => x.Path.Any() && x.Path.First() == Index);
                        Message message = new Message(Index, messageNodes);

                        foreach (MessageNode m in messageNodes)
                        {
                            output += m.Value;
                        }
                        output += " ";
                        Program.Byz[i].Post(message);
                    }

                }
                Console.WriteLine(output);
            }
            else
            {

                // Console.WriteLine(msg + " to " + Index);
                MessagesReceived += 1;

                
                foreach (MessageNode n in msg.Data)
                {
                    TreeNode p = Root;
                    TreeNode c;
                    int count = 0;

                    if (n.Value == 2)
                    {
                        n.Value = Program.DefaultValue;
                    }

                    if (!n.Path.Any())
                    {
                        p.Children.Add(new TreeNode(n.Value, msg.Sender));
                        p.Children.Sort();
                        break;
                    }

                    int node = n.Path.ElementAt(count);
                    while (p.Children.Count > 0 && count < n.Path.Count())
                    {
                        c = p.Children.Find(x => x.Index.Equals(node));
                        p = c;
                        node = n.Path.ElementAt(count);
                        count++;
                    }


                    p.Children.Add(new TreeNode(n.Value, msg.Sender));
                    p.Children.Sort();
                }
                
            }

            if (MessagesReceived == Program.N && Interlocked.Decrement(ref Program.CompletionCount) == 0)
            {
                Program.Completion.SetResult(true);

            }

        }

        public string GetResult()
        {
            string s = ((RoundNumber + 1) + " " + Index + " : ");
            

            List<string> output = new TreeTraverser().GenerateOutput(Root);
            for (int index = output.Count - 1; index >= 0; index--)
            {
                s += output[index] + " ";
            }
            return s;
        }

        public static int tid()
        {
            return Thread.CurrentThread.ManagedThreadId;
        }

        // todo : move to tree traverser

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

            return new Message(index, m);
        }


        public override string ToString()
        {
            if (Faulty)
            {
                string s = Index + " is faulty \n";
                for (int i = 0; i < FaultyMessages.Length; i++)
                {
                    for (int j = 0; j < Program.N; j++)
                    {
                        s += FaultyMessages[i][j] + " ";
                    }
                    s += "\n";
                }
                return s;
            }
            return Index + " is not faulty";
        }
    }
}