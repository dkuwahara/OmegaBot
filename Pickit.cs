using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;

namespace BattleNet
{
    class Pickit
    {

        protected static Dictionary<String, PickTest> m_pickitMap = new Dictionary<string, PickTest>();
        public static Dictionary<String, PickTest> PickitMap { get { return m_pickitMap; } }

        public delegate bool PickTest(Item x);

        public static void InitializePickit()
        {
            FileStream fs = new FileStream("pickit.xml", FileMode.Open);
            XmlSerializer x = new XmlSerializer(typeof(List<Item>));
            List<Item> pickitList = (List<Item>)x.Deserialize(fs);

            foreach (Item i in pickitList)
            {
                Console.WriteLine("{0}: {1}, {2}, Ethereal:{3}, {4}", i.name, i.type, i.quality, i.ethereal, i.sockets == uint.MaxValue ? 0 : i.sockets);
                if (!m_pickitMap.ContainsKey(i.type))
                {
                    m_pickitMap.Add(i.type, CreatePickTest(i));
                }
                else
                {
                    m_pickitMap[i.type] = AddPickTest(m_pickitMap[i.type], CreatePickTest(i));
                }
            }
            fs.Close();
        }

        protected static PickTest AddPickTest(PickTest x, PickTest newTest)
        {
            return delegate(Item item)
            {
                return (x(item) || newTest(item));
            };
        }

        protected static PickTest CombinePickSet(PickTest x, PickTest y)
        {
            return delegate(Item item)
            {
                return (x(item) && y(item));
            };
        }

        protected static PickTest CreatePickTest(Item x)
        {
            PickTest pickTest = delegate(Item item)
            {
                return item.quality == x.quality;
            };

            if (x.ethereal)
            {
                PickTest ethTest = delegate(Item item)
                {
                    return item.ethereal = x.ethereal;
                };

                pickTest = CombinePickSet(pickTest, ethTest);
            }

            if (x.sockets != uint.MaxValue)
            {
                PickTest socketTest = delegate(Item item)
                {
                    return item.sockets == x.sockets;
                };
                pickTest = CombinePickSet(pickTest, socketTest);
            }

            return pickTest;
        }

        public static void TestPickit()
        {
            Item item1 = new Item();
            item1.type = "gld";

            Item item2 = new Item();
            item2.type = "rvl";

            Item item3 = new Item();
            item3.type = "r33";

            Item item4 = new Item();
            item4.type = "oba";
            item4.quality = Item.QualityType.unique;

            Item item5 = new Item();
            item5.type = "oba";
            item5.quality = Item.QualityType.set;

            List<Item> items = new List<Item>();

            items.Add(item1);
            items.Add(item2);
            items.Add(item3);
            items.Add(item4);
            items.Add(item5);
            foreach (Item i in items)
            {
                if (!m_pickitMap.ContainsKey(i.type) && i.type != "rvl" && i.type != "gld")
                    break;

                if (m_pickitMap[i.type](i))
                {
                    Console.WriteLine("Picking up Item!");
                    Console.WriteLine("{0}: {1}, {2}, Ethereal:{3}, {4}", i.name, i.type, i.quality, i.ethereal, i.sockets);
                }
            }

            Console.ReadKey();
        }

    }
}