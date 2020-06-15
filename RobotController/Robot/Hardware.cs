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
                if(r == l)
                {
                    if (r == 255)
                    {
                        r = 255;
                        l = 235;
                    }
                    else if(r == -255)
                    {
                        r = -255;
                        l = -255;
                    }
                }

                if(window.sensorData.mostRecent.range == 0 || window.sensorData.mostRecent.range > 30 || (l < 0 && r < 0))
                {
                    window.client.writeCache.Enqueue("motor|" + (r) + "," + (l));
                }
                else
                {
                    window.client.writeCache.Enqueue("motor|0,0");
                }
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
