using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BattleNet.Connections
{
    class GenericHandler
    {
        static public readonly byte[] nulls = { 0x00, 0x00, 0x00, 0x00 };
        static public readonly byte[] ten = { 0x10, 0x00, 0x00, 0x00 };
        static public readonly byte[] six = { 0x06, 0x00, 0x00, 0x00 };
        static public readonly byte[] zero = { 0x00 };
        static public readonly byte[] one = { 0x01, 0x00, 0x00, 0x00 };

        static protected readonly String platform = "68XI", classic_id = "VD2D", lod_id = "PX2D";

        public delegate void PacketDispatcher(byte command, params IEnumerable<byte>[] args);
        public event PacketDispatcher BuildDispatchPacket;

        protected Connection m_connection;
        public GenericHandler(Connection conn)
        {
            m_connection = conn;
        }

        public delegate void StatusUpdaterHandler(Client.Status status);
        public event StatusUpdaterHandler UpdateStatus = delegate { };
        protected void OnUpdateStatus(Client.Status status)
        {
            UpdateStatus(status);
        }

        public virtual void ThreadFunction()
        {
        }
        
        public void BuildWritePacket(byte command, params IEnumerable<byte>[] args)
        {
            BuildDispatchPacket(command,args);
        }

    }
}
