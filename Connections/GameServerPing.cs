using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace BattleNet.Connections
{
    class GameServerPing
    {
        D2gsConnection m_connection;

        public GameServerPing(ref D2gsConnection conn)
        {
            m_connection = conn;
        }

        public void Run()
        {
            while (m_connection.Socket.Connected)
            {
                List<byte> packet = new List<byte>();
                packet.Add(0x6d);
                packet.AddRange(BitConverter.GetBytes((uint)System.Environment.TickCount));
                packet.AddRange(GenericHandler.nulls);
                packet.AddRange(GenericHandler.nulls);
                m_connection.Write(packet.ToArray());

                int sleepStep = 100;
                for (int i = 0; i < 5000 && m_connection.Socket.Connected; i += sleepStep)
                {
                    Thread.Sleep(sleepStep);
                }
            }
        }
    }
}
