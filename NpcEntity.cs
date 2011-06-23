using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BattleNet
{
    class NpcEntity : Entity
    {
        protected String m_name;
        public String Name { get { return m_name; } set { m_name = value; } } 

        protected UInt32 m_type;
        public UInt32 Type { get { return m_type; } set { m_type = value; } }

        protected UInt32 m_life;
        public UInt32 Life { get { return m_life; } set { m_life = value; } }

        protected Coordinate m_targetLocation;
        public Coordinate TargetLocation { get { return m_targetLocation; } set { m_targetLocation = value; } }

        protected Boolean m_moving;
        public Boolean Moving { get { return m_moving; } set { m_moving = value; } }

        protected Boolean m_running;
        public Boolean Running { get { return m_running; } set { m_running = value; } }

        protected Boolean m_hasFlags;
        public Boolean HasFlags { get { return m_hasFlags; } set { m_hasFlags = value; } }

        protected Boolean m_flag1;
        public Boolean Champion { get { return m_flag1; } set { m_flag1 = value; } }

        protected Boolean m_flag2;
        public Boolean Unique { get { return m_flag2; } set { m_flag2 = value; } }

        protected Boolean m_superUnique;
        public Boolean SuperUnique { get { return m_superUnique; } set { m_superUnique = value; } }

        protected Boolean m_isMinion;
        public Boolean IsMinion { get { return m_isMinion; } set { m_isMinion = value; } }

        protected Boolean m_flag5;
        public Boolean Ghostly { get { return m_flag5; } set { m_flag5= value; } }

        protected Boolean m_isLightning;
        public Boolean IsLightning { get { return m_isLightning; } set { m_isLightning = value; } }

        protected Int32 m_superUniqueId;
        public Int32 SuperUniqueId { get { return m_superUniqueId; }  set { m_superUniqueId = value; } }

        public NpcEntity()
        { }

        public NpcEntity(UInt32 id, UInt32 type, UInt32 life, Int32 x, Int32 y) :
            base(id,x,y)
        {
            m_type = type;
            m_life = life;
            m_hasFlags = false;
            m_moving = false;
        }

    }
}
