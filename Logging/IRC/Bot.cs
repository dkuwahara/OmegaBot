using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;

namespace BattleNet.Logging.IRC
{
    class Bot
    {
        protected String m_server;
        protected UInt16 m_port;
        protected String m_user;
        protected String m_nickname;
        protected String m_channel;

        protected NetworkStream m_stream;
        protected TcpClient m_socket;

        protected StreamWriter m_writer;
        public StreamWriter Writer { get { return m_writer; } }

        public void Init(String server, UInt16 port, String nickname, String channel)
        {
            m_server = server;
            m_port = port;
            m_nickname = nickname;
            m_channel = channel;
            m_user = "USER D2Bot" + m_nickname + " 8 * :Clientless Bot outputter";
        }

        public void Connect()
        {
            m_socket = new TcpClient(m_server, m_port);
            Thread pingThread = new Thread(PingThread);
            m_stream = m_socket.GetStream();
            m_writer = new StreamWriter(m_stream);
            m_writer.AutoFlush = true;
            pingThread.Start();
            m_writer.WriteLine(m_user);
            m_writer.Flush();
            m_writer.WriteLine("NICK " + m_nickname); m_writer.Flush();
            m_writer.WriteLine("JOIN " + m_channel);
            m_writer.Flush();
            Thread.Sleep(10000);
            Console.WriteLine("Connected to IRC");
        }

        public void Write(String str)
        {
            m_writer.WriteLine("PRIVMSG " + m_channel + " " +  str);
        }

        private Bot()
        {
        }

        private static Bot m_bot = new Bot();

        public static Bot Instance()
        {
           return m_bot;
        }

        private void PingThread()
        {
            while (m_socket.Connected)
            {
                Writer.WriteLine("PING " + m_server);
                m_writer.Flush();
                Thread.Sleep(15000);
            }
        }

    }
}
