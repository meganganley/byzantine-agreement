using System;
using System.Collections.Generic;


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
}
