using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Threading;

namespace FocusedObjective.Simulation.Extensions
{

    public static class ThreadSafeRandom
  {
      [ThreadStatic] private static Random Local;

      public static Random ThisThreadsRandom
      {
          get { return Local ?? (Local = new Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId))); }
      }
  }

  
internal static class IListExtensions
    {

        //TODO:Investigate this algorithm
        /*
         * http://www.goodplan.ca/2011/08/proper-shuffle.html
         * 
         * The Knuth Fisher Yates Shuffle removes the bias without adding any computational load. Instead of swapping with any other element, you swap only with downstream elements, like this:
 	n = dist.length
	while(--n){
		r = randomInteger() % (n)
		t = dist[r]
		dist[r] = dist[n]
		dist[n] = t
	}
*/

    internal static void ShuffleFast<T>(this IList<T> list)
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


        internal static void Shuffle<T>(this IList<T> list)
        {
            RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
            int n = list.Count;
            while (n > 1)
            {
                byte[] box = new byte[1];
                do provider.GetBytes(box);
                while (!(box[0] < n * (Byte.MaxValue / n)));

                int k = (box[0] % n);
                n--;
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        internal static IEnumerable<IEnumerable<T>> GetPowerSet<T>(this IList<T> list)
        {
            return from m in Enumerable.Range(0, 1 << list.Count)
                   select
                       from i in Enumerable.Range(0, list.Count)
                       where (m & (1 << i)) != 0
                       select list[i];
        }


    }
}
