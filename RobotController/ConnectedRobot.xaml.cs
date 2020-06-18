using RobotController.netcode;
using RobotController.Robot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RobotController
{
    /// <summary>
    /// Interaction logic for RobotController.xaml
    /// </summary>
    public partial class ConnectedRobot : Window
    {
        public tcpClient client;
        public SensorData sensorData;
        public ControllerController controller;
        public Hardware hardware;
        public Settings settings;

        private int offset1 = 0;
        private int offset2 = 0;
        private MainWindow mainWindow;

        public ConnectedRobot(MainWindow mainWindow, string ip)
        {
            InitializeComponent();

            this.mainWindow = mainWindow;
            controller = new ControllerController(this);
            hardware = new Hardware(this);

            settings = new Settings();

            client = new tcpClient(ip);
            client.connect();

            sensorData = new SensorData(this);
            client.taggedReceivers.Add(sensorData.tag, sensorData.receive);

            controller.startThread();

            this.Closing += ConnectedRobot_Closing;
        }

        private void ConnectedRobot_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            mainWindow.Close();
        }

        public void updateDisplay(datapoint mostRecent)
        {
            if (Application.Current != null)
            {
                Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    float powerPercent = (mostRecent.battery - 5.28f).Remap(0.1f, -0.8f, 0, 100);
                    int motorProgL = (int)((float)mostRecent.currentMotorL).Remap(-255, 255, 0, 100);
                    int motorProgR = (int)((float)mostRecent.currentMotorL).Remap(-255, 255, 0, 100);

                    if (powerPercent < 0.6 && powerPercent > -4)
                    {
                        powerPercent = 0;
                    }

                    tick.Content = string.Format(sensorData.averageLatencyMS + "MS {0:0.##} ups", sensorData.averageUPS);
                    battery.Content = string.Format("{0:0} % power used", powerPercent);
                    range.Content = string.Format("{0:0.##} cm", mostRecent.range);
                    angles.Content = string.Format("[{0:0.##}, {0:0.##}, {0:0.##}]", mostRecent.AngleX, mostRecent.AngleY, mostRecent.AngleZ);
                    kangles.Content = string.Format("[{0:0.##}, {0:0.##}, {0:0.##}]", mostRecent.KAngleX, mostRecent.KAngleY, mostRecent.KAngleZ);
                    accel.Content = string.Format("[{0:0.##}, {0:0.##}, {0:0.##}]", mostRecent.aX, mostRecent.aY, mostRecent.aZ);
                    gyro.Content = string.Format("[{0:0.##}, {0:0.##}, {0:0.##}]", mostRecent.gX, mostRecent.gY, mostRecent.gZ);
                    motorL.Content = "" + mostRecent.currentMotorL;
                    motorR.Content = "" + mostRecent.currentMotorR;
                    motorProgressL.Value = motorProgL;
                    motorProgressR.Value = motorProgR;
                    servos.Content = "[ " + mostRecent.currentServo1 + ", " + mostRecent.currentServo2 + " ]";
                    try
                    {
                        Frame.Source = datapoint.BitmapToImageSource(datapoint.BitmapFromData(mostRecent.imageData, sensorData.blackOffset));
                    }
                    catch (Exception e)
                    {
                        Trace.WriteLine("Image decode error: " + e.Message);
                    }
                });
            }
        }

        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            switch (e.Key)
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
                case Key.O:
                    {
                        offset1 = 0;
                        offset2 = 0;
                        break;
                    }
            }

            if (sensorData != null)
            {
                hardware.setServo(offset1, offset2);
            }
        }
    }
}
