using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using BattleNet.Connections;
using BattleNet.Logging;
using System.IO;

namespace BattleNet
{
    class GameThread
    {
        private GameData m_gameData;
        public GameData GameData { get { return m_gameData; } set { m_gameData = value; } }
        
        private D2gsConnection m_d2gsConnection;

        public GameThread(D2gsConnection connection, UInt32 chickenLife, UInt32 potLife)
        {
            m_d2gsConnection = connection;
            m_gameData = new GameData();
            m_gameData.ChickenLife = chickenLife;
            m_gameData.PotLife = potLife;
        }

        public static int Time()
        {
            return System.Environment.TickCount / 1000;
        }

        public AutoResetEvent m_startRun;
        public void BotThread()
        {
            Logging.Logger.Write("Starting Bot thread");
            m_startRun = new AutoResetEvent(false);
            m_startRun.WaitOne();
            while (true)
            {
                Logging.Logger.Write("Signalled to start bot");
                Int32 startTime = Time();
                Logging.Logger.Write("Bot is in town.");
                Thread.Sleep(3000);

                StashItems();

                MoveToAct5();
                /*
                if (m_gameData.WeaponSet != 0)
                    WeaponSwap();
                */
                DoPindle();

                Logging.Logger.Write("Leaving the game.");
                LeaveGame();
                Thread.Sleep(500);

                Int32 endTime = Time() - startTime;
                Logging.Logger.Write("Run took {0} seconds.", endTime);
                m_startRun.WaitOne();
            }
        }

        public void SendPacket(byte command, params IEnumerable<byte>[] args)
        {
            m_d2gsConnection.Write(m_d2gsConnection.BuildPacket(command, args));
        }
        public void SendPacket(byte[] packet)
        {
            m_d2gsConnection.Write(packet);
        }

        public void LeaveGame()
        {
            Logging.Logger.Write("Leaving the game.");
            m_d2gsConnection.Write(m_d2gsConnection.BuildPacket(0x69));

            Thread.Sleep(500);

            m_d2gsConnection.Kill();
            
            //Status = ClientStatus.STATUS_NOT_IN_GAME;
        }

        public void MoveTo(UInt16 x, UInt16 y)
        {
            MoveTo(new Coordinate(x, y));
        }

        public void MoveTo(Coordinate target)
        {
            int time = Time();
            if (time - m_gameData.LastTeleport > 5)
            {
                SendPacket(Actions.Relocate(target));             
                m_gameData.LastTeleport = time;
                Thread.Sleep(120);
            }
            else
            {
                double distance = m_gameData.Me.Location.Distance(target);
                SendPacket(Actions.Run(target));
                Thread.Sleep((int)(distance * 80));
            }
            m_gameData.Me.Location = target;
        }

        public virtual void StashItems()
        {
            bool onCursor = false;
            List<Item> items;
            lock (m_gameData.Items)
            {
                items = new List<Item>(m_gameData.Items.Values);
            }
            foreach (Item i in items)
            {
                onCursor = false;

                if (i.action == (uint)Item.Action.to_cursor)
                    onCursor = true;
                else if (i.container == Item.ContainerType.inventory)
                    onCursor = false;
                else
                    continue;

                if (i.type == "tbk" || i.type == "cm1" || i.type == "cm2")
                    continue;

                Coordinate stashLocation;
                if (!m_gameData.Stash.FindFreeSpace(i, out stashLocation))
                {
                    continue;
                }

                Logger.Write("Stashing item {0}, at {1}, {2}", i.name, stashLocation.X, stashLocation.Y);

                if (!onCursor)
                {
                    SendPacket(0x19, BitConverter.GetBytes((UInt32)i.id));
                    Thread.Sleep(500);
                }

                SendPacket(0x18, BitConverter.GetBytes((UInt32)i.id), BitConverter.GetBytes((UInt32)stashLocation.X), BitConverter.GetBytes((UInt32)stashLocation.Y), new byte[] { 0x04, 0x00, 0x00, 0x00 });
                Thread.Sleep(400);
            }
        }

        public bool SwitchSkill(uint skill)
        {
            m_gameData.RightSkill = skill;
            byte[] temp = { 0xFF, 0xFF, 0xFF, 0xFF };
            SendPacket(Actions.SwitchSkill(skill));
            Thread.Sleep(100);
            return true;
        }

        public bool GetAliveNpc(String name, double range, out NpcEntity output)
        {
            var n = (from npc in m_gameData.Npcs
                     where npc.Value.Name == name
                     && npc.Value.Life > 0
                     && (range == 0 || range > m_gameData.Me.Location.Distance(npc.Value.Location))
                     select npc).FirstOrDefault();
            if (n.Value == null)
            {
                output = default(NpcEntity);
                return false;
            }
            output = n.Value;
            return true;
        }

        public NpcEntity GetNpc(String name)
        {
            NpcEntity npc = (from n in m_gameData.Npcs
                             where n.Value.Name == name
                             select n).FirstOrDefault().Value;
            return npc;
        }

        public bool Attack(UInt32 id)
        {
            if (!m_d2gsConnection.Socket.Connected)
                return false;
            m_gameData.CharacterSkillSetup = BattleNet.GameData.CharacterSkillSetupType.SORCERESS_LIGHTNING;
            switch (m_gameData.CharacterSkillSetup)
            {
                case GameData.CharacterSkillSetupType.SORCERESS_LIGHTNING:
                    if (m_gameData.RightSkill != (uint)Skills.Type.LIGHTNING)
                        SwitchSkill((uint)Skills.Type.LIGHTNING);
                    Thread.Sleep(300);
                    SendPacket(Actions.CastOnObject(id));
                    break;
                case GameData.CharacterSkillSetupType.SORCERESS_BLIZZARD:
                    if (m_gameData.RightSkill != (uint)Skills.Type.blizzard)
                        SwitchSkill((uint)Skills.Type.blizzard);
                    Thread.Sleep(300);
                    SendPacket(Actions.CastOnObject(id));
                    break;
                case GameData.CharacterSkillSetupType.SORCERESS_METEOR:
                    break;
                case GameData.CharacterSkillSetupType.SORCERESS_METEORB:
                    break;
            }
            return true;
        }

        public void MoveToAct5()
        {
            if (m_gameData.CurrentAct == BattleNet.Globals.ActType.ACT_I)
            {
                Logger.Write("Moving to Act 5");
                MoveTo(m_gameData.RogueEncampmentWp.Location);
                byte[] temp = { 0x02, 0x00, 0x00, 0x00 };
                SendPacket(0x13, temp, BitConverter.GetBytes(m_gameData.RogueEncampmentWp.Id));
                Thread.Sleep(300);
                byte[] tempa = { 0x6D, 0x00, 0x00, 0x00 };
                SendPacket(0x49, BitConverter.GetBytes(m_gameData.RogueEncampmentWp.Id), tempa);
                Thread.Sleep(300);
                MoveTo(5105, 5050);
                MoveTo(5100, 5025);
                MoveTo(5096, 5018);
            }
        }

        public virtual void PickItems()
        {
            var picking_items = (from i in m_gameData.Items
                                 where i.Value.ground
                                 select i.Value);

            lock (m_gameData.Items)
            {
                foreach (var i in picking_items)
                {
                    if (i.type != "mp5" && i.type != "hp5" && i.type != "gld" && i.type != "rvl" && i.quality > Item.QualityType.normal)
                    {
                        Logger.Write("{0}: {1}, {2}, Ethereal:{3}, {4}", i.name, i.type, i.quality, i.ethereal, i.sockets);
                    }
                }
                try
                {
                    foreach (var i in picking_items)
                    {
                        if (!Pickit.PickitMap.ContainsKey(i.type))
                            continue;
                        if (m_gameData.Belt.m_items.Count >= 16 && i.type == "rvl")
                            continue;
                        if (Pickit.PickitMap[i.type](i))
                        {
                            if (i.type != "gld" && i.type != "rvl")
                            {
                                Logger.Write("Picking up Item!");
                                Logger.Write("{0}: {1}, {2}, Ethereal:{3}, {4}", i.name, i.type, i.quality, i.ethereal, i.sockets);
                            }
                            SwitchSkill(0x36);
                            Thread.Sleep(200);
                            SendPacket(Actions.CastOnCoord((ushort)i.x, (ushort)i.y));
                            Thread.Sleep(400);
                            byte[] tempa = { 0x04, 0x00, 0x00, 0x00 };
                            SendPacket(0x16, tempa, BitConverter.GetBytes((Int32)i.id), GenericHandler.nulls);
                            Thread.Sleep(500);
                            if (i.type != "rvl" && i.type != "gld")
                            {
                                using (StreamWriter sw = File.AppendText("log.txt"))
                                {
                                    sw.WriteLine("{6} [{5}] {0}: {1}, {2}, Ethereal:{3}, {4}", i.name, i.type, i.quality, i.ethereal, i.sockets == uint.MaxValue ? 0 : i.sockets, m_gameData.Me.Name, DateTime.Today.ToShortTimeString());
                                }
                            }
                        }
                    }
                }
                catch
                {
                    Logging.Logger.Write("Failed to pickup items, something bad happened");
                }
            }
        }

        public void DoPindle()
        {
            UInt32 curLife = 0;

            MoveTo(5089, 5019);
            MoveTo(5090, 5030);
            MoveTo(5082, 5033);
            MoveTo(5074, 5033);

            if (!VisitMalah())
                return;

            MoveTo(5073, 5032);
            MoveTo(5073, 5044);
            MoveTo(5078, 5055);
            MoveTo(5081, 5065);
            MoveTo(5081, 5076);

            if (!ReviveMerc())
                return;

            MoveTo(5082, 5087);
            MoveTo(5085, 5098);
            MoveTo(5088, 5110);
            MoveTo(5093, 5121);
            MoveTo(5103, 5124);
            MoveTo(5111, 5121);

            EnterRedPortal();

            //Status = ClientStatus.STATUS_KILLING_PINDLESKIN;
            Logger.Write("Killing Pindleskin");

            //Precast();

            SwitchSkill(0x36);
            Thread.Sleep(300);

            SendPacket(Actions.CastOnCoord(10064, 13286));
            Thread.Sleep(300);
            SendPacket(Actions.CastOnCoord(10061, 13260));
            Thread.Sleep(300);
            SendPacket(Actions.CastOnCoord(10058, 13236));
            Thread.Sleep(300);
           
            NpcEntity pindle = GetNpc("Pindleskin");
            if (pindle == default(NpcEntity))
            {
                Thread.Sleep(500);
                pindle = GetNpc("Pindleskin");
                if (pindle == default(NpcEntity))
                {
                    Logger.Write("Unable to find Pindleskin, probably got stuck.");
                    LeaveGame();
                    return;
                }
            }
            curLife = m_gameData.Npcs[pindle.Id].Life;
            if (m_gameData.Npcs[pindle.Id].IsLightning && m_gameData.CharacterSkillSetup == GameData.CharacterSkillSetupType.SORCERESS_LIGHTNING)
            {
                LeaveGame();
                return;
            }
            while (m_gameData.Npcs[pindle.Id].Life > 0 && m_d2gsConnection.Socket.Connected)
            {
                if (!Attack(pindle.Id))
                {
                    LeaveGame();
                    return;
                }
                if (curLife > m_gameData.Npcs[pindle.Id].Life)
                {
                    curLife = m_gameData.Npcs[pindle.Id].Life;
                    //Console.WriteLine("{0}: [D2GS] Pindleskins Life: {1}", Account, curLife);
                }
            }
            Logger.Write("{0} is dead. Killing minions", pindle.Name);

            NpcEntity monster;
            while (GetAliveNpc("Defiled Warrior", 20, out monster) && m_d2gsConnection.Socket.Connected)
            {
                curLife = m_gameData.Npcs[monster.Id].Life;
                Logger.Write("Killing Defiled Warrior");
                while (m_gameData.Npcs[monster.Id].Life > 0 && m_d2gsConnection.Socket.Connected)
                {
                    if (!Attack(monster.Id))
                    {
                        LeaveGame();
                        return;
                    }
                    if (curLife > m_gameData.Npcs[monster.Id].Life)
                    {
                        curLife = m_gameData.Npcs[monster.Id].Life;
                        //Console.WriteLine("{0}: [D2GS] Defiled Warriors Life: {1}", Account, curLife);
                    }
                }
            }
            Logger.Write("Minions are dead, looting...");
            PickItems();

            //if (!TownPortal())
            //{
            //LeaveGame();
            //return;
            //}
            
        }
        public UInt32 GetSkillLevel(Skills.Type skill)
        {
            return m_gameData.SkillLevels[skill] + m_gameData.ItemSkillLevels[skill];
        }

        public virtual bool UsePotion()
        {
            Item pot = (from n in GameData.Belt.m_items
                        where n.type == "rvl"
                        select n).FirstOrDefault();

            if (pot == default(Item))
            {
                Logger.Write("No potions found in belt!");
                return false;
            }
            SendPacket(0x26, BitConverter.GetBytes(pot.id), GenericHandler.nulls, GenericHandler.nulls);
            GameData.Belt.m_items.Remove(pot);
            return true;
        }

        public bool VisitMalah()
        {
            NpcEntity malah = GetNpc("Malah");
            if (malah != null && malah != default(NpcEntity))
                TalkToTrader(malah.Id);
            else
            {
                LeaveGame();
                return false;
            }

            if (GetSkillLevel(Skills.Type.book_of_townportal) < 10)
            {
                Thread.Sleep(300);
                SendPacket(0x38, GenericHandler.one, BitConverter.GetBytes(malah.Id), GenericHandler.nulls);
                Thread.Sleep(2000);
                Item n = (from item in m_gameData.Items
                          where item.Value.action == (uint)Item.Action.add_to_shop
                          && item.Value.type == "tsc"
                          select item).FirstOrDefault().Value;

                Logger.Write("Buying TPs");
                byte[] temp = { 0x02, 0x00, 0x00, 0x00 };
                for (int i = 0; i < 9; i++)
                {
                    SendPacket(0x32, BitConverter.GetBytes(malah.Id), BitConverter.GetBytes(n.id), GenericHandler.nulls, temp);
                    Thread.Sleep(200);
                }
                Thread.Sleep(500);
            }
            if (malah != null && malah != default(NpcEntity))
                SendPacket(0x30, GenericHandler.one, BitConverter.GetBytes(malah.Id));
            else
            {
                LeaveGame();
                return false;
            }

            Thread.Sleep(300);
            return true;
        }

        public bool TalkToTrader(UInt32 id)
        {
            m_gameData.TalkedToNpc = false;
            NpcEntity npc = m_gameData.Npcs[id];

            double distance = m_gameData.Me.Location.Distance(npc.Location);

            //if(debugging)
            Logger.Write("Attempting to talk to NPC");
            SendPacket(Actions.MakeEntityMove(id, m_gameData.Me.Location));
            
            int sleepStep = 200;
            for (int timeDifference = (int)distance * 120; timeDifference > 0; timeDifference -= sleepStep)
            {
                SendPacket(0x04, GenericHandler.one, BitConverter.GetBytes(id));
                Thread.Sleep(Math.Min(sleepStep, timeDifference));
            }

            SendPacket(0x13, GenericHandler.one, BitConverter.GetBytes(id));
            Thread.Sleep(200);
            SendPacket(0x2f, GenericHandler.one, BitConverter.GetBytes(id));

            int timeoutStep = 100;
            for (long npc_timeout = 4000; npc_timeout > 0 && !m_gameData.TalkedToNpc; npc_timeout -= timeoutStep)
                Thread.Sleep(timeoutStep);

            if (!m_gameData.TalkedToNpc)
            {
                Logger.Write("Failed to talk to NPC");
                return false;
            }
            return true;
        }

        public bool ReviveMerc()
        {
            if (!m_gameData.HasMerc)
            {
                Logger.Write("Reviving Merc");
                MoveTo(5082, 5080);
                MoveTo(5060, 5076);

                NpcEntity qual = GetNpc("Qual-Kehk");
                if (qual != null && qual != default(NpcEntity))
                    TalkToTrader(qual.Id);
                else
                {
                    LeaveGame();
                    return false;
                }
                byte[] three = { 0x03, 0x00, 0x00, 0x00 };
                SendPacket(0x38, three, BitConverter.GetBytes(qual.Id), GenericHandler.nulls);
                Thread.Sleep(300);
                SendPacket(0x62, BitConverter.GetBytes(qual.Id));
                Thread.Sleep(300);
                SendPacket(0x38, three, BitConverter.GetBytes(qual.Id), GenericHandler.nulls);
                Thread.Sleep(300);
                SendPacket(0x30, GenericHandler.one, BitConverter.GetBytes(qual.Id));
                Thread.Sleep(300);

                MoveTo(5060, 5076);
                MoveTo(5082, 5080);
                MoveTo(5081, 5076);
            }
            return true;
        }

        void EnterRedPortal()
        {
            Thread.Sleep(700);
            byte[] two = { 0x02, 0x00, 0x00, 0x00 };
            SendPacket(0x13, two, BitConverter.GetBytes(m_gameData.RedPortal.Id));
            Thread.Sleep(500);
        }

    }
}
