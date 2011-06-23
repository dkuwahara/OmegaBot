using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BattleNet.Connections;
using System.Threading;
using BattleNet.Connections.Handlers;
using BattleNet.Connections.Readers;
using System.Net;

namespace BattleNet
{
    class Bnet : IDisposable
    {
#region Members

        //Holds the connection and stream for the BNCS
        BncsConnection m_bncsConnection;

        // Reads packets from the BNCS stream and places it in a queue for the handler
        BncsReader m_bncsReader;
        Thread m_bncsReaderThread;

        // Handles all the packets received and dispatches them appropriately
        BncsHandler m_bncsHandler;
        Thread m_bncsHandlerThread;

        //Holds the connection and stream to the MCP
        McpConnection m_mcpConnection;

        // This class pulls all MCP packets from the stream and places it
        // into a queue for the handler to use
        McpReader m_mcpReader;
        Thread m_mcpReaderThread;

        //This class handles all of the received MCP Packets
        McpHandler m_mcpHandler;
        Thread m_mcpHandlerThread;

        // Battle.Net server we are connecting to for this client
        IPAddress m_server;

#endregion

#region Initializers

        private void InitThreads(String account)
        {
            m_bncsReaderThread = new Thread(m_bncsReader.ThreadFunction);
            m_bncsReaderThread.Name = account + " [BNCS]:";

            m_bncsHandlerThread = new Thread(m_bncsHandler.ThreadFunction);
            m_bncsHandlerThread.Name = account + " [BNCS]:";

            m_mcpReaderThread = new Thread(m_mcpReader.ThreadFunction);
            m_mcpReaderThread.Name = account + " [MCP]:";

            m_mcpHandlerThread = new Thread(m_mcpHandler.ThreadFunction);
            m_mcpHandlerThread.Name = account + " [MCP]:";
        }

        private void InitServers(String character, String account, String password, 
                                 Client.GameDifficulty difficulty, String classicKey, 
                                 String expansionKey, String exeInfo)
        {
            m_bncsConnection = new BncsConnection();
            m_bncsReader = new BncsReader(ref m_bncsConnection);
            m_bncsHandler = new BncsHandler(ref m_bncsConnection, account, password,
                                            classicKey, expansionKey, exeInfo);
            m_mcpConnection = new McpConnection();
            m_mcpReader = new McpReader(ref m_mcpConnection);
            m_mcpHandler = new McpHandler(ref m_mcpConnection, character, difficulty);
        }

        private void AssociateEvents()
        {
            m_bncsConnection.StartThread += m_bncsReaderThread.Start;
            m_bncsConnection.StartThread += m_bncsHandlerThread.Start;

            m_mcpConnection.StartThread += m_mcpReaderThread.Start;
            m_mcpConnection.StartThread += m_mcpHandlerThread.Start;

            m_bncsHandler.StartMcpThread += StartMcp;
            m_bncsHandler.RealmUpdate += m_mcpHandler.UpdateRealm;

            m_mcpHandler.BuildDispatchPacket += WriteBncsPacket;
        }

#endregion

#region Constructors

        public Bnet(IPAddress server, String character, String account, String password, Client.GameDifficulty difficulty, String classicKey, String expansionKey, String exeInfo)
        {
            m_server = server;
            InitServers(character, account, password, difficulty, classicKey, expansionKey, password);
            InitThreads(account);
            AssociateEvents();
        }

#endregion

#region Events
        
        protected void WriteBncsPacket(byte command, params IEnumerable<byte>[] args)
        {
            byte[] packet = m_bncsConnection.BuildPacket(command, args);
            m_bncsConnection.Write(packet);
        }

        protected void StartMcp(IPAddress server, ushort port, List<byte> data)
        {
            m_mcpConnection.Init(server, port, data);
        }

#endregion

#region Subscribers

        public void SubscribeCharacterNameUpdate(McpHandler.CharacterUpdateDel sub)
        {
            m_mcpHandler.UpdateCharacterName += sub;
        }
        public void SubscribeGameCreationThread(BncsHandler.GameCreationThreadHandler sub)
        {
            m_bncsHandler.StartGameCreationThread += sub;

        }

        public void SubscribeClassByteUpdate(McpHandler.SetByte sub)
        {
            m_mcpHandler.SetClassByte += sub;
        }

        public void SubscribeGameServerStart(McpHandler.D2gsStarter sub)
        {
            m_mcpHandler.StartGameServer += sub;
        }

        public void SubscribeStatusUpdates(GenericDispatcher.StatusUpdaterHandler sub)
        {
            m_bncsHandler.UpdateStatus += sub;
            m_mcpHandler.UpdateStatus += sub;
        }

#endregion

        public void JoinGame(String gameName, String gamePass)
        {
            m_mcpHandler.JoinGame(gameName, gamePass);
        }

        public void MakeGame(Client.GameDifficulty difficulty, String gameName, String gamePass)
        {
            m_mcpHandler.MakeGame(difficulty, gameName, gamePass);
        }

        public void MakeRandomGame(Client.GameDifficulty difficulty)
        {
            m_mcpHandler.MakeRandomGame(difficulty);
        }


        // Connect to the BNCS and MCP
        public bool Connect()
        {
            return m_bncsConnection.Init(m_server, 6112);
        }

        #region IDisposable Members

        public void Close()
        {
            m_bncsConnection.Close();
            m_mcpConnection.Close();
        }

        void IDisposable.Dispose()
        {
            m_bncsConnection.Close();
            m_mcpConnection.Close();
        }

        #endregion
    }
}
