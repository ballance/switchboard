using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Switchboard.Common.Logging;
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
        //private const string testUrlTarget = "http://www.nytimes.com";
        private CancellationTokenSource tokenSource;
        public RelayCommand StartCommand { get; private set; }
        public RelayCommand StopCommand { get; private set; }
        private bool _running;
        private object _synco = new object();
        private long _currentTransferredBytes = 0;


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

        public long CurrentTransferredBytes
        {
            get { return _currentTransferredBytes; }
            set
            {
                if (_currentTransferredBytes == value)
                    return;

                _currentTransferredBytes = value;
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
                var traceFilename = String.Format("logs\\outputoutput{0}.txt", DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss"));
                Trace.Listeners.Add(new TextWriterLogger(File.Create(traceFilename),_synco));
                var textStream = new MemoryStream();
                Trace.Listeners.Add(new TextWriterLogger(textStream, _synco));

                var endPoint = new IPEndPoint(IPAddress.Loopback, 8080);
                var handler = new SimpleReverseProxyHandler(ConfigurationManager.AppSettings.Get("RemoteUrl"));
                var server = new SwitchboardServer(endPoint, handler);

                var lastStreamLength = -0L;

                server.Start();
                //long lastPosition = -1;
                var previousPosition = textStream.Position;
                TestUrl = String.Format("Intercepting at http://{0}:{1}", endPoint.Address, endPoint.Port);
                while (true)
                {
                    lock (_synco)
                    {

                        if (textStream.Position > previousPosition)
                        {
                            textStream.Position = 0L;
                            var sr = new StreamReader(textStream); //new StreamReader(copyStream);
                        
                            var currentMemStream = sr.ReadToEnd();

                            if (currentMemStream.Length > lastStreamLength)
                            {
                                lastStreamLength = textStream.Position;
                                Status = currentMemStream;
                                CurrentTransferredBytes = lastStreamLength;
                                //String.Format("{0}: {1}.{2}", currentMemStream, System.DateTime.Now.ToLongTimeString(), System.Environment.NewLine);
                            }

                            textStream.Position = previousPosition;
                        }
                        if (token.IsCancellationRequested)
                        {
                            server.Stop();
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