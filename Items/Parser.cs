using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BattleNet.Items
{
    class Parser
    {

        private delegate void ParseSection(BitReader reader, ref Item item);
        
        private static void ExampleFunction(BitReader reader, ref Item item)
        {
            item.action = (uint)reader.Read(8);
        }

        public static Item Parse(List<byte> packet)
        {
            Item item = new Item();
            BitReader reader = new BitReader(packet.ToArray());

            ExampleFunction(reader, ref item);

            return item;
        }

    }
}
