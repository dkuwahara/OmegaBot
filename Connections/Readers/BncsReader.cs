using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BattleNet.Connections.Readers
{
    class BncsReader : GenericHandler
    {
        public BncsReader(ref BncsConnection conn)
            : base(conn)
        {

        }

        public override void ThreadFunction()
        {
            List<byte> bncsBuffer = new List<byte>();
            List<byte> data = new List<byte>();
            while (m_connection.Socket.Connected)
            {
                if(!m_connection.GetPacket(ref bncsBuffer,ref data))
                {
                    break;
                }
                lock (m_connection.Packets)
                {
                    m_connection.Packets.Enqueue(1, data);
                    m_connection.PacketsReady.Set();
                }
            }
        }
    }
}
