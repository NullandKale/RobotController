using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Markup;

namespace RobotController.Robot
{
    public class SensorData
    {
        public bool run = true;
        public string tag = "S";
        List<datapoint> memory;
        public datapoint mostRecent;
        public Queue<long> latencies;

        MainWindow window;

        public long averageAccumulator = 0;
        public long averageLatencyMS = 0;
        public double averageUPS = 0;
        public readonly int targetFPS;

        private readonly int averageSampleCount = 100;

        public Stopwatch timer;

        public Thread updateRequestThread;
        public bool waitingForResponse = false;

        public SensorData(MainWindow window)
        {
            memory = new List<datapoint>();
            latencies = new Queue<long>();
            timer = new Stopwatch();
            timer.Start();

            updateRequestThread = new Thread(updateMain);
            updateRequestThread.IsBackground = true;
            updateRequestThread.Start();

            this.window = window;
        }

        private void updateMain()
        {
            while(run)
            {
                if(waitingForResponse)
                {
                    Thread.Sleep(1);
                }
                else
                {
                    window.client.writeCache.Enqueue("S|update," + mostRecent.tick);
                    waitingForResponse = true;
                }
            }
        }

        public void updateTimer()
        {
            long added = timer.ElapsedMilliseconds;

            latencies.Enqueue(added);
            averageAccumulator += added;

            if (latencies.Count > averageSampleCount)
            {
                long removed = latencies.Dequeue();
                averageAccumulator -= removed;
            }

            averageUPS = (double)latencies.Count / ((double)averageAccumulator / 1000.0);

            averageLatencyMS = averageAccumulator / latencies.Count;

            timer.Restart();
        }

        public void receive(string message)
        {
            mostRecent = datapoint.Deserialize(Convert.FromBase64String(message.Trim('\n')), 0);
            memory.Add(mostRecent);
            updateDisplay();
            updateTimer();
            waitingForResponse = false;
        }

        private void updateDisplay()
        {
            if(Application.Current != null)
            {
                Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    float powerPercent = (mostRecent.battery - 5.28f).Remap(0.1f, -0.8f, 0, 100);

                    if(powerPercent < 0.6 && powerPercent > -4)
                    {
                        powerPercent = 0;
                    }

                    window.tick.Content = string.Format(averageLatencyMS + "MS {0:0.##} ups", averageUPS);
                    window.battery.Content = string.Format("{0:0} % power used", powerPercent);
                    window.range.Content = string.Format("{0:0.##} cm", mostRecent.range);
                    window.angles.Content = string.Format("[{0:0.##}, {0:0.##}, {0:0.##}]", mostRecent.AngleX, mostRecent.AngleY, mostRecent.AngleZ);
                    window.kangles.Content = string.Format("[{0:0.##}, {0:0.##}, {0:0.##}]", mostRecent.KAngleX, mostRecent.KAngleY, mostRecent.KAngleZ);
                    window.accel.Content = string.Format("[{0:0.##}, {0:0.##}, {0:0.##}]", mostRecent.aX, mostRecent.aY, mostRecent.aZ);
                    window.gyro.Content = string.Format("[{0:0.##}, {0:0.##}, {0:0.##}]", mostRecent.gX, mostRecent.gY, mostRecent.gZ);
                    window.motors.Content = "[ " + mostRecent.currentMotorL + ", " + mostRecent.currentMotorR + " ]";
                    window.servos.Content = "[ " + mostRecent.currentServo1 + ", " + mostRecent.currentServo2 + " ]";
                    try
                    {
                        window.Frame.Source = datapoint.BitmapToImageSource(datapoint.BitmapFromData(mostRecent.imageData));
                    }
                    catch (Exception e)
                    {
                        Trace.WriteLine("Image decode error: " + e.Message);
                    }
                });
            }
        }
    }
}
