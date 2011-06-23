using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BattleNet
{
    class Coordinate
    {
        protected Int32 m_x;
        public Int32 X { get { return m_x; } set { m_x = value; } }

        protected Int32 m_y;
        public Int32 Y { get { return m_y; } set { m_y = value; }  }

        public Coordinate()
        {

        }

        public Coordinate(Int32 x, Int32 y)
        {
            m_x = x;
            m_y = y;
        }

        public override bool Equals(object obj)
        {
            return this == (Coordinate)obj;
        }

        public override int GetHashCode()
        {
            return m_x^m_y;
        }

        public static bool operator==(Coordinate first, Coordinate second)
        {
            return (first.X == second.X) && (first.Y == second.Y);
        }

        public static bool operator!=(Coordinate first, Coordinate second)
        {
            return !(first == second);
        }

        public Double Distance(Coordinate other)
        {
            Double x2 = Math.Pow(m_x - other.X, 2.0);
            Double y2 = Math.Pow(m_y - other.Y, 2.0);
            Double distance = Math.Sqrt(x2 + y2);
            return distance;
        }

    }
}
