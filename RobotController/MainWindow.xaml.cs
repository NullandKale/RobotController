using RobotController.netcode;
using RobotController.Robot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

namespace RobotController
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public tcpClient client;
        public SensorData sensorData;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Trace.WriteLine("Connecting to robot at: " + IP.Text);
            client = new tcpClient(IP.Text);
            client.connect();

            sensorData = new SensorData(this);
            client.taggedReceivers.Add(sensorData.tag, sensorData.receive);
            client.receivers.Add((string s) =>
            {
                if(s.Length > 10)
                {
                    Trace.WriteLine(s.Substring(0, 10));
                }
                else
                {
                    Trace.WriteLine(s);
                }
            });
        }
    }
}
