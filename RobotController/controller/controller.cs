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
        public ConnectedRobot window;
        public Controller controller;
        public State currentControllerState;
        public Thread updateThread;
        public bool run = true;

        public readonly int thumbMin = -32767;
        public readonly int thumbMax = 32767;
        public readonly int thumbDeadZone = 5000;

        public int motorL = 0;
        public int motorR = 0;

        public int offset1 = 0;
        public int offset2 = 0;

        public Stopwatch lastInputTimer;

        public ControllerController(ConnectedRobot window)
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
                Thread.Sleep(50);
            }
        }

        private (float x, float y) getThumb(bool left)
        {
            (float x, float y) toReturn;

            float XVal;
            float YVal;

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

        private void updateServoControl((float x, float y) stick)
        {
            int servo1 = (int)stick.y.Remap(255, -255, -90, 90);
            int servo2 = (int)stick.x.Remap(255, -255, -90, 90);

            if(servo1 > 50)
            {
                servo1 = 50;
            }

            window.hardware.setServo(servo1, servo2);
        }

        private void updateMotorControl((float x, float y) stick)
        {
            float motorR;
            float motorL;

            motorL = stick.y;
            motorR = stick.y;

            //if(stick.y != 0)
            //{
            //    if (stick.x < 0)
            //    {
            //        motorL += stick.x;
            //    }
            //    else if (stick.x > 0)
            //    {
            //        motorR -= stick.x;
            //    }
            //}
            //else
            //{
            //    if (stick.x < 0)
            //    {
            //        motorL += stick.x;
            //        motorR -= stick.x;
            //    }
            //    else if (stick.x > 0)
            //    {
            //        motorR -= stick.x;
            //        motorL += stick.x;
            //    }
            //}

            if (Math.Abs(stick.y) < 90)
            {
                motorL += stick.x * 2;
                motorR -= stick.x * 2;
            }
            else if (Math.Abs(stick.x) > 46)
            {
                motorL += stick.x / 2;
                motorR -= stick.x / 2;
            }

            if (motorL > 255)
            {
                motorL = 255;
            }
            else if (motorL < -255)
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

            if (motorR > 0 && motorL > 0)
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

                if (currentControllerState.Gamepad.Buttons.HasFlag(GamepadButtonFlags.RightShoulder))
                {
                    window.hardware.setServo(0, 0);
                }

                updateMotorControl(left);
                updateServoControl(right);
            }
        }

    }
}
