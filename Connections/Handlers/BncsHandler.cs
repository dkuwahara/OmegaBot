using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace BattleNet.Connections.Handlers
{
    class BncsHandler : GenericDispatcher
    {
        private String m_classicKey;
        private String m_expansionKey;
        private String m_exeInfo;
        private String m_account;
        private String m_password;
        private String m_realm;

        protected UInt32 m_serverToken;

        public BncsHandler(ref BncsConnection conn, String account, String password, String classicKey, String expansionKey, String exeInfo)
            : base(conn)
        {
            m_account = account;
            m_password = password;
            m_realm = null;
            m_classicKey = classicKey;
            m_expansionKey = expansionKey;
            m_exeInfo = exeInfo;
        }

        public delegate void StringUpdater(String str);
        public event StringUpdater RealmUpdate = delegate { };

        public void OnRealmUpdate(String realm)
        {
            m_realm = realm;
            RealmUpdate(realm);
        }

        public delegate void McpStartHandler(IPAddress ip, ushort port, List<byte> data);
        public event McpStartHandler StartMcpThread = delegate { };

        public delegate void GameCreationThreadHandler();
        public event GameCreationThreadHandler StartGameCreationThread = delegate { };

        protected void OnStartGameCreationThread()
        {
            Logging.Logger.Write("Starting game creation Thread");
            StartGameCreationThread();
        }

        protected void OnStartMcpThread(IPAddress ip, ushort port, List<byte> data)
        {
            //Logging.Logger.Write("Starting the MCP");
            StartMcpThread(ip, port, data);
        }

        public override void ThreadFunction()
        {
            while (m_connection.Socket.Connected)
            {
                if (m_connection.Packets.IsEmpty())
                {
                    m_connection.PacketsReady.WaitOne();
                }
                else
                {
                    List<byte> packet;
                    lock (m_connection.Packets)
                    {
                        packet = m_connection.Packets.Dequeue();

                    }
                    byte type = packet[1];
                    DispatchPacket(type)(type, packet);
                }
            }
        }

        public override PacketHandler DispatchPacket(byte type)
        {
            switch (type)
            {
                // Cases in order that they should be received
                case 0x00:
                case 0x25:  return PingRequest;
                case 0x50:  return AuthInfoRequest;
                case 0x51:  return AuthCheck;
                case 0x33:  return AccountLogin;
                case 0x3a:  return LoginResult;
                case 0x40:  return RealmList;
                case 0x3e:  return StartMcp;
                case 0x0a:  return EnterChat;
                case 0x15:  return HandleAdvertising;
                default:    return VoidRequest;
            }
        }

        protected void PingRequest(byte type, List<byte> data)
        {
            m_connection.Write(data.ToArray());
        }

        protected void AuthInfoRequest(byte type, List<byte> data)
        {
            m_serverToken = BitConverter.ToUInt32(data.ToArray(), 8);
            List<byte> temp = data.GetRange(16, 8);

            String mpq_file_time = System.Text.Encoding.ASCII.GetString(temp.ToArray());

            int offset;
            if (data[24] == 0x76)
                offset = 24;
            else
                offset = 24;

            String mpq_file_name = ReadNullTerminatedString(System.Text.Encoding.ASCII.GetString(data.ToArray()), ref offset);
            String formula_string = ReadNullTerminatedString(System.Text.Encoding.ASCII.GetString(data.ToArray()), ref offset);

            /*
             * Download MPQ would go here.
             */

            uint exe_checksum = AdvancedCheckRevision.FastComputeHash(formula_string, mpq_file_name,
                                                           System.IO.Path.Combine(Globals.BinaryDirectory, "Game.exe"),
                                                           System.IO.Path.Combine(Globals.BinaryDirectory, "Bnclient.dll"),
                                                           System.IO.Path.Combine(Globals.BinaryDirectory, "D2Client.dll"));

            uint client_token = (uint)System.Environment.TickCount;

            List<byte> classic_hash = new List<byte>(), lod_hash = new List<byte>(), classic_public = new List<byte>(), lod_public = new List<byte>();

            if(!CdKey.GetD2KeyHash(m_classicKey, ref  client_token, m_serverToken, ref classic_hash, ref classic_public))
                OnUpdateStatus(Client.Status.STATUS_INVALID_CD_KEY);
            if(!CdKey.GetD2KeyHash(m_expansionKey, ref client_token, m_serverToken, ref  lod_hash, ref lod_public))
                OnUpdateStatus(Client.Status.STATUS_INVALID_EXP_CD_KEY);

            byte[] packet = m_connection.BuildPacket((byte)0x51, BitConverter.GetBytes(client_token), BitConverter.GetBytes(0x01000001), BitConverter.GetBytes(exe_checksum),
                            BitConverter.GetBytes(0x00000002), nulls, ten, six, classic_public, nulls, classic_hash, ten, BitConverter.GetBytes((UInt32)10),
                            lod_public, nulls, lod_hash, System.Text.Encoding.UTF8.GetBytes(m_exeInfo), zero, System.Text.Encoding.ASCII.GetBytes("OmegaBot"), zero);
            
            m_connection.Write(packet);
        }

        protected void AuthCheck(byte type, List<byte> data)
        {
            byte[] packet = data.ToArray();
            ulong result = BitConverter.ToUInt32(packet, 4);
            String info = BitConverter.ToString(packet, 8);
            handleAuthCheckResult(result, info);
        }

        protected bool handleAuthCheckResult(ulong result, string info)
        {
            switch (result)
            {
                case 0x000: break; 
                case 0x100: break;
                case 0x101: break;
                case 0x102: break;
                case 0x200: OnUpdateStatus(Client.Status.STATUS_INVALID_CD_KEY); break;
                case 0x210: OnUpdateStatus(Client.Status.STATUS_INVALID_EXP_CD_KEY); break;
                case 0x201: OnUpdateStatus(Client.Status.STATUS_KEY_IN_USE); break;
                case 0x211: OnUpdateStatus(Client.Status.STATUS_EXP_KEY_IN_USE); break;
                case 0x202: OnUpdateStatus(Client.Status.STATUS_BANNED_CD_KEY); break;
                case 0x212: OnUpdateStatus(Client.Status.STATUS_BANNED_EXP_CD_KEY); break;
                case 0x203: break;
                default: break;
            }
            if (result == 0)
            {
                byte[] packet = m_connection.BuildPacket((byte)0x33, BitConverter.GetBytes(0x80000004), nulls, System.Text.Encoding.UTF8.GetBytes("bnserver-D2DV.ini"), zero);
                m_connection.Write(packet);
                return true;
            }
            return false;
        }

        protected void AccountLogin(byte type, List<byte> data)
        {
            UInt32 client_token = (uint)System.Environment.TickCount;
            List<byte> hash = Bsha1.DoubleHash(client_token, m_serverToken, m_password);

            byte[] packet = m_connection.BuildPacket((byte)0x3a, BitConverter.GetBytes(client_token), BitConverter.GetBytes(m_serverToken), hash, System.Text.Encoding.ASCII.GetBytes(m_account), zero);
            m_connection.Write(packet);
        }

        protected void LoginResult(byte type, List<byte> data)
        {
            UInt32 result = BitConverter.ToUInt32(data.ToArray(), 4);
            switch (result)
            {
                case 0x00:
                    Logging.Logger.Write("[BNCS] Successfully logged into the account ");
                    break;
                case 0x01:
                    Logging.Logger.Write("[BNCS] Account does not exist");
                    break;
                case 0x02:
                    Logging.Logger.Write("[BNCS] Invalid password specified");
                    break;
                case 0x06:
                    Logging.Logger.Write("[BNCS] Account has been closed");
                    break;
                default:
                    Logging.Logger.Write("[BNCS] Unknown login error ");
                    break;
            }
            if (result == 0)
            {
                byte[] packet = { 0xFF, 0x40, 0x04, 0x00 };
                m_connection.Write(packet);
            }
            else
                OnUpdateStatus(Client.Status.STATUS_LOGIN_ERROR);
        }

        protected void RealmList(byte type, List<byte> data)
        {
            UInt32 count = BitConverter.ToUInt32(data.ToArray(), 8);
            Int32 offset = 12;

            for (ulong i = 1; i <= count; i++)
            {
                offset += 4;
                String realmTitle = ReadNullTerminatedString(System.Text.Encoding.ASCII.GetString(data.ToArray()), ref offset);
                String realmDescription = ReadNullTerminatedString(System.Text.Encoding.ASCII.GetString(data.ToArray()), ref offset);
                
                if (m_realm == null && i == 1)
                {
                    OnRealmUpdate(realmTitle);
                    //Logging.Logger.Write("Logging on to realm {0}", realmTitle);
                }
            }

            UInt32 clientToken = 1;
            byte[] packet = m_connection.BuildPacket((byte)0x3e, BitConverter.GetBytes(clientToken), Bsha1.DoubleHash(clientToken, m_serverToken, "password"), System.Text.Encoding.ASCII.GetBytes(m_realm), zero);
            m_connection.Write(packet);
        }

        protected void StartMcp(byte type, List<byte> data)
        {
            if (data.Count <= 12)
            {
                OnUpdateStatus(Client.Status.STATUS_MCP_LOGON_FAIL);
                return;
            }

            UInt32 ip = (uint)IPAddress.NetworkToHostOrder((int)BitConverter.ToUInt32(data.ToArray(), 20));
            ushort mcpPort = Utils.ReverseBytes(BitConverter.ToUInt16(data.ToArray(), 24));
            IPAddress mcpIp = IPAddress.Parse(ip.ToString());

            Int32 offset = 28;
            List<byte> mcpData = new List<byte>(data.GetRange(4, 16));
            mcpData.AddRange(data.GetRange(offset, data.Count - offset));

            OnStartMcpThread(mcpIp, mcpPort, mcpData);
        }

        protected void EnterChat(byte type, List<byte> data)
        {
            m_connection.Write(m_connection.BuildPacket(0x46, nulls));
            m_connection.Write(m_connection.BuildPacket(0x15, System.Text.Encoding.ASCII.GetBytes(platform), System.Text.Encoding.ASCII.GetBytes(lod_id), nulls, BitConverter.GetBytes((uint)System.Environment.TickCount)));

            OnStartGameCreationThread();
        }

        protected void HandleAdvertising(byte type, List<byte> data)
        {
            UInt32 ad_id = BitConverter.ToUInt32(data.ToArray(), 4);
            byte[] packet = m_connection.BuildPacket((byte)0x21, System.Text.Encoding.ASCII.GetBytes(platform), System.Text.Encoding.ASCII.GetBytes(lod_id), BitConverter.GetBytes(ad_id), zero, zero);
            m_connection.Write(packet);
        }

        protected void VoidRequest(byte type, List<byte> data)
        { }

        public static string ReadNullTerminatedString(string packet, ref int offset)
        {
            int zero = packet.IndexOf('\0', offset);
            string output;
            if (zero == -1)
            {
                zero = packet.Length;
                output = packet.Substring(offset, zero - offset);
                offset = 0;
            }
            else
            {
                output = packet.Substring(offset, zero - offset);
                offset = zero + 1;
            }
            return output;
        }
    }
}
