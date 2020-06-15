using SharpDX.XInput;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace RobotController
{
    public class ControllerController
    {
        public MainWindow window;
        public Controller controller;
        public State currentControllerState;
        public Thread updateThread;
        public bool run = true;

        public readonly int thumbMin = -32767;
        public readonly int thumbMax = 32767;
        public readonly int thumbDeadZone = 6000;

        public int motorL = 0;
        public int motorR = 0;

        public int offset1 = 0;
        public int offset2 = 0;

        public ControllerController(MainWindow window)
        {
            this.window = window;
            controller = new Controller(UserIndex.One);
            updateThread = new Thread(updateThreadMain);
            updateThread.IsBackground = true;
        }

        public void startThread()
        {
            updateThread.Start();
        }

        private void updateThreadMain()
        {
            while(run)
            {
                updateState();
                Thread.Sleep(125);
            }
        }

        public void updateState()
        {
            if (controller.IsConnected)
            {
                currentControllerState = controller.GetState();
                float leftYVal = currentControllerState.Gamepad.LeftThumbY;
                float rightYVal = currentControllerState.Gamepad.RightThumbY;
                float rightTrigger = currentControllerState.Gamepad.RightTrigger;
                float leftTrigger = currentControllerState.Gamepad.LeftTrigger;

                motorR = 0;
                motorL = 0;

                if (rightTrigger > 0)
                {
                    motorL = (int)rightTrigger;
                    motorR = (int)rightTrigger;
                }

                if (leftTrigger > 0)
                {
                    motorL = (int)-leftTrigger;
                    motorR = (int)-leftTrigger;
                }

                if (leftYVal > thumbDeadZone || leftYVal < -thumbDeadZone)
                {
                    int val = (int)leftYVal.Remap(thumbMin, thumbMax, -255, 255);
                    if (val > 0 && val > motorL)
                    {
                        motorL = val;
                    }
                    else if (val < 0 && val < motorL)
                    {
                        motorL = val;
                    }
                }

                if (rightYVal > thumbDeadZone || rightYVal < -thumbDeadZone)
                {
                    int val = (int)rightYVal.Remap(thumbMin, thumbMax, -255, 255);
                    if(val > 0 && val > motorR)
                    {
                        motorR = val;
                    }
                    else if(val < 0 && val < motorR)
                    {
                        motorR = val;
                    }
                }

                window.hardware.setMotor(motorL, motorR);
            }
        }
    }
}
