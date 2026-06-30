using System;

namespace WebApplication2
{
    internal static class Counters2
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
            //your code
            lock (_thisLock)
            {
                _counter = _counter + 1;
            }
        }

        internal static int GetCounter()
        {
            int temp = 0;
            lock (_thisLock)
            {
                temp = _counter;
            }
            return temp;
        }

        internal static void IncrementActives()
        {
            //your code
            lock (_thisLock)
            {
                _actives = _actives + 1;
            }
        }

        internal static void DecrementActives()
        {
            //your code
            lock (_thisLock)
            {
                _actives = _actives - 1;
            }
        }

        internal static int GetActives()
        {
            int temp = 0;
            lock (_thisLock)
            {
                temp = _actives;
            }
            return temp;
        }
    }
}
