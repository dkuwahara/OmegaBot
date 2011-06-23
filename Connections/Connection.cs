using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;

namespace BattleNet.Connections
{
    class Connection : IDisposable
    {
        #region Constructors 
        public Connection()
        {
            m_socket = new TcpClient();

            m_packets = new PriorityQueue<uint, List<byte>>();
            m_packetsReady = new AutoResetEvent(false);
        }
        #endregion

        protected AutoResetEvent m_packetsReady;
        public AutoResetEvent PacketsReady { get { return m_packetsReady; } set { m_packetsReady = value; } }
        #region Events

        public delegate void ThreadStarter();
        public event ThreadStarter StartThread = delegate { };

        public void OnStartThread()
        {
            StartThread();
        }

        #endregion

        #region Members
        protected NetworkStream m_stream;
        public NetworkStream Stream { get { return m_stream; } set { m_stream = value; } }

        protected TcpClient m_socket;
        public TcpClient Socket { get { return m_socket; } set { m_socket = value; } }

        protected PriorityQueue<UInt32,List<byte>> m_packets;
        public PriorityQueue<UInt32, List<byte>> Packets { get { return m_packets; } set { m_packets = value; } }

        #endregion

        #region Methods
        public virtual bool Init(IPAddress server, ushort port, List<byte> data = null)
        {
            return false;
        }

        public virtual void Kill()
        {
            m_stream.Close();
            m_socket.Close();
        }

        public void Close()
        {
            Kill();
        }

        public virtual bool GetPacket(ref List<byte> buffer, ref List<byte> data)
        {
            return false;
        }

        public virtual void Write(byte[] packet)
        {
            if (m_socket.Connected)
            {
                try
                {
                    m_stream.Write(packet, 0, packet.Length);
                }
                catch
                {
                    Kill();
                }
            }
        }

        public virtual byte[] BuildPacket(byte command, params IEnumerable<byte>[] args)
        {
            return null;
        }
        #endregion

        #region IDisposable Members

        void IDisposable.Dispose()
        {
            m_stream.Close();
            m_socket.Close();
        }

        #endregion
    }
}
