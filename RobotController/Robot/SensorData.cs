using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
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

        ConnectedRobot window;

        public long averageAccumulator = 0;
        public long averageLatencyMS = 0;
        public double averageUPS = 0;
        public readonly int targetFPS;

        private readonly int averageSampleCount = 100;

        public Stopwatch timer;

        public Thread updateRequestThread;
        public bool waitingForResponse = false;

        public int screenshotNumber = 0;

        public Color[,] blackOffset;

        public SensorData(ConnectedRobot window)
        {
            this.window = window;

            memory = new List<datapoint>();
            latencies = new Queue<long>();
            timer = new Stopwatch();
            timer.Start();

            calibrateImage();

            updateRequestThread = new Thread(updateMain);
            updateRequestThread.IsBackground = true;
            updateRequestThread.Start();

            screenshotNumber = window.settings.readInt("screenshotCounter", 0);
        }

        public void calibrateImage()
        {
            List<Bitmap> blackImages = new List<Bitmap>();

            try
            {
                string[] images = Directory.GetFiles("./calibrationImages/fullDark/");

                for (int i = 0; i < images.Length; i++)
                {
                    Bitmap fromFile = (Bitmap)Image.FromFile(images[i]);
                    if (fromFile != null)
                    {
                        blackImages.Add(fromFile);
                    }
                }

                if (blackImages.Count > 0)
                {
                    int[,] averageAccumulatorR = new int[blackImages[0].Width, blackImages[0].Height];
                    int[,] averageAccumulatorG = new int[blackImages[0].Width, blackImages[0].Height];
                    int[,] averageAccumulatorB = new int[blackImages[0].Width, blackImages[0].Height];

                    for (int x = 0; x < blackImages.Count; x++)
                    {
                        for (int i = 0; i < blackImages[0].Width; i++)
                        {
                            for (int j = 0; j < blackImages[0].Height; j++)
                            {
                                Color c = blackImages[0].GetPixel(i, j);
                                averageAccumulatorR[i, j] += c.R;
                                averageAccumulatorG[i, j] += c.G;
                                averageAccumulatorB[i, j] += c.B;
                            }
                        }
                    }

                    blackOffset = new Color[blackImages[0].Width, blackImages[0].Height];

                    for (int i = 0; i < blackImages[0].Width; i++)
                    {
                        for (int j = 0; j < blackImages[0].Height; j++)
                        {
                            blackOffset[i, j] = Color.FromArgb(averageAccumulatorR[i, j] / blackImages.Count, averageAccumulatorG[i, j] / blackImages.Count, averageAccumulatorB[i, j] / blackImages.Count);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
            }
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
            window.updateDisplay(mostRecent);
            updateTimer();
            waitingForResponse = false;
        }

        public void takeScreenshot()
        {
            datapoint.BitmapFromData(window.sensorData.mostRecent.imageData, blackOffset).Save("screenshot" + screenshotNumber + ".png", ImageFormat.Png);
            screenshotNumber++;
            window.settings.saveString("screenshotCounter", screenshotNumber + "");
        }
    }
}
