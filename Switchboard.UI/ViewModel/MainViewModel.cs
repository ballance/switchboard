using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using Switchboard.ConsoleHost;
using Switchboard.ConsoleHost.Logging;
using Switchboard.Server;

namespace Switchboard.UI.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private string _status;
        private string _currentCall;
        private string _testUrl;
        private const string testUrlTarget = "http://www.road.is/travel-info/road-conditions-and-weather/the-entire-country/island1e.html";

        public MainViewModel()
        {
            Status = "Loading...";
            Initialize();
        }

        public string CurrentCall
        {
            get { return _currentCall; }
            set
            {
                if (_currentCall == value)
                    return;

                _currentCall = value;
                RaisePropertyChanged();
            }
        }

        public string Status
        {
            get { return _status; }
            set
            {
                if (_status == value)
                    return;

                _status = value;
                RaisePropertyChanged();
            }
        }

        public string TestUrl
        {
            get { return _testUrl; }
            set
            {
                if (_testUrl == value)
                    return;

                _testUrl = value;
                RaisePropertyChanged();
            }
        }

        public void Initialize()
        {
            Task.Run(() =>
            {
                Status = "Loading 2...";
                RunInitialize();
            });
        }

        public async void RunInitialize()
        {
            try
            {
                var traceFilename = String.Format("outputoutput{0}.txt", DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss"));
                Trace.Listeners.Add(new TextWriterLogger(File.Create(traceFilename)));
                var textStream = new MemoryStream();
                Trace.Listeners.Add(new TextWriterLogger(textStream));

                var endPoint = new IPEndPoint(IPAddress.Loopback, 8080);
                var handler = new SimpleReverseProxyHandler(testUrlTarget);
                var server = new SwitchboardServer(endPoint, handler);

                server.Start();

                TestUrl = String.Format("Point your browser at http://{0}:{1}", endPoint.Address, endPoint.Port);
                for (int i = 0; i < 1000; i++)
                {
                    var currentMemStream = textStream.Position;
                    Status = String.Format("Current Stream position {0} at {1} .{2}", currentMemStream, System.DateTime.Now.ToLongTimeString(), System.Environment.NewLine);
                    await Task.Delay(500);
                }

            }
            catch (Exception exception)
            {
                Status = String.Format("Failed to load. {0}", exception);
            }
        }


    }
}