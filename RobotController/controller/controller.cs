using RobotController.Robot;
using SharpDX.XInput;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
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

        private (float x, float y) getThumb(bool left)
        {
            (float x, float y) toReturn;

            float XVal = 0;
            float YVal = 0;

            if (left)
            {
                XVal = currentControllerState.Gamepad.LeftThumbX;
                YVal = currentControllerState.Gamepad.LeftThumbY;
            }
            else
            {
                XVal = currentControllerState.Gamepad.RightThumbX;
                YVal = currentControllerState.Gamepad.RightThumbY;
            }

            if (YVal > thumbDeadZone || YVal < -thumbDeadZone)
            {
                toReturn.y = YVal.Remap(thumbMin, thumbMax, -255, 255);
            }
            else
            {
                toReturn.y = 0;
            }

            if (XVal > thumbDeadZone || XVal < -thumbDeadZone)
            {
                toReturn.x = XVal.Remap(thumbMin, thumbMax, -255, 255);
            }
            else
            {
                toReturn.x = 0;
            }

            return toReturn;
        }

        private void updateMotorControl((float x, float y) stick)
        {
            float motorR;
            float motorL;

            motorL = stick.y;
            motorR = stick.y;

            if(Math.Abs(stick.y) < 90)
            {
                motorL += stick.x * 2;
                motorR -= stick.x * 2;
            }
            else if(Math.Abs(stick.x) > 46)
            {
                motorL += stick.x / 2;
                motorR -= stick.x / 2;
            }

            if(motorL > 255)
            {
                motorL = 255;
            }
            else if(motorL < -255)
            {
                motorL = -255;
            }

            if (motorR > 255)
            {
                motorR = 255;
            }
            else if (motorR < -255)
            {
                motorR = -255;
            }

            if(motorR > 0 && motorL > 0)
            {
                if(window.sensorData.mostRecent.range <= 25 && window.sensorData.mostRecent.range != 0)
                {
                    float reduction = window.sensorData.mostRecent.range.Remap(0, 20, Math.Min(motorR, motorL), 0);
                    motorL -= reduction;
                    motorR -= reduction;
                }
            }

            window.hardware.setMotor((int)motorL, (int)motorR);
        }

        private void updateState()
        {
            if (controller.IsConnected)
            {
                currentControllerState = controller.GetState();

                (float x, float y) left = getThumb(true);
                (float x, float y) right = getThumb(false);

                if(currentControllerState.Gamepad.Buttons.HasFlag(GamepadButtonFlags.LeftShoulder))
                {
                    window.sensorData.takeScreenshot();
                }

                updateMotorControl(left);
            }
        }

    }
}
