using System;
using System.Timers;
using xbridge.Util;
namespace xbridge.Modules
{
    public class AccurateTimer : XBridgeSharedObject
    {
        private Util.AccurateTimer timer;
        private bool? sendZero;

        public AccurateTimer(XBridge bridge, double milliseconds, bool? isTimeout, bool? sendZero) : base(bridge) {
            this.timer = new Util.AccurateTimer();
            timer.Elapsed += _Timer_Elapsed;
            if (isTimeout != null && (bool)isTimeout)
                this.Timeout(milliseconds);
            else
                this.Interval(milliseconds);
            this.SendZero(sendZero);
        }

        void _Timer_Elapsed(object sender, double delay)
        {
            this._Trigger("elapsed");
        }

        public double? Interval(double? milliseconds) {
            if (milliseconds == null)
            {

                    return this.timer.Interval;
            }
            timer.Interval = (double)milliseconds;
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
            throw new NotImplementedException();
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

    public class AccurateTimers
    {
        private XBridge bridge;

        public AccurateTimers(XBridge xbridge)
        {
            this.bridge = xbridge;
        }

        public AccurateTimer Create(double milliseconds, bool? isTimeout, bool? sendZero) {
            return new AccurateTimer(bridge, milliseconds, isTimeout, sendZero);
        }
    }
}
