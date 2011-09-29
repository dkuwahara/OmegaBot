using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BattleNet.Connections.Readers;
using BattleNet.Connections.Handlers;
using System.Net;
using System.Threading;
using BattleNet.Logging;

namespace BattleNet.Connections
{
    class D2GS : IDisposable
    {
        D2gsConnection m_d2gsConnection;

        D2gsReader m_d2gsReader;
        Thread m_d2gsReaderThread;
        
        D2gsHandler m_d2gsHandler;
        Thread m_d2gsHandlerThread;

        GameServerPing m_gsPing;
        Thread m_gsPingThread;

        GameThread m_gameThread;
        Thread m_botThread;

        AsciiMap m_asciiMap;
        Thread m_mapThread;

        public D2GS(String character, String account, UInt32 chickenLife, UInt32 potLife)
        {
            //ConnectedToGs = false;
            m_d2gsConnection = new D2gsConnection();
            m_d2gsReader = new D2gsReader(ref m_d2gsConnection, character);
            m_d2gsHandler = new D2gsHandler(ref m_d2gsConnection);
            m_gsPing = new GameServerPing(ref m_d2gsConnection);
            m_gameThread = new GameThread(m_d2gsConnection, chickenLife, potLife);
            m_asciiMap = new AsciiMap(m_gameThread.GameData, m_d2gsConnection);

            m_d2gsConnection.StartThread += delegate {
                m_d2gsHandlerThread = new Thread(m_d2gsHandler.ThreadFunction);
                m_d2gsHandlerThread.Name = account + " [D2Gs]:";
                m_d2gsReaderThread = new Thread(m_d2gsReader.ThreadFunction);
                m_d2gsReaderThread.Name = account + " [D2GS]:";

                m_d2gsHandlerThread.Start();
                m_d2gsReaderThread.Start();
            };
            
            m_d2gsHandler.StartPinging += delegate
            {
                m_gsPingThread = new Thread(m_gsPing.Run);
                m_gsPingThread.Name = account + " [D2GS]:";
                m_gsPingThread.Start();
                m_mapThread = new Thread(m_asciiMap.ThreadFunction);
                m_mapThread.Start();
            };

            
            m_botThread = new Thread(m_gameThread.BotThread);
            m_botThread.Name = account + " [BOT]:";
            m_botThread.Start();

            SubscribeGameServerEvents();
        }

        public void SubscribeGameServerEvents()
        {
            m_d2gsHandler.UpdateActData += delegate(Globals.ActType act, Int32 mapId, Int32 areaId)
            {
                m_gameThread.GameData.CurrentAct = act;
                m_gameThread.GameData.MapId = mapId;
                m_gameThread.GameData.AreaId = areaId;

                if (!m_gameThread.GameData.InGame)
                {
                    m_gameThread.m_startRun.Set();
                    m_gameThread.GameData.InGame = true;
                }
            };

            m_d2gsHandler.UpdateWorldObject += delegate(UInt16 type, WorldObject ent)
            {
                // Pindles portal
                if (type == 0x003c)
                {
                    m_gameThread.GameData.RedPortal = ent;
                    Logger.Write("Received red portal ID and coordinates");
                }
                /*
                // A5 WP
                if (type == 0x01ad)
                {
                    m_harrogathWp.Id = BitConverter.ToUInt32(packet, 2);
                    m_harrogathWp.Location.X = BitConverter.ToUInt16(packet, 8);
                    m_harrogathWp.Location.Y = BitConverter.ToUInt16(packet, 10);

                    if (debugging)
                        Console.WriteLine("{0}: [D2GS] Received A5 WP id and coordinates", Account);
                }
                 */
                // A1 WP
                else if (type == 0x0077)
                {
                    m_gameThread.GameData.RogueEncampmentWp = ent;
                    Logger.Write("Received A1 WP id and coordinates");
                }
                else
                {
                    if(m_gameThread.GameData.WorldObjects.ContainsKey(ent.Id))
                    {
                        m_gameThread.GameData.WorldObjects[ent.Id] = ent;
                    }
                    else
                    {
                        m_gameThread.GameData.WorldObjects.Add(ent.Id, ent);
                    }
                }
            };
            m_d2gsHandler.UpdateExperience += delegate(UInt32 exp) { m_gameThread.GameData.Experience += exp; };
            m_d2gsHandler.SetPlayerLevel += delegate(byte level) { m_gameThread.GameData.Me.Level = level; };
            m_d2gsHandler.UpdateItemSkill += delegate(Skills.Type skill, byte level)
            {
                if(m_gameThread.GameData.ItemSkillLevels.ContainsKey(skill))
                {
                    m_gameThread.GameData.ItemSkillLevels[skill] += level;
                }
                else
                {
                    m_gameThread.GameData.ItemSkillLevels.Add(skill,level);
                }
                
            };
            m_d2gsHandler.UpdateNpcLife += delegate(UInt32 id, byte life)
            {
                m_gameThread.GameData.Npcs[id].Life = life;
            };
            m_d2gsHandler.UpdatePlayerPosition += delegate(UInt32 id, Coordinate coords, bool directoryKnown)
            {
                Player player = m_gameThread.GameData.GetPlayer(id);
                player.Location = coords;
                player.DirectoryKnown = directoryKnown;
            };
            m_d2gsHandler.PlayerExited += delegate(UInt32 id)
            {
                if (m_gameThread.GameData.Players.ContainsKey(id))
                {
                    m_gameThread.GameData.Players.Remove(id);
                }
            };
            m_d2gsHandler.PlayerEnters += delegate(Player player)
            {
                Logging.Logger.Write("Adding new Player {0}", player.Name);
                if (m_gameThread.GameData.Players.ContainsKey(player.Id))
                {
                    m_gameThread.GameData.Players[player.Id] = player;
                }
                else
                {
                    m_gameThread.GameData.Players.Add(player.Id, player);
                }
            };
            m_d2gsHandler.InitMe += delegate(Player player)
            {
                Logging.Logger.Write("Initializing Self");
                m_gameThread.GameData.Me = player;
            };
            m_d2gsHandler.UpdateNpcMovement += delegate(UInt32 id, Coordinate coord, bool moving, bool running)
            {
                NpcEntity npc = m_gameThread.GameData.Npcs[id];
                npc.Location = coord;
                npc.Moving = moving;
                npc.Running = running;
            };
            m_d2gsHandler.NpcMoveToTarget += delegate(UInt32 id, Coordinate coord, bool moving, bool running)
            {
                NpcEntity npc = m_gameThread.GameData.Npcs[id];
                npc.Location = coord;
                npc.Moving = moving;
                npc.Running = running;
            };
            m_d2gsHandler.UpdateNpcState += delegate(UInt32 id, Coordinate coord, byte life)
            {
                //Logging.Logger.Write("Updating NPC {0}, ({1},{2}), Life:{3}", id, coord.X, coord.Y, life);
                NpcEntity npc = m_gameThread.GameData.Npcs[id];
                npc.Location = coord;
                npc.Life = life;
            };
            m_d2gsHandler.MercUpdateEvent += delegate(UInt32 id, UInt32 mercId)
            {
                Logging.Logger.Write("Mercenary for 0x{0:X} found your id: 0x{1:X}", id, m_gameThread.GameData.Me.Id);
                if (id == m_gameThread.GameData.Me.Id)
                {
                    m_gameThread.GameData.Me.MercenaryId = mercId;
                    m_gameThread.GameData.Me.HasMecenary = true;
                    m_gameThread.GameData.HasMerc = true;
                }
                else
                {
                    m_gameThread.GameData.Players[id].MercenaryId = mercId;
                    m_gameThread.GameData.Players[id].HasMecenary = true;
                }
            };

            m_d2gsHandler.PortalUpdateEvent += delegate(UInt32 ownerId, UInt32 portalId)
            {
                Logging.Logger.Write("Town Portal belonging to 0x{0:X} found ", ownerId);
                if (ownerId == m_gameThread.GameData.Me.Id)
                {
                    m_gameThread.GameData.Me.PortalId = portalId;
                }
                else
                {
                    m_gameThread.GameData.Players[ownerId].PortalId = portalId;
                }
            };
            m_d2gsHandler.UpdateTimestamp += delegate { m_gameThread.GameData.LastTimestamp = (int)System.DateTime.Now.ToFileTimeUtc(); };

            m_d2gsHandler.SwapWeaponSet += delegate { m_gameThread.GameData.WeaponSet = m_gameThread.GameData.WeaponSet == 0 ? 1 : 0; };

            m_d2gsHandler.UpdateSkillLevel += delegate(Skills.Type skill, byte level)
            {
                Logging.Logger.Write("Adding new Skill {0}:{1}", skill, level);
                m_gameThread.GameData.SkillLevels.Add(skill, level);
            };

            m_d2gsHandler.UpdateItemSkill += delegate(Skills.Type skill, byte level)
            {
                m_gameThread.GameData.ItemSkillLevels[(Skills.Type)skill] = level;
            };

            m_d2gsHandler.UpdateLife += delegate(UInt32 plife)
            {
                if (m_gameThread.GameData.CurrentLife == 0)
                    m_gameThread.GameData.CurrentLife = plife;

                if (plife < m_gameThread.GameData.CurrentLife && plife > 0)
                {
                    UInt32 damage = m_gameThread.GameData.CurrentLife - plife;
                    Logger.Write("{0} damage was dealt to {1} ({2} left)", damage, m_gameThread.GameData.Me.Name, plife);
                    if (plife <= m_gameThread.GameData.ChickenLife)
                    {
                        Logger.Write("Chickening with {0} left!", plife);

                        Logger.Write("Leaving the game.");
                        m_gameThread.SendPacket((byte)0x69);

                        Thread.Sleep(500);
                        m_d2gsConnection.Kill();
                    }
                    else if (plife <= m_gameThread.GameData.PotLife)
                    {
                        Logger.Write("Attempting to use potion with {1} life left.", plife);
                        m_gameThread.UsePotion();
                    }
                }

                m_gameThread.GameData.CurrentLife = plife;
            };

            m_d2gsHandler.NewItem += delegate(Item item)
            {
                lock (m_gameThread.GameData.Items)
                {
                    if (m_gameThread.GameData.Items.ContainsKey(item.id))
                        m_gameThread.GameData.Items[item.id] = item;
                    else
                        m_gameThread.GameData.Items.Add(item.id, item);
                    
                }
                if (!item.ground && !item.unspecified_directory)
                {
                    switch (item.container)
                    {
                        case Item.ContainerType.inventory:
                            m_gameThread.GameData.Inventory.Add(item);
                            //Console.WriteLine("New Item in Inventory!");
                            break;
                        case Item.ContainerType.cube:
                            m_gameThread.GameData.Cube.Add(item);
                            break;
                        case Item.ContainerType.stash:
                            m_gameThread.GameData.Stash.Add(item);
                            break;
                        case Item.ContainerType.belt:
                            m_gameThread.GameData.Belt.Add(item);
                            break;
                    }
                }
            };

            m_d2gsHandler.AddNpcEvent += delegate(NpcEntity npc)
            {
                //Logging.Logger.Write("Adding new NPC {0}", npc);
                if (m_gameThread.GameData.Npcs.ContainsKey(npc.Id))
                    m_gameThread.GameData.Npcs[npc.Id] = npc;
                else
                    m_gameThread.GameData.Npcs.Add(npc.Id, npc);
            };

            m_d2gsHandler.NpcTalkedEvent += delegate 
            {
                Logger.Write("Talked to NPC");
                m_gameThread.GameData.TalkedToNpc = true; 
            };
        }

        public void SubscribeNextGameEvent(D2gsReader.NoParam sub)
        {
            m_d2gsReader.NextGame += sub;
        }

        public void UpdateCharacterName(String name)
        {
            m_d2gsReader.Character = name;
        }
        public void UpdateClassByte(byte classByte)
        {
            m_d2gsReader.ClassByte = classByte;
        }

        public void SetGSInfo(List<byte> hash, List<byte> token)
        {
            m_d2gsReader.SetInfo(hash, token);
        }
        public void KillAll()
        {
            m_d2gsConnection.Kill();
            
            if (m_gsPingThread.IsAlive)
                m_gsPingThread.Join();
        }
        public void Init(IPAddress ip, ushort port, List<byte> data)
        {
            m_gameThread.GameData.Init();
            Logging.Logger.Write("Initializing D2GS");
            if (!m_d2gsConnection.Init(ip, port, data))
            {
                m_d2gsReader.Die();
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            m_d2gsConnection.Close();
        }

        #endregion
    }
}
