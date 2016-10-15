using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ByzantineAgreement
{
    public class Message
    {
        public int Sender { get; set; }
        public IEnumerable<MessageNode> Data { get; private set; }

        public Message(int sender, IEnumerable<MessageNode> data)
        {
            Sender = sender;
            Data = data;
        }

        public override string ToString()
        {
            string result = "";
            foreach (var node in Data)
                result += (node + ", ");

            return Sender + " sends " + result;
        }
    }
}