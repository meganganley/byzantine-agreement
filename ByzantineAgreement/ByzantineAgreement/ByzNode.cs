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

                Console.WriteLine("Node " + Index + " is starting round " + RoundNumber);


                /* not keeping */

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


                for (var i = 0; i < Program.N; i++)
                {



                    if (Faulty)
                    {
                        // TODO Deal with faulty stuff. Need to generate list of messageNodes from faulty message input.


                        Program.Byz[i].Post(FaultyMessageParser(FaultyMessages[RoundNumber - 1][i], Index, RoundNumber));
                    }
                    else
                    {
                        List<MessageNode> messageNodes = new TreeTraverser().GenerateMessages(Root);
                        messageNodes.RemoveAll(x => x.Path.Any() && x.Path.First() == Index)
                    ;
                        Message message = new Message(Index, messageNodes);
                        Program.Byz[i].Post(message);
                    }

                }

            }
            //            else if (msg.Sender == Index)
            //            {
            //                MessagesReceived += 1;
            //            }
            // message receieved with information from other nodes
            else
            {

                Console.WriteLine(msg + " to " + Index);
                MessagesReceived += 1;




                foreach (MessageNode n in msg.Data)
                {
                    TreeNode p = Root;
                    TreeNode c;
                    int count = 0;


                    if (!n.Path.Any())
                    {
                        p.Children.Add(new TreeNode(n.Value, msg.Sender));
                        break;
                    }

                    int node = n.Path.ElementAt(count);
                    while (p.Children.Count > 0 && count < n.Path.Count() - 1)
                    {
                        count++;
                        c = p.Children.Find(x => x.Index.Equals(node));
                        p = c;
                        node = n.Path.ElementAt(count);
                    }

                    p.Children.Add(new TreeNode(n.Value, msg.Sender));

                }

                //                while (p.Children.Count > 0)
                //                {
                //                    // find the start of path to go down 
                //                    c = p.Children.Find(x => x.Data.NodePath.Equals(index));
                //                }
                //
                //                string index;

                /*
                                TreeNode p = Root.Children.Find(x => x.Data.NodePath.Equals(msg.NodePath.Substring(0, 1)));

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
                                }*/
            }

            // add children at level round number 
            //    Root.Children.Add(new Message());

            // TODO 
            // add to some data structure 

            if (MessagesReceived == Program.N && Interlocked.Decrement(ref Program.CompletionCount) == 0)
            {
                Program.Completion.SetResult(true);
            }

        }

        public static int tid()
        {
            return Thread.CurrentThread.ManagedThreadId;
        }

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
            //            foreach (char c in s)
            //            {
            //                
            //            }

            return new Message(index, m);
        }


        // debug
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