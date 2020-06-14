using System;
using System.Collections.Generic;
using System.Text;

namespace RobotController.Robot
{
    public class Hardware
    {
        public MainWindow window;

        public Hardware(MainWindow window)
        {
            this.window = window;
        }

        public void setMotor(int l, int r)
        {
            if (window.client != null && window.sensorData != null)
            {
                window.client.writeCache.Enqueue("motor|" + (r) + "," + (l));
            }
        }

        public void setServo(int servo1, int servo2)
        {
            if(window.client != null)
            {
                window.client.writeCache.Enqueue("servo|" + (servo1) + "," + (servo2));
            }
        }
    }
}
