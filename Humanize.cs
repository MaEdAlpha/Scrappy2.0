using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Scrappy2._0
{
    class Humanize
    {
        public static void RandomSleep(int baseTimeMilliseconds) //Thread.Sleep(milliseconds) modification
        {
            Random r1 = new Random();
            float randTime = r1.Next(100, 555);
            int time = (int)Math.Round(baseTimeMilliseconds + (2 / ((randTime / 1000) * 100)));
            Thread.Sleep(time); // creates thread sleep for basetime + 360ms - 2000ms random generator
        }

        public static void Shuffle<T>(IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = ThreadSafeRandom.ThisThreadsRandom.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
