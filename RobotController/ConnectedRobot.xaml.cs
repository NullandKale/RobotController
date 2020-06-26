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

        public MemoryManager memory;
        public SensorData sensorData;
        public Hardware hardware;
        public Settings settings;
        public ControllerController controller;
        
        private MainWindow mainWindow;

        public ConnectedRobot(MainWindow mainWindow, string ip)
        {
            InitializeComponent();

            this.mainWindow = mainWindow;
            controller = new ControllerController(this);
            hardware = new Hardware(this);

            settings = new Settings();
            memory = new MemoryManager(this);

            client = new tcpClient(ip);
            client.connect();

            sensorData = new SensorData(this);
            client.taggedReceivers.Add(sensorData.tag, sensorData.receive);

            controller.startThread();

            Closing += ConnectedRobot_Closing;
        }

        private void ConnectedRobot_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            memory.Stop();
            mainWindow.Close();
        }

        public void updateMemoryState(string state)
        {
            if (Application.Current != null)
            {
                Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    memoryState.Content = state;
                });
            }
        }

        public void updateDisplay(datapoint mostRecent)
        {
            if (Application.Current != null)
            {
                Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    float powerPercent = (mostRecent.battery - 5.28f).Remap(0.1f, -0.8f, 0, 100);
                    int motorProgL = (int)((float)mostRecent.currentMotorL).Remap(-255, 255, 0, 100);
                    int motorProgR = (int)((float)mostRecent.currentMotorR).Remap(-255, 255, 0, 100);

                    if (powerPercent < 0.6 && powerPercent > -4)
                    {
                        powerPercent = 0;
                    }

                    tick.Content = string.Format("{0:0} ups | {1:0} ms | {2:0.##} v | {3:0.##} cm", sensorData.averageUPS, sensorData.averageLatencyMS, mostRecent.battery, mostRecent.range);
                    motorL.Content = "" + mostRecent.currentMotorL;
                    motorR.Content = "" + mostRecent.currentMotorR;
                    motorProgressL.Value = motorProgL;
                    motorProgressR.Value = motorProgR;
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
                case Key.P:
                    {
                        sensorData.takeScreenshot();
                        break;
                    }
            }
        }

        private void REC_click(object sender, RoutedEventArgs e)
        {
            memory.Record();
        }

        private void PLAY_click(object sender, RoutedEventArgs e)
        {
            memory.Play();
        }

        private void STOP_click(object sender, RoutedEventArgs e)
        {
            memory.Stop();
        }

        private void LOAD_click(object sender, RoutedEventArgs e)
        {
            memory.loadMemories();
        }
    }
}
