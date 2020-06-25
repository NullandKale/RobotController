using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RobotController.util
{
    public class TimedBackgroundThread
    {
        public bool run = false;
        public int delay = 0;
        public Thread thread;
        public Action action;

        public TimedBackgroundThread(int delay, Action action)
        {
            this.action = action;
            this.delay = delay;

            thread = new Thread(Run);
            thread.IsBackground = true;

        }

        public void Start()
        {
            run = true;
            thread.Start();
        }

        public void Stop()
        {
            run = false;
        }

        private void Run()
        {
            while(run)
            {
                action.Invoke();
                Thread.Sleep(delay);
            }
        }
    }
}
