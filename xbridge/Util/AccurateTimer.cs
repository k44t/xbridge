using System;
using System.Diagnostics;
using System.Threading;
using System.Timers;

namespace xbridge.Util
{
    public class AccurateTimer
    {
        private readonly System.Timers.Timer timer;
        private double interval;
        public double Interval { 
            get {
                return interval;
            } set {
                interval = value;
                timer.Interval = value;
                stopWatch.Stop();
                first = true;
            } 
        }
        public float HighAccuracyPrerun = 15;
        private Stopwatch stopWatch = new Stopwatch();
        private bool first;
        private static readonly double TickLength = 1000f / Stopwatch.Frequency;
        private static double nextTick = 0;
        public event EventHandler<double> Elapsed;

        public AccurateTimer()
        {
            this.timer = new System.Timers.Timer();
            timer.AutoReset = true;
            this.timer.Elapsed += Timer_Elapsed;
        }

        void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            //timer.Stop();
            if (first)
            {
                first = false;
                timer.AutoReset = true;
                timer.Start();
                stopWatch.Start();
                nextTick = HighAccuracyPrerun;
            }
            var elapsed = stopWatch.ElapsedTicks * TickLength;
            var toWait = nextTick - elapsed;
            //Console.WriteLine("elapsed: " + elapsed + ", nextTick: " + nextTick + ", toWait: " + toWait);
            if (toWait > HighAccuracyPrerun)
                timer.Interval += 1;
            else
                timer.Interval -= 1;

            int full = 0;
            int waitA = 0;
            int waitB = 0;
            int waitC = 0;
            while (true)
            {

                if (toWait <= 0)
                {
                    Console.WriteLine("delay: " + -toWait + ", full: " + full + ", waitA: " + waitA + ", waitB: " + waitB + ", waitC: " + waitC);
                    break;
                }/*
                else if (toWait < 0.1)
                {
                    ++full;
                    Thread.SpinWait(2);
                }/*
                else if (toWait < 1)
                {
                    ++waitA;
                    Thread.SpinWait(10);
                }
                else if (toWait < 3)
                {
                    ++waitB;
                    Thread.SpinWait(100);
                }*/
                /*else if (toWait < 10)
                {
                    ++waitB;
                    Thread.SpinWait(100);
                }*/
                else
                {
                    ++waitC;
                    Thread.Sleep(1);
                }

                elapsed = stopWatch.ElapsedTicks * TickLength;
                toWait = nextTick - elapsed;
            }

            Elapsed?.Invoke(this, -toWait);



            nextTick += interval;
            // if we got lost (cpu was doin something else for some time), reset
            if (nextTick < elapsed)
            {
                Console.WriteLine("resetting nextTick");
                nextTick = elapsed + interval;
            }
            if (stopWatch.Elapsed.TotalHours >= 1d)
            {
                Console.WriteLine("resetting stopwatch");
                stopWatch.Restart();
                nextTick = interval;
            }
            //timer.Start();

        }


        public void Start()
        {
            first = true;
            timer.AutoReset = false;
            timer.Start();
        }

        public void Stop() {
            stopWatch.Stop();
            timer.Stop();
        }




    }
}
