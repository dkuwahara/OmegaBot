using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BattleNet.Connections.Readers;
using System.Threading;
using BattleNet.Connections.Handlers;
using System.Net;

namespace BattleNet.Connections
{ 
    class BncsConnection : Connection
    {
        static readonly byte[] AuthInfoPacket =
	    {
		    0xff, 0x50, 0x3a, 0x00, 0x00, 0x00, 0x00, 0x00,
		    0x36, 0x38, 0x58, 0x49, 0x50, 0x58, 0x32, 0x44,
		    0x0d, 0x00, 0x00, 0x00, 0x53, 0x55, 0x6e, 0x65,
		    0x55, 0xb4, 0x47, 0x40, 0x88, 0xff, 0xff, 0xff,
		    0x09, 0x04, 0x00, 0x00, 0x09, 0x04, 0x00, 0x00,
		    0x55, 0x53, 0x41, 0x00, 0x55, 0x6e, 0x69, 0x74,
		    0x65, 0x64, 0x20, 0x53, 0x74, 0x61, 0x74, 0x65,
		    0x73, 0x00
	    };

        public override bool Init(IPAddress server, ushort port, List<byte> data = null)
        {
            //m_owner.GameRequestId = 0x02;
            //m_owner.InGame = false;
            Logging.Logger.Write("[BNCS] Connecting to {0}:{1}", server, port);
            // Establish connection
            m_socket.Connect(server, port);
            m_stream = m_socket.GetStream();
            if (!m_stream.CanWrite)
            {
                Logging.Logger.Write("Failed To connect to {0}:{1}", server,port);
                return false;
            }

            m_stream.WriteByte(0x01);
            m_stream.Write(AuthInfoPacket, 0, AuthInfoPacket.Length);
            OnStartThread();
            return true;
        }

        public override bool GetPacket(ref List<byte> bncsBuffer, ref List<byte> data)
        {
            while (bncsBuffer.Count < 4)
            {
                try
                {
                    byte temp = 0;

                    temp = (byte)m_stream.ReadByte();
                    bncsBuffer.Add(temp);
                }
                catch
                {
                    //Console.WriteLine("\n{0}: [BNCS] Disconnected From BNCS", m_owner.Account);
                    Kill();
                    return false;
                }
            }

            byte[] bytes = new byte[bncsBuffer.Count];
            bncsBuffer.CopyTo(bytes);

            short packetLength = BitConverter.ToInt16(bytes, 2);

            while (packetLength > bncsBuffer.Count)
            {
                try
                {
                    byte temp = (byte)m_stream.ReadByte();
                    bncsBuffer.Add(temp);
                }
                catch
                {
                    Kill();
                    return false;
                }
            }
            data = new List<byte>(bncsBuffer.GetRange(0, packetLength));
            bncsBuffer.RemoveRange(0, packetLength);
            return true;
        }

        public override byte[] BuildPacket(byte command, params IEnumerable<byte>[] args)
        {
            List<byte> packet = new List<byte>();
            packet.Add((byte)0xFF);
            packet.Add((byte)command);
            List<byte> packetArray = new List<byte>();
            foreach (IEnumerable<byte> a in args)
            {
                packetArray.AddRange(a);
            }

            UInt16 arrayCount = (UInt16)(packetArray.Count + 4);
            packet.AddRange(BitConverter.GetBytes(arrayCount));

            packet.AddRange(packetArray);

            return packet.ToArray();
        }

        public BncsConnection()
            : base()
        {
        }
    }
}
