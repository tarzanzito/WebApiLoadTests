using System;
using System.Threading;

namespace WebApplication2
{
    internal static class Counters
    {
        private static int _counter = 0;
        private static int _actives = 0;

        private static Object _thisLock = new Object();

        //private static int Value = 42;
        //public static void Foo()
        //{
        //    Interlocked.Increment(ref Value);
        //}

        internal static void IncrementCounter()
        {
            Interlocked.Increment(ref _counter);
        }

        internal static int GetCounter()
        {
            //Interlocked.Read(ref _counter);
            //Interlocked.Read(ref _counter);
            ////int temp = 0;
            ////lock (_thisLock)
            ////{
            ////    temp = _counter;
            ////}
            return _counter;
        }

        internal static void IncrementActives()
        {
            Interlocked.Increment(ref _actives);
        }

        internal static void DecrementActives()
        {
            Interlocked.Decrement(ref _actives);
        }

        internal static int GetActives()
        {
            //int temp = 0;
            //lock (_thisLock)
            //{
            //    temp = _actives;
            //}
            return _actives;
        }
    }
}
