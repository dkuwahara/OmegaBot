using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BattleNet.Connections
{
    class GenericDispatcher : GenericHandler
    {
        public GenericDispatcher(Connection conn)
            : base(conn)
        {
        }

        public delegate void PacketHandler(byte type, List<byte> data);

        public virtual PacketHandler DispatchPacket(byte type)
        {
            return null;
        }
    }
}
