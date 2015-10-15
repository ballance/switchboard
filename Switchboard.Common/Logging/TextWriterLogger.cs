using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace Switchboard.Common.Logging
{
    public class TextWriterLogger : TextWriterTraceListener
    {
        private readonly object syncRoot;// = new object();
        private static Regex ipPortRe = new Regex(@"(?:[0-9]{1,3}\.){3}[0-9]{1,3}:\d{1,5}");
        private static TextWriter output = Console.Out;

        private Stream _outputTarget;
        public TextWriterLogger(Stream outputStream, object synco) : base(outputStream)
        {
            syncRoot = synco;
        }

        public override void Write(string message)
        {
            lock (syncRoot)
            {
                base.Write(message);
            }
        }

        public override void WriteLine(string message)
        {

            var m = ipPortRe.Match(message);

            if (m.Success)
            {

                lock (syncRoot)
                {
                    base.WriteLine(message);
                }
                base.Flush();
                return;
            }

            lock (syncRoot)
            {
                base.WriteLine(message);
            }
        }
    }
}