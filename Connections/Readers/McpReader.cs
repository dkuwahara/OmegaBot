using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BattleNet.Logging;

namespace BattleNet.Connections.Readers
{
    class McpReader : GenericHandler
    {
        public McpReader(ref McpConnection conn)
            : base(conn)
        {
        }

        public override void ThreadFunction()
        {
            Logger.Write("MCP Reader started!");
            List<byte> data = new List<byte>();
            List<byte> mcpBuffer = new List<byte>();
            while (m_connection.Socket.Connected)
            {
                if (!m_connection.GetPacket(ref mcpBuffer, ref  data))
                {
                    break;
                }
                
                lock (m_connection.Packets)
                {
                    m_connection.Packets.Enqueue(1, data);
                }
                m_connection.PacketsReady.Set();
            }
        }
    }
}
