using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;

namespace BattleNet.Logging
{
    enum Level
    {
        LOG_ONLY,
        CONSOLE,
        IRC,
    }
    class Logger
    {
        public static bool logToConsole = true;

        public static void InitTrace()
        {
            //IRC.Bot.Instance().Init("irc.synirc.net", 6667, "NovaD2Bot", "#d2bs");
            //IRC.Bot.Instance().Connect();
            TextWriterTraceListener myWriter = new TextWriterTraceListener(System.Console.Out);
            //Trace.Listeners.Add(myWriter);
            Trace.Listeners.Add(new TextWriterTraceListener(DateTime.Now.ToString("ddMMHHmmss") + ".log" ));
            Trace.AutoFlush = true;
        }

        public static void Write(String str)
        {
            String stamp = DateTime.Now.ToString("[dd/MM HH:mm:ss]");
            String output = stamp + " " + Thread.CurrentThread.Name + " " + str;
            if (logToConsole)
            {
                lock (Console.Out)
                {
                    Console.WriteLine(output);
                }
            }
            Trace.WriteLine(output);
        }

        public static void Write(String str, params Object[] args)
        {
            String stamp = DateTime.Now.ToString("[dd/MM HH:mm:ss]");
            String output = stamp + " " + Thread.CurrentThread.Name + " " + String.Format(str, args);
            if (logToConsole)
                Console.WriteLine(output);
            Trace.WriteLine(output);
        }

    }
}
