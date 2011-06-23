using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BattleNet.Connections.Readers;
using BattleNet.Connections.Handlers;
using System.Net;
using System.Threading;

namespace BattleNet.Connections
{
    class D2GS : IDisposable
    {
        D2gsConnection m_d2gsConnection;

        D2gsReader m_d2gsReader;
        Thread m_d2gsReaderThread;
        
        D2gsHandler m_d2gsHandler;
        Thread m_d2gsHandlerThread;

        GameServerPing m_gsPing;
        Thread m_gsPingThread;

        public D2GS(String character, String account)
        {
            m_d2gsConnection = new D2gsConnection();
            m_d2gsReader = new D2gsReader(ref m_d2gsConnection, character);
            m_d2gsHandler = new D2gsHandler(ref m_d2gsConnection);
            m_gsPing = new GameServerPing(ref m_d2gsConnection);

            m_gsPingThread = new Thread(m_gsPing.Run);
            m_gsPingThread.Name = account + " [D2GS]:";

            m_d2gsReaderThread = new Thread(m_d2gsReader.ThreadFunction);
            m_d2gsReaderThread.Name = account + " [D2GS]:";

            m_d2gsHandlerThread = new Thread(m_d2gsHandler.ThreadFunction);
            m_d2gsHandlerThread.Name = account = " [D2Gs]:";

            m_d2gsConnection.StartThread += m_d2gsHandlerThread.Start;
            m_d2gsConnection.StartThread += m_d2gsReaderThread.Start;

            m_d2gsHandler.StartPinging += m_gsPingThread.Start;
        }

        public void SubscribeNextGameEvent(D2gsReader.NoParam sub)
        {
            m_d2gsReader.NextGame += sub;
        }

        public void UpdateCharacterName(String name)
        {
            m_d2gsReader.Character = name;
        }
        public void UpdateClassByte(byte classByte)
        {
            m_d2gsReader.ClassByte = classByte;
        }

        public void SetGSInfo(List<byte> hash, List<byte> token)
        {
            m_d2gsReader.SetInfo(hash, token);
        }

        public void Init(IPAddress ip, ushort port, List<byte> data)
        {
            Logging.Logger.Write("Initializing D2GS");
            m_d2gsConnection.Init(ip, port, data);
        }

        #region IDisposable Members

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
