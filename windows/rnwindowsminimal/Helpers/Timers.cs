//using System;

//namespace rnwindowsminimal.Helpers
//{
//    public static class Timers
//    {
//        private static System.Timers.Timer Timer;

//        public Timers()
//        {
//        }

//        private static void initialiseTimer(object sender, EventArgs e)
//        {
//            // Create a timer with a two second interval.
//            Timer = new System.Timers.Timer(2000);
//            // Hook up the Elapsed event for the timer. 
//            Timer.Elapsed += OnTimedEvent;
//            Timer.AutoReset = true;
//            Timer.Enabled = true;
//        }

//        private static void OnTimedEvent(Object source, ElapsedEventArgs e)
//        {
//            Console.WriteLine("The Elapsed event was raised at {0:HH:mm:ss.fff}",
//                              e.SignalTime);
//        }
//    }
//}