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
        public ControllerController controller;
        public Hardware hardware;
        public Settings settings;
        public MainWindow()
        {
            InitializeComponent();
            controller = new ControllerController(this);
            hardware = new Hardware(this);

            settings = new Settings();

            controller.startThread();
        }

        int offset1 = 0;
        int offset2 = 0;

        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            switch(e.Key)
            {
                case Key.Up:
                    {
                        offset1++;
                        break;
                    }
                case Key.Down:
                    {
                        offset1--;
                        break;
                    }
                case Key.Left:
                    {
                        offset2++;
                        break;
                    }
                case Key.Right:
                    {
                        offset2--;
                        break;
                    }
                case Key.P:
                    {
                        sensorData.takeScreenshot();
                        break;
                    }
            }

            if (sensorData != null)
            {
                hardware.setServo(offset1, offset2);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Trace.WriteLine("Connecting to robot at: " + IP.Text);
            client = new tcpClient(IP.Text);
            client.connect();

            sensorData = new SensorData(this);
            client.taggedReceivers.Add(sensorData.tag, sensorData.receive);

            IP.IsEnabled = false;
            //client.receivers.Add((string s) =>
            //{
            //    if(s.Length > 10)
            //    {
            //        Trace.WriteLine(s.Substring(0, 10));
            //    }
            //    else
            //    {
            //        Trace.WriteLine(s);
            //    }
            //});
        }
    }
}
