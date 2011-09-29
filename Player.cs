using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BattleNet
{
    class Player : Entity
    {
        protected String m_name;
        public String Name { get { return m_name; } set { m_name = value; } }

        protected Boolean m_hasMercenary;
        public Boolean HasMecenary { get { return m_hasMercenary; }  set { m_hasMercenary = value; } }

        protected Boolean m_directoryKnown;
        public Boolean DirectoryKnown { get { return m_directoryKnown; } set { m_directoryKnown = value; } }

        protected UInt32 m_mercenaryId;
        public UInt32 MercenaryId { get { return m_mercenaryId; } set  { m_hasMercenary = true; m_mercenaryId = value; } }

        protected Globals.CharacterClassType m_class;
        public Globals.CharacterClassType Class { get { return m_class; } set { m_class = value; } }

        protected UInt32 m_level;
        public UInt32 Level { get { return m_level; } set { m_level = value; } }

        protected UInt32 m_portalId;
        public UInt32 PortalId { get { return m_portalId; } set { m_portalId = value; } }

        public Player()
        {
            m_directoryKnown = false;
        }

        public Player(String name, UInt32 id, Globals.CharacterClassType class_, UInt32 level)
            : base(id, 0, 0)
        {
            m_name = name;
            m_class = class_;
            m_level = level;
            m_directoryKnown = false;
        }

        public Player(String name, UInt32 id, Globals.CharacterClassType class_, UInt32 level, UInt16 x, UInt16 y) 
            : base(id,x,y)
        {
            m_name = name;
            m_class = class_;
            m_level = level;
            m_directoryKnown = true;
        }


    }
}
