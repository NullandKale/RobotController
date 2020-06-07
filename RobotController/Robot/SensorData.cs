using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Markup;

namespace RobotController.Robot
{
    public class SensorData
    {
        public string tag = "S";
        List<datapoint> memory;
        datapoint mostRecent;

        MainWindow window;

        public SensorData(MainWindow window)
        {
            memory = new List<datapoint>();
            this.window = window;
        }

        public void receive(string message)
        {
            mostRecent = datapoint.Deserialize(Convert.FromBase64String(message.Trim('\n')), 0);
            Trace.WriteLine(mostRecent.tick);
            memory.Add(mostRecent);
            updateDisplay();
        }

        private void updateDisplay()
        {
            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                window.tick.Content = mostRecent.tick;
                window.battery.Content = mostRecent.battery + "V per cell";
                window.range.Content = mostRecent.range + " cm";
                window.angles.Content = "[ " + mostRecent.AngleX + ", " + mostRecent.AngleY + ", " + mostRecent.AngleZ + " ]";
                window.kangles.Content = "[ " + mostRecent.KAngleX + ", " + mostRecent.KAngleY + ", " + mostRecent.KAngleZ + " ]";
                window.accel.Content = "[ " + mostRecent.aX + ", " + mostRecent.aY + ", " + mostRecent.aZ + " ]";
                window.gyro.Content = "[ " + mostRecent.gX + ", " + mostRecent.gX + ", " + mostRecent.gZ + " ]";
                window.motors.Content = "[ " + mostRecent.currentMotorL + ", " + mostRecent.currentMotorR + " ]";
                window.servos.Content = "[ " + mostRecent.currentServo1 + ", " + mostRecent.currentServo2 + " ]";
                window.Frame.Source = datapoint.BitmapToImageSource(datapoint.BitmapFromData(mostRecent.imageData));
            });
        }
    }
}
