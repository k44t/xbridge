using System;
//using System.Timers;
using xbridge.Util;
namespace xbridge.Modules
{
    public class HRTimer: XBridgeSharedObject
    {
        private HighResolutionTimer timer;
        private bool sendZero;
        private bool isTimeout;

        public HRTimer(XBridge bridge, float milliseconds, bool? isTimeout, bool? sendZero, bool hpt) : base(bridge) {
            this.timer = new HighResolutionTimer();
            timer.UseHighPriorityThread = hpt;
            timer.Elapsed += _Timer_Elapsed;
            if (isTimeout != null && (bool)isTimeout)
                this.Timeout(milliseconds);
            else
                this.Interval(milliseconds);
            this.SendZero(sendZero);
        }

        void _Timer_Elapsed(object sender, HighResolutionTimerElapsedEventArgs e)
        {
            Console.WriteLine("delay: " + e.Delay);
            this._Trigger("elapsed");
        }

        public double? Interval(double? milliseconds) {
            if (milliseconds == null)
            {
                if (!this.isTimeout)
                {
                    return this.timer.Interval;
                }
                else
                {
                    return null;
                }
            }
            timer.Interval = (float)milliseconds;
            this.isTimeout = false;
            return null;
        }


        public void Start()
        {
            timer.Start();
        }

        public void Stop() {
            timer.Stop();
        }

        public double? Timeout(double? milliseconds)
        {
            if (milliseconds == null)
            {
                if (!this.isTimeout)
                {
                    return null;
                }
                else
                {
                    return this.timer.Interval;
                }
            }
            timer.Interval = (float)milliseconds;
            this.isTimeout = true;
            return null;
        }

        public bool? SendZero(bool? v)
        {
            if (v == null)
            {
                return this.sendZero;
            }
            this.sendZero = (bool) v;
            return null;
        }

        public override void Destroy()
        {
            this.timer.Stop();
        }


    }

    public class HighResolutionTimers
    {
        private XBridge bridge;
        private bool useHighPriorityThread = false;

        public HighResolutionTimers(XBridge xbridge)
        {
            this.bridge = xbridge;
        }

        public HRTimer Create(double milliseconds, bool? isTimeout, bool? sendZero) {
            return new HRTimer(bridge, (float)milliseconds, isTimeout, sendZero, useHighPriorityThread);
        }

        public bool? UseHighPriorityThread(bool? v)
        {
            if (v == null)
            {
                return this.useHighPriorityThread;
            }
            this.useHighPriorityThread = (bool)v;
            return null;
        }
    }
}
