using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;

namespace BattleNet.Logging
{
    class Logger
    {
        public static void InitTrace()
        {
            TextWriterTraceListener myWriter = new TextWriterTraceListener(System.Console.Out);
            Trace.Listeners.Add(myWriter);
        }

        public static void Write(String str)
        {
            String stamp = DateTime.Now.ToString("[dd/MM HH:mm:ss]");
            Trace.WriteLine(stamp + " " + Thread.CurrentThread.Name + " " + str);
        }

        public static void Write(String str, params Object[] args)
        {
            String stamp = DateTime.Now.ToString("[dd/MM HH:mm:ss]");
            String output = String.Format(str, args);
            Trace.WriteLine(stamp + " " + Thread.CurrentThread.Name + " " + output);
        }
    }
}
