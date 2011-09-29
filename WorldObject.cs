using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BattleNet
{
    class WorldObject : Entity
    {
        protected UInt16 m_type;
        public UInt16 Type { get { return m_type; } set { m_type = value; } }

        public WorldObject()
        { }

        public WorldObject(UInt32 id, UInt16 type, UInt16 x, UInt16 y) :
            base(id, x, y)
        {
            m_type = type;
        }
    }
}
