using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BattleNet.Items
{
    class Parser
    {

        private delegate void ParseSection(BitReader reader, ref Item item);

        private static void GenericInfo(BitReader reader, ref Item item) // get basic info such as item
        {
            byte packet = (byte)reader.Read(8);
            item.action = (uint)reader.Read(8);
            item.category = (uint)reader.Read(8);
            byte validSize = (byte)reader.Read(8);
            item.id = (uint)reader.Read(32);
            if (packet == 0x9d)
            {
                reader.Read(40);
            }
        }

        private static void StatusInfo(BitReader reader, ref Item item) // get info for basic status info
        {
            item.equipped = reader.ReadBit();
            reader.ReadBit();
            reader.ReadBit();
            item.in_socket = reader.ReadBit();
            item.identified = reader.ReadBit();
            reader.ReadBit();
            item.switched_in = reader.ReadBit();
            item.switched_out = reader.ReadBit();
            item.broken = reader.ReadBit();
            reader.ReadBit();
            item.potion = reader.ReadBit();
            item.has_sockets = reader.ReadBit();
            reader.ReadBit();
            item.in_store = reader.ReadBit();
            item.not_in_a_socket = reader.ReadBit();
            reader.ReadBit();
            item.ear = reader.ReadBit();
            item.start_item = reader.ReadBit();
            reader.ReadBit();
            reader.ReadBit();
            reader.ReadBit();
            item.simple_item = reader.ReadBit();
            item.ethereal = reader.ReadBit();
            reader.ReadBit();
            item.personalised = reader.ReadBit();
            item.gambling = reader.ReadBit();
            item.rune_word = reader.ReadBit();
            reader.Read(5);
            item.version = (Item.VersionType)(reader.Read(8));
            reader.Read(2);
        }

        private static void getLocation(BitReader reader, ref Item item)
        {
            byte destination = (byte)reader.Read(3);
            item.ground = (destination == 0x03);

            if (item.ground)
            {
                item.x = (UInt16)reader.Read(16);
                item.y = (UInt16)reader.Read(16);
            }
            else
            {
                item.directory = (byte)reader.Read(4);
                item.x = (byte)reader.Read(4);
                item.y = (byte)reader.Read(3);
                item.container = (Item.ContainerType)(reader.Read(4));
            }
            item.unspecified_directory = false;

            if (item.action == (uint)Item.Action.add_to_shop || item.action == (uint)Item.Action.remove_from_shop)
            {
                long container = (long)(item.container);
                container |= 0x80;
                if ((container & 1) != 0)
                {
                    container--; //remove first bit
                    item.y += 8;
                }
                item.container = (Item.ContainerType)container;
            }
            else if (item.container == Item.ContainerType.unspecified)
            {
                if (item.directory == (uint)Item.DirectoryType.not_applicable)
                {
                    if (item.in_socket)
                        //y is ignored for this container type, x tells you the index
                        item.container = Item.ContainerType.item;
                    else if (item.action == (uint)Item.Action.put_in_belt || item.action == (uint)Item.Action.remove_from_belt)
                    {
                        item.container = Item.ContainerType.belt;
                        item.y = item.x / 4;
                        item.x %= 4;
                    }
                }
                else
                    item.unspecified_directory = true;
            }
        }

        public static bool EarInfo(BitReader reader, ref Item item)
        {
            if (item.ear)
            {
                reader.Read(3);
                item.ear_level = (byte)reader.Read(7);
                //item.ear_name = "Fix Me"; //fix me later
                List<Byte> ear_name = new List<byte>();
                reader.Read(8);
                while (ear_name.Last() != 0x00)
                {
                    reader.Read(8); // 16 characters of 7 bits each for the name of the ear to process later
                }
                
                item.ear_name = Convert.ToBase64String(ear_name.ToArray());
                return true;
            }
            else
                return false;
        }

        public static bool GetItemType(BitReader reader, ref Item item) // gets the 3 letter item code
        {
            byte[] code_bytes = new byte[4];
            for (int i = 0; i < code_bytes.Length; i++)
                code_bytes[i] = (byte)(reader.Read(8));
            code_bytes[3] = 0;

            item.type = System.Text.Encoding.ASCII.GetString(code_bytes).Substring(0, 3);

            ItemEntry entry;
            if (!DataManager.Instance.m_itemData.Get(item.type, out entry))
            {
                Console.WriteLine("Failed to look up item in item data table");
                return true;
            }

            item.name = entry.Name;
            item.width = entry.Width;
            item.height = entry.Height;

            item.is_armor = entry.IsArmor();
            item.is_weapon = entry.IsWeapon();

            if (item.type == "gld")
            {
                item.is_gold = true;
                bool big_pile = reader.ReadBit();
                if (big_pile) item.amount = (uint)reader.Read(32);
                else item.amount = (uint)reader.Read(12);
                return true;
            }
            else return false;
        }

        public static void GetSocketInfo(BitReader reader, ref Item item)
        {
            item.used_sockets = (byte)reader.Read(3);
        }

        public static bool GetLevelQuality(BitReader reader, ref Item item)
        {
            item.quality = Item.QualityType.normal;
            if (item.simple_item || item.gambling)
                return false;
            item.level = (byte)reader.Read(7);
            item.quality = (Item.QualityType)(reader.Read(4));
            return true;
        }

        public static void GetGraphicInfo(BitReader reader, ref Item item)
        {
            item.has_graphic = reader.ReadBit(); ;
            if (item.has_graphic)
                item.graphic = (byte)reader.Read(3);

            item.has_colour = reader.ReadBit();
            if (item.has_colour)
                item.colour = (UInt16)reader.Read(11);
        }

        public static void GetIdentifiedInfo(BitReader reader, ref Item item)
        {
            if (item.identified)
            {
                switch (item.quality)
                {
                    case Item.QualityType.inferior:
                        item.prefix = (byte)reader.Read(3);
                        break;
                    case Item.QualityType.superior:
                        item.superiority = (Item.SuperiorItemClassType)(reader.Read(3));
                        break;
                    case Item.QualityType.magical:
                        item.prefix = (uint)reader.Read(11);
                        item.suffix = (uint)reader.Read(11);
                        break;

                    case Item.QualityType.crafted:
                    case Item.QualityType.rare:
                        item.prefix = (uint)reader.Read(8) - 156;
                        item.suffix = (uint)reader.Read(8) - 1;
                        break;

                    case Item.QualityType.set:
                        item.set_code = (uint)reader.Read(12);
                        break;
                    case Item.QualityType.unique:
                        if (item.type != "std") //standard of heroes exception?
                            item.unique_code = (uint)reader.Read(12);
                        break;
                }
            }

            if (item.quality == Item.QualityType.rare || item.quality == Item.QualityType.crafted)
            {
                for (ulong i = 0; i < 3; i++)
                {
                    if (reader.ReadBit())
                        item.prefixes.Add((uint)reader.Read(11));
                    if (reader.ReadBit())
                        item.suffixes.Add((uint)reader.Read(11));
                }
            }

            if (item.rune_word)
            {
                item.runeword_id = (uint)reader.Read(12);
                item.runeword_parameter = (byte)reader.Read(4);
                //std::cout << "runeword_id: " << item.runeword_id << ", parameter: " << item.runeword_parameter << std::endl;
            }

            if (item.personalised)
            {
                List<Byte> personalised_name = new List<byte>();
                reader.Read(8);
                while (personalised_name.Last() != 0x00)
                {
                    reader.Read(8); // 16 characters of 7 bits each for the name of the ear to process later
                }
                item.personalised_name = Convert.ToBase64String(personalised_name.ToArray()); //this is also a problem part i'm not sure about

            }

            if (item.is_armor)
                item.defense = (uint)reader.Read(11) - 10;

            if (item.type == "7cr")
                reader.Read(8);
            else if (item.is_armor || item.is_weapon)
            {
                item.maximum_durability = (byte)reader.Read(8);
                item.indestructible = (uint)((item.maximum_durability == 0) ? 1 : 0);

                item.durability = (byte)reader.Read(8);
                reader.ReadBit();
            }
            if (item.has_sockets)
                item.sockets = (byte)reader.Read(4);
        }
        public static Item Parse(List<byte> packet)
        {
            Item item = new Item();
            BitReader reader = new BitReader(packet.ToArray());
            try
            {
                GenericInfo(reader, ref item);
                StatusInfo(reader, ref item);
                getLocation(reader, ref item);
                if (EarInfo(reader, ref item)) return item;
                if (GetItemType(reader, ref item)) return item;
                GetSocketInfo(reader, ref item);
                if (!GetLevelQuality(reader, ref item)) return item;
                GetGraphicInfo(reader, ref item);
                GetIdentifiedInfo(reader, ref item); // get nova to help with this
            }
            catch
            {
            }
            return item;
        }
    }
}
