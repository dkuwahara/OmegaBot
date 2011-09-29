using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BattleNet.Logging;

namespace BattleNet.Connections.Handlers
{
    class D2gsHandler : GenericDispatcher
    {
        public D2gsHandler(ref D2gsConnection conn)
            : base(conn)
        {
        }

        public override void ThreadFunction()
        {
            firstInfoPacket = true;
            Logger.Write("D2GS handler started!");
            while (m_connection.Socket.Connected)
            {
                if (m_connection.Packets.IsEmpty())
                {
                    //m_connection.PacketsReady.WaitOne();
                }
                else
                {
                    List<byte> packet;
                    lock (m_connection.Packets)
                    {
                        packet = m_connection.Packets.Dequeue();
                    }
                    byte type = packet[0];
                    DispatchPacket(type)(type, packet);
                }
            }
            Logger.Write("D2GS Handler Ending...");
        }

        public delegate void PingStarter();
        public event PingStarter StartPinging = delegate { };

        public override PacketHandler DispatchPacket(byte type)
        {
            switch (type)
            {
                case 0x00: return GameLoading;
                case 0x01: return GameFlagsPing;
                case 0x02: return StartPingThread;
                case 0x03: return LoadActData;
                case 0x0c: return NpcUpdate;
                case 0x0f: return PlayerMove;
                case 0x15: return PlayerReassign;
                case 0x1a:
                case 0x1b:
                case 0x1c: return ProcessExperience;
                case 0x1d: return BaseAttribute;
                case 0x21:
                case 0x22: return ItemSkillBonus;
                case 0x26: return ChatMessage;
                case 0x27: return NpcInteraction;
                case 0x51: return WorldObject;
                case 0x5b: return PlayerJoins;
                case 0x5c: return PlayerLeaves;
                case 0x59: return InitializePlayer;
                case 0x67: return NpcMovement;
                case 0x68: return NpcMoveEntity;
                case 0x69: return NpcStateUpdate;
                case 0x6d: return NpcStoppedMoving;
                case 0x81: return MercUpdate;
                case 0x82: return PortalUpdate;
                case 0x8f: return Pong;
                case 0x94: return SkillPacket;
                case 0x95: return LifeManaPacket;
                case 0x97: return WeaponSetSwitched;
                case 0x9c:
                case 0x9d: return ItemAction;
                case 0xac: return NpcAssignment;
                default: return VoidRequest;
            }
        }
        
        protected void GameLoading(byte type, List<byte> data)
        {
            Logger.Write("Game is loading, please wait...");
        }

        protected void GameFlagsPing(byte type, List<byte> data)
        {
            Logger.Write("Game flags ping");
            List<byte> packet = new List<byte>();
            packet.Add(0x6d);
            packet.AddRange(BitConverter.GetBytes((uint)System.Environment.TickCount));
            packet.AddRange(nulls);
            packet.AddRange(nulls);
            /*
             m_connection.BuildPacket(0x6d, BitConverter.GetBytes((uint)System.Environment.TickCount),
                              nulls, nulls);
             */
            m_connection.Write(packet.ToArray());
        }
        public delegate void NewEntity(UInt16 type, WorldObject ent);
        public event NewEntity UpdateWorldObject;
        protected void WorldObject(byte type, List<byte> data)
        {
            byte[] packet = data.ToArray();
            if (packet[1] == 0x02)
            {
                UInt16 obj = BitConverter.ToUInt16(packet, 6);
                UpdateWorldObject(obj, new WorldObject(BitConverter.ToUInt32(packet, 2),
                                                    obj,
                                                    BitConverter.ToUInt16(packet, 8),
                                                    BitConverter.ToUInt16(packet, 10)));
            }
        }

        protected void StartPingThread(byte type, List<byte> data)
        {
            Logger.Write("Starting Ping thread");
            m_connection.Stream.WriteByte(0x6b);
            StartPinging();
        }

        public delegate void ActData(Globals.ActType act, Int32 mapId, Int32 areaId);
        public event ActData UpdateActData = delegate { };

        protected void LoadActData(byte type, List<byte> data)
        {
            byte[] packet = data.ToArray();
            
            Logger.Write("Loading Act Data");

            Globals.ActType currentAct = (Globals.ActType)data[1];
            Int32 mapId = BitConverter.ToInt32(packet, 2);
            Int32 areaId = BitConverter.ToInt32(packet, 6);

            UpdateActData(currentAct, mapId, areaId);
            /*
            if (!m_fullEntered)
            {
                m_fullEntered = true;
                Logger.Write("Fully Entered Game.");
            }
            */
        }

        public delegate void NpcLifeUpdate(UInt32 id, byte life);
        public event NpcLifeUpdate UpdateNpcLife = delegate { };

        protected void NpcUpdate(byte type, List<byte> data)
        {
            UInt32 id = BitConverter.ToUInt32(data.ToArray(), 2);
            UpdateNpcLife(id, data[8]);
            //m_owner.BotGameData.Npcs[id].Life = data[8];
        }

        public delegate void PlayerPositionUpdate(UInt32 id, Coordinate coords, bool directoryKnown);
        public event PlayerPositionUpdate UpdatePlayerPosition = delegate { };

        protected void PlayerMove(byte type, List<byte> data)
        {
            Logger.Write("A player is moving");
            byte[] packet = data.ToArray();
            UInt32 playerId = BitConverter.ToUInt32(packet, 2);
            Coordinate coords = new Coordinate(BitConverter.ToUInt16(packet, 7), BitConverter.ToUInt16(packet, 9));
            UpdatePlayerPosition(playerId, coords, true);
            /*
            Player current_player = m_owner.BotGameData.GetPlayer(playerId);
            current_player.Location = new Coordinate(BitConverter.ToUInt16(packet, 7), BitConverter.ToUInt16(packet, 9));
            current_player.DirectoryKnown = true;
             */
        }

        protected void PlayerReassign(byte type, List<byte> data)
        {
            byte[] packet = data.ToArray();
            UInt32 id = BitConverter.ToUInt32(packet, 2);
            Coordinate coords = new Coordinate(BitConverter.ToUInt16(packet, 6), BitConverter.ToUInt16(packet, 8));
            UpdatePlayerPosition(id, coords, true);
            /*
            Player current_player = m_owner.BotGameData.GetPlayer(id);
            current_player.Location = new Coordinate(BitConverter.ToUInt16(packet, 6), BitConverter.ToUInt16(packet, 8));
             */
        }

        public delegate void ExperienceUpdate(UInt32 exp);
        public event ExperienceUpdate UpdateExperience = delegate { };
        protected void ProcessExperience(byte type, List<byte> data)
        {
            byte[] packet = data.ToArray();
            UInt32 exp = 0;
            if (type == 0x1a)
                exp = data[1];
            else if (type == 0x1b)
                exp = BitConverter.ToUInt16(packet, 1);
            else if (type == 0x1c)
                exp = BitConverter.ToUInt32(packet, 1);

            UpdateExperience(exp);
        }

        public delegate void PlayerLevelSet(byte level);
        public event PlayerLevelSet SetPlayerLevel = delegate { };
        byte m_level;
        protected void BaseAttribute(byte type, List<byte> data)
        {
            if (data[1] == 0x0c)
            {
                SetPlayerLevel(data[2]);
                m_level = data[2];
                //Console.WriteLine("Setting Player Level: {0}", data[2]);
            }
        }

        public delegate void SkillUpdate(Skills.Type skill, byte level);
        public event SkillUpdate UpdateItemSkill = delegate { };
        protected void ItemSkillBonus(byte type, List<byte> data)
        {
            UInt32 skill, amount;
            skill = BitConverter.ToUInt16(data.ToArray(), 7);
            if (type == 0x21)
                amount = data[10];
            else
                amount = data[9];

            //Console.WriteLine("Setting Skill: {0} bonus to {1}", skill, amount);
            UpdateItemSkill((Skills.Type)skill, (byte)amount);
        }

        protected void ChatMessage(byte type, List<byte> data)
        {
        }

        private bool firstInfoPacket;
        private bool talkedToNpc;

        public event NoParams NpcTalkedEvent = delegate { };
        protected void NpcInteraction(byte type, List<byte> data)
        {
            if (firstInfoPacket)
                firstInfoPacket = false;
            else
            {
                Logger.Write("{0}: [D2GS] Talking to an NPC.");
                talkedToNpc = true;
                UInt32 id = BitConverter.ToUInt32(data.ToArray(), 2);
                m_connection.Write(m_connection.BuildPacket(0x2f, one, BitConverter.GetBytes(id)));
                NpcTalkedEvent();
            }
        }

        public delegate void PlayerLeft(UInt32 id);
        public event PlayerLeft PlayerExited = delegate { };
        protected void PlayerLeaves(byte type, List<byte> data)
        {
            UInt32 id = BitConverter.ToUInt32(data.ToArray(), 1);
            PlayerExited(id);
            //m_owner.BotGameData.Players.Remove(id);
        }

        public delegate void NewPlayer(Player newPlayer);
        public event NewPlayer PlayerEnters = delegate { };
        Player me;
        protected void PlayerJoins(byte type, List<byte> data)
        {
            byte[] packet = data.ToArray();
            UInt32 id = BitConverter.ToUInt32(packet, 3);
            if (id != me.Id)
            {
                String name = BitConverter.ToString(packet, 8, 15);
                Globals.CharacterClassType charClass = (Globals.CharacterClassType)data[7];
                UInt32 level = BitConverter.ToUInt16(packet, 24);
                Player newPlayer = new Player(name, id, charClass, level);
                PlayerEnters(newPlayer);
                //m_owner.BotGameData.Players.Add(id, newPlayer);
            }
        }

        public event NewPlayer InitMe = delegate { };
        protected void InitializePlayer(byte type, List<byte> data)
        {
            byte[] packet = data.ToArray();
            UInt32 id = BitConverter.ToUInt32(packet, 1);
            Globals.CharacterClassType charClass = (Globals.CharacterClassType)data[5];
            String name = BitConverter.ToString(packet, 6, 15);
            UInt16 x = BitConverter.ToUInt16(packet, 22);
            UInt16 y = BitConverter.ToUInt16(packet, 24);
            Player newPlayer = new Player(name, id, charClass, m_level, x, y);
            me = newPlayer;
            InitMe(me);
        }

        public delegate void NpcUpdateDel(UInt32 id, Coordinate coord, bool moving, bool running);
        public event NpcUpdateDel UpdateNpcMovement = delegate { };
        protected void NpcMovement(byte type, List<byte> data)
        {
            byte[] packet = data.ToArray();
            UInt32 id = BitConverter.ToUInt32(packet, 1);
            byte movementType = packet[5];
            UInt16 x = BitConverter.ToUInt16(packet, 6);
            UInt16 y = BitConverter.ToUInt16(packet, 8);
            bool running;
            if (movementType == 0x17)
                running = true;
            else if (movementType == 0x01)
                running = false;
            else
                return;

            UpdateNpcMovement(id, new Coordinate(x, y), true, running);           
        }

        public event NpcUpdateDel NpcMoveToTarget = delegate { };
        protected void NpcMoveEntity(byte type, List<byte> data)
        {
            byte[] packet = data.ToArray();
            UInt32 id = BitConverter.ToUInt32(packet, 1);
            byte movementType = packet[5];
            UInt16 x = BitConverter.ToUInt16(packet, 6);
            UInt16 y = BitConverter.ToUInt16(packet, 8);
            bool running;
            if (movementType == 0x18)
                running = true;
            else if (movementType == 0x00)
                running = false;
            else
                return;

            NpcMoveToTarget(id, new Coordinate(x, y), true, running);          
        }

        public delegate void NpcStateUpdateDel(UInt32 id, Coordinate coord, byte life);
        public event NpcStateUpdateDel UpdateNpcState = delegate { };
        protected void NpcStateUpdate(byte type, List<byte> data)
        {
            byte[] packet = data.ToArray();
            UInt32 id = BitConverter.ToUInt32(packet, 1);
            byte state = packet[5];

            byte life;
            if (state == 0x09 || state == 0x08)
                life = 0;
            else
                life = packet[10];

            UpdateNpcState(id, new Coordinate(BitConverter.ToUInt16(packet, 6),
                               BitConverter.ToUInt16(packet, 8)), life);
        }

        protected void NpcStoppedMoving(byte type, List<byte> data)
        {
            byte[] packet = data.ToArray();
            UInt32 id = BitConverter.ToUInt32(packet, 1);
            UInt16 x = BitConverter.ToUInt16(packet, 5);
            UInt16 y = BitConverter.ToUInt16(packet, 7);
            byte life = packet[9];

            UpdateNpcMovement(id, new Coordinate(x, y), false, false);
            UpdateNpcLife(id, life);
        }

        public delegate void MercenaryUpdate(UInt32 id, UInt32 mercId);
        public event MercenaryUpdate MercUpdateEvent = delegate { };
        protected void MercUpdate(byte type, List<byte> data)
        {
            byte[] packet = data.ToArray();
            UInt32 id = BitConverter.ToUInt32(packet, 4);
            UInt32 mercId = BitConverter.ToUInt32(packet, 8);

            MercUpdateEvent(id, mercId);
        }

        public delegate void PortalAssign(UInt32 ownerId, UInt32 portalId);
        public event PortalAssign PortalUpdateEvent = delegate { };
        protected void PortalUpdate(byte type, List<byte> data)
        {
            byte[] packet = data.ToArray();
            int offset = 5;
            UInt32 ownerId = BitConverter.ToUInt32(packet, 1);

            PortalUpdateEvent(ownerId, BitConverter.ToUInt32(packet, 21));

            //String name = System.Text.Encoding.ASCII.GetString(packet, offset, 15);
            /*
            if (name.Substring(0, m_owner.Me.Name.Length) == m_owner.BotGameData.Me.Name)
            {
                Logger.Write("Received new portal id");
                m_owner.BotGameData.Me.PortalId = BitConverter.ToUInt32(packet, 21);
            }
             */
        }

        public delegate void NoParams();
        public event NoParams UpdateTimestamp = delegate { };
        protected void Pong(byte type, List<byte> data)
        {
            UpdateTimestamp();
        }

        public delegate void LifeUpdate(UInt32 plife);
        public event LifeUpdate UpdateLife = delegate { };
        protected void LifeManaPacket(byte type, List<byte> data)
        {
            byte[] packet = data.ToArray();
            if (BitConverter.ToUInt16(packet, 6) == 0x0000)
                return;

            UInt32 plife = (uint)BitConverter.ToUInt16(packet, 1) & 0x7FFF;

            UpdateLife(plife);
        }

        public event NoParams SwapWeaponSet = delegate { };
        protected void WeaponSetSwitched(byte type, List<byte> data)
        {
            SwapWeaponSet();
        }

        public event SkillUpdate UpdateSkillLevel = delegate { };
        protected void SkillPacket(byte type, List<byte> data)
        {
            byte[] packet = data.ToArray();
            byte skillCount = packet[1];
            int offset = 6;
            for (int i = 0; i < skillCount; i++)
            {
                UInt16 skill = BitConverter.ToUInt16(packet, offset);
                byte level = packet[offset + 2];
                UpdateSkillLevel((Skills.Type)skill, level);
                offset += 3;
            }
        }
        public delegate void ItemUpdate(Item item);
        public event ItemUpdate NewItem = delegate { };
        protected void ItemAction(byte type, List<byte> data)
        {
            Item item = Items.Parser.Parse(data);
            NewItem(item);
        }

        public static bool BitScanReverse(out int index, ulong mask)
        {
            index = 0;
            while (mask > 1)
            {
                mask >>= 1;
                index++;
            }
            return mask == 1;
        }
        public delegate void AddNpcDelegate(NpcEntity npc);
        public event AddNpcDelegate AddNpcEvent;
        protected void NpcAssignment(byte type, List<byte> data)
        {
            byte[] packet = data.ToArray();
            NpcEntity output;
            //try
            //{
            BitReader br = new BitReader(data.ToArray());
            br.ReadBitsLittleEndian(8);
            UInt32 id = (uint)br.Read(32);
            UInt16 npctype = (ushort)br.Read(16);
            UInt16 x = (ushort)br.Read(16);
            UInt16 y = (ushort)br.Read(16);
            byte life = (byte)br.Read(8);
            byte size = (byte)br.Read(8);

            output = new NpcEntity(id, npctype, life, x, y);

            int informationLength = 16;

            String[] entries;

            if (!DataManager.Instance.m_monsterFields.Get(npctype, out entries))
                Logger.Write("Failed to read monstats data for NPC of type {0}", type);
            if (entries.Length != informationLength)
                Logger.Write("Invalid monstats entry for NPC of type {0}", type);

            bool lookupName = false;

            if (data.Count > 0x10)
            {
                br.Read(4);
                if (br.ReadBit())
                {
                    for (int i = 0; i < informationLength; i++)
                    {
                        int temp;

                        int value = Int32.Parse(entries[i]);

                        if (!BitScanReverse(out temp, (uint)value - 1))
                            temp = 0;
                        if (temp == 31)
                            temp = 0;

                        //Console.WriteLine("BSR: {0} Bitcount: {1}", temp+1, bitCount);
                        int bits = br.Read(temp + 1);
                    }
                }

                output.SuperUnique = false;

                output.HasFlags = br.ReadBit();
                if (output.HasFlags)
                {
                    output.Champion = br.ReadBit();
                    output.Unique = br.ReadBit();
                    output.SuperUnique = br.ReadBit();
                    output.IsMinion = br.ReadBit();
                    output.Ghostly = br.ReadBit();
                    //Console.WriteLine("{0} {1} {2} {3} {4}", output.Champion, output.Unique, output.SuperUnique, output.IsMinion, output.Ghostly);
                }

                if (output.SuperUnique)
                {
                    output.SuperUniqueId = br.ReadBitsLittleEndian(16);
                    String name;
                    if (!DataManager.Instance.m_superUniques.Get(output.SuperUniqueId, out name))
                    {
                        Logger.Write("Failed to lookup super unique monster name for {0}", output.SuperUniqueId);
                        output.Name = "invalid";
                    }
                    else
                    {
                        output.Name = name;
                        //Console.WriteLine("NPC: {0}", name);
                    }
                }
                else
                    lookupName = true;

                if (data.Count > 17 && lookupName != true && output.Name != "invalid")
                {
                    output.IsLightning = false;
                    while (true)
                    {
                        byte mod = (byte)br.ReadBitsLittleEndian(8);
                        if (mod == 0)
                            break;
                        if (mod == 0x11)
                            output.IsLightning = true;
                    }
                }
            }
            else
                lookupName = true;

            if (lookupName)
            {
                String name;
                if (!DataManager.Instance.m_monsterNames.Get((int)output.Type, out name))
                    Console.WriteLine("Failed to Look up monster name for {0}", output.Type);
                else
                    output.Name = name;

                //Console.WriteLine("NPC: {0}", name);
            }

            AddNpcEvent(output);
        }
        protected void VoidRequest(byte type, List<byte> data)
        {
            //Logger.Write("Unknown Packet 0x{0:X2} received!", type);
        }

    }
}
