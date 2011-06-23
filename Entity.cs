using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BattleNet
{
    class Entity
    {
        protected Boolean m_initialized;
        public Boolean Initialized { get { return m_initialized; } set { m_initialized = value; } }

        protected UInt32 m_id;
        public UInt32 Id { get { return m_id; } set { m_id = value; } }

        protected Coordinate m_location;
        public Coordinate Location { get { return m_location; } set { m_location = value; } }

        public Entity()
        {
            m_initialized = false;
            m_location = new Coordinate(0, 0);
        }

        public Entity(UInt32 id, Int32 x, Int32 y)
        {
            m_initialized = true;
            m_id = id;
            m_location = new Coordinate(x, y);
        }
    
    }
}
