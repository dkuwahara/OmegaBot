using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BattleNet.Connections;
using System.Threading;

namespace BattleNet
{
    class MapPoint
    {
        public Char Character;
        public ConsoleColor Color;
        public MapPoint(Char chara, ConsoleColor color)
        {
            Character = chara;
            Color = color;
        }

        public static implicit operator MapPoint(Char chara)
        {
            return new MapPoint(chara, ConsoleColor.White);
        }
    }
    class AsciiMap
    {
        
        private MapPoint[,] m_map;
        private static UInt16 width = 80;
        private static UInt16 height = 80;
        private GameData m_gameData;
        D2gsConnection m_connection;
        private UInt16 m_x;
        private UInt16 m_y;
        public AsciiMap(GameData gameData, D2gsConnection connection)
        {
            m_gameData = gameData;
            m_connection = connection;

            m_map = new MapPoint[width,height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    m_map[x, y] = ' ';
                }
            }
        }

        public void ThreadFunction()
        {
            Logging.Logger.logToConsole = false;
            while (m_connection.Socket.Connected)
            {
                PopulateMap();
                DrawScreen();
                Thread.Sleep(50);
            }
            Logging.Logger.logToConsole = true;
        }

        public void PopulateMap()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    m_map[x, y] = ' ';
                }
            }

            m_x = (m_gameData.Me.Location.X);
            m_y = (m_gameData.Me.Location.Y);


            foreach (NpcEntity npc in m_gameData.Npcs.Values)
            {
                if (Math.Abs(npc.Location.X - m_x) < 40)
                {
                    if (Math.Abs(npc.Location.Y - m_y) < 40)
                    {
                        if(npc.SuperUnique && npc.Life > 0)
                            m_map[40 + npc.Location.X - m_x, 40 + npc.Location.Y - m_y] 
                                = new MapPoint('S', ConsoleColor.DarkRed);
                        else if (npc.IsMinion && npc.Life > 0)
                            m_map[40 + npc.Location.X - m_x, 40 + npc.Location.Y - m_y] 
                                = new MapPoint('M',ConsoleColor.DarkRed);
                        else if (npc.Life > 0)
                            m_map[40 + npc.Location.X - m_x, 40 + npc.Location.Y - m_y] 
                                = new MapPoint('m', ConsoleColor.DarkYellow);
                        else
                            m_map[40 + npc.Location.X - m_x, 40 + npc.Location.Y - m_y] 
                                = '.';
                    }
                }
            }
            foreach (WorldObject obj in m_gameData.WorldObjects.Values)
            {
                int x = (obj.Location.X - m_x);
                int y = (obj.Location.Y - m_y);
                if (Math.Abs(obj.Location.X - m_x) < 40)
                {
                    if (Math.Abs(obj.Location.Y - m_y) < 40)
                    {
                        if (obj.Type == 0x01ad)
                        {
                            m_map[40 + x, 40 + y] = new MapPoint('W', ConsoleColor.Blue);
                        }
                        else
                        {
                            m_map[40 + x, 40 + y] = new MapPoint('*', ConsoleColor.DarkGray);
                        }
                    }
                }
            }
            if (m_gameData.RedPortal != null)
            {
                if (Math.Abs(m_gameData.RedPortal.Location.X - m_x) < 40
                    && Math.Abs(m_gameData.RedPortal.Location.Y - m_y) < 40)
                {
                    m_map[40 + m_gameData.RedPortal.Location.X - m_x,
                          40 + m_gameData.RedPortal.Location.Y - m_y] = new MapPoint('R', ConsoleColor.Red);
                }
            }

            AddNpc("Malah", '♥');
            AddNpc("Qual-Kehk", 'Q');
            AddNpc("Anya", 'A');
            m_map[40, 50] = new MapPoint('@',ConsoleColor.White);
        }

        public void AddNpc(String name, Char symbol)
        {
            NpcEntity npc = GetNpc(name);

            if (npc != null && npc.Initialized && Math.Abs(npc.Location.X - m_x) < 40
                    && Math.Abs(npc.Location.Y - m_y) < 40)
            {
                m_map[40 + npc.Location.X - m_x,
                          40 + npc.Location.Y - m_y] = new MapPoint(symbol, ConsoleColor.Green);
            }
        }

        public NpcEntity GetNpc(String name)
        {
            NpcEntity npc = (from n in m_gameData.Npcs
                             where n.Value.Name == name
                             select n).FirstOrDefault().Value;
            return npc;
        }

        public void DrawScreen()
        {
            Console.SetCursorPosition(0, 0);
            for (int y = 0; y < width; y++)
            {
                for (int x = 0; x < height; x++)
                {
                    lock (Console.Out)
                    {
                        Console.ForegroundColor = m_map[x, y].Color;
                        Console.SetCursorPosition(x, y);
                        Console.Write("{0}", m_map[x, y].Character);
                        Console.SetCursorPosition(0, 82);
                        Console.ResetColor();
                    }
                }
            }
        }
    }
}
