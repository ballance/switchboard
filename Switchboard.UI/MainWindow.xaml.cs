using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Switchboard.ConsoleHost;
using Switchboard.ConsoleHost.Logging;
using Switchboard.Server;

namespace Switchboard.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Startup();
        }

        public void Startup()
        {
            StatusLabel.Text = "Ready.";

            try
            {
                Trace.Listeners.Add(new ConsoleLogger());

                var endPoint = new IPEndPoint(IPAddress.Loopback, 8080);
                var handler = new SimpleReverseProxyHandler("http://google.com");
                var server = new SwitchboardServer(endPoint, handler);

                server.Start();

                StatusDisplay.Text = String.Format("Point your browser at http://{0}:{1}", endPoint.Address, endPoint.Port);

                //            Console.ReadLine();

            }
            catch (Exception exception)
            {
                StatusLabel.Text = String.Format("Failed to load. {0}", exception);
            }
        }
    }
}
