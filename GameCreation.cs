using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace BattleNet
{
    class GameCreation
    {
        public AutoResetEvent m_makeNextGame = new AutoResetEvent(true); 

        public void Run()
        {
            while (true)
            {

                m_makeNextGame.WaitOne();
            }
        }
    }
}
