using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
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
        //private const string testUrlTarget = "http://www.road.is/travel-info/road-conditions-and-weather/the-entire-country/island1e.html";
        private const string testUrlTarget = "https://localhost:61000";
        private CancellationTokenSource tokenSource;
        public RelayCommand StartCommand { get; private set; }
        public RelayCommand StopCommand { get; private set; }
        private bool _running;
        private object _synco = new object();


        public MainViewModel()
        {
            Status = "Loading...";
            this.StartCommand = new RelayCommand(Start);
            this.StopCommand = new RelayCommand(Stop);
            _running = false;

        }

        private void Stop()
        {
            tokenSource.Cancel();
            _running = false;
            TestUrl = string.Empty;

            RaisePropertyChanged("Running");
        }

        public bool Running
        {
            get { return _running; }
            set
            {
                if (_running == value)
                    return;
                _running = value;
            }
        }

        private void Start()
        {
            tokenSource = new CancellationTokenSource();
            Initialize(tokenSource.Token);
            _running = true;
            RaisePropertyChanged("Running");
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

        public void Initialize(CancellationToken token)
        {
            Task.Run(() =>
            {
                Status = "Ready.";
                RunInitialize(token);
            }, token);
        }

        public async void RunInitialize(CancellationToken token)
        {
            try
            {
                var traceFilename = String.Format("outputoutput{0}.txt", DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss"));
                Trace.Listeners.Add(new TextWriterLogger(File.Create(traceFilename),_synco));
                var textStream = new MemoryStream();
                Trace.Listeners.Add(new TextWriterLogger(textStream, _synco));

                var endPoint = new IPEndPoint(IPAddress.Loopback, 61009);
                var handler = new SimpleReverseProxyHandler(testUrlTarget);
                var server = new SwitchboardServer(endPoint, handler);

                var lastStreamLength = -0L;

                server.Start();
                //long lastPosition = -1;
                TestUrl = String.Format("Point your browser at http://{0}:{1}", endPoint.Address, endPoint.Port);
                while (true)
                {
                    lock (_synco)
                    {
                        textStream.Position = 0L;
                        //if (textStream.Position > lastPosition)
                        //{
                        var sr = new StreamReader(textStream); //new StreamReader(copyStream);
                        

                            var currentMemStream = sr.ReadToEnd();

                            if (currentMemStream.Length > lastStreamLength)
                            {
                                lastStreamLength = textStream.Position;
                                Status = currentMemStream;
                                //String.Format("{0}: {1}.{2}", currentMemStream, System.DateTime.Now.ToLongTimeString(), System.Environment.NewLine);
                            }
                        //}
                        if (token.IsCancellationRequested)
                        {
                            server.Start();
                            break;
                        }
                    }
                    await Task.Delay(100);
                }
            }

            catch (Exception exception)
            {
                Status = String.Format("Failed to load. {0}", exception);
            }
        }

        public static void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[32768];
            int read;
            while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, read);
            }
        }
    }
}