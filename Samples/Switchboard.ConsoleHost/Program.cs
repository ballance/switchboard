using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using Switchboard.ConsoleHost.Logging;
using Switchboard.Server;

namespace Switchboard.ConsoleHost
{
    class Program
    {
        static void Main(string[] args)
        {
            var traceFilename = String.Format("outputoutput{0}.txt", DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss"));
            Trace.Listeners.Add(new TextWriterLogger(File.Create(traceFilename), new object()));

            // Dump all debug data to the console, coloring it if possible
            Trace.Listeners.Add(new ConsoleLogger());

            var endPoint = new IPEndPoint(IPAddress.Loopback, 8080);
            var handler = new SimpleReverseProxyHandler("http://www.road.is/travel-info/road-conditions-and-weather/the-entire-country/island1e.html");
            var server = new SwitchboardServer(endPoint, handler);

            server.Start();

            Console.WriteLine("Point your browser at http://{0}", endPoint);

            Console.ReadLine();
        }
    }


}
