using System;
using System.Timers;
namespace xbridge.Modules
{
    public class Timer: XBridgeSharedObject
    {
        private System.Timers.Timer timer;
        private bool? sendZero;

        public Timer(XBridge bridge, double milliseconds, bool? isTimeout, bool? sendZero) : base(bridge) {
            this.timer = new System.Timers.Timer();
            timer.Elapsed += _Timer_Elapsed;
            if (isTimeout != null && (bool)isTimeout)
                this.Timeout(milliseconds);
            else
                this.Interval(milliseconds);
            this.SendZero(sendZero);
        }

        void _Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this._Trigger("elapsed");
        }

        public double? Interval(double? milliseconds) {
            if (milliseconds == null)
            {
                if (timer.AutoReset)
                {
                    return this.timer.Interval;
                }
                else
                {
                    return null;
                }
            }
            timer.Interval = (double)milliseconds;
            timer.AutoReset = true;
            return null;
        }


        public void Start()
        {
            timer.Start();
            if(this.sendZero == true)
            {
                this._Trigger("elapsed");
            }
        }

        public void Stop() {
            timer.Stop();
        }

        public double? Timeout(double? milliseconds)
        {
            if (milliseconds == null)
            {
                if (timer.AutoReset)
                {
                    return null;
                }
                else
                {
                    return this.timer.Interval;
                }
            }
            timer.Interval = (double)milliseconds;
            timer.AutoReset = false;
            timer.Start();
            return null;
        }

        public bool? SendZero(bool? v)
        {
            if (v == null)
            {
                return this.sendZero;
            }
            this.sendZero = v;
            return null;
        }

        public override void Destroy()
        {
            this.timer.Stop();
        }


    }

    public class Timers
    {
        private XBridge bridge;

        public Timers(XBridge xbridge)
        {
            this.bridge = xbridge;
        }

        public Timer Create(double milliseconds, bool? isTimeout, bool? sendZero) {
            return new Timer(bridge, milliseconds, isTimeout, sendZero);
        }
    }
}
