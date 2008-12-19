using System;
using Logger;

namespace CommonLib
{
	public class UserOnHost
	{
		public string host;
		public long userCount;

		public UserOnHost(long userCount, string host)
		{
			this.userCount = userCount;
			this.host = host;
		}

		public override string ToString()
		{
			return host + ":" + userCount;
		}
	}

	public class Class1
	{
		public static void preMP(string x, int[] mpNext)
		{
			var m = x.Length;
			var i = 0;
			var j = -1;
			mpNext[0] = -1;
			while (i < m)
			{
				while (j > -1 && x[i] != x[j])
				{
					j = mpNext[j];
				}
				mpNext[++i] = ++j;
			}
		}

		static void MP(string x, string y) {
			int m = x.Length;
			int n = y.Length;
			int i, j;
			int[] mpNext = new int[m + 1];

			/* Preprocessing */
			preMP(x, mpNext);

			/* Searching */
			i = j = 0;
			while (j < n) 
			{
				while (i > -1 && x[i] != y[j])
					i = mpNext[i];
				i++;
				j++;
				if (i >= m) 
				{
					Console.WriteLine(j - i);
					i = mpNext[i];
				}
			}
		}

		public static void Main(string[] args)
		{
			string testStr = "abcab";
			string parentStr = "defadcabghi";
			MP(testStr, parentStr);

//			Cache<long, string> cache = new Cache<long, string>(1000);
//			cache.Insert(1, "hello world", new TimeSpan(0, 0, 10), false, NotifyRemoved);
//			Logger.Log.WriteLog(cache[1]);
//			cache.Remove(1);
			Console.ReadLine();
		}

		private static void NotifyRemoved(object value)
		{
            Logger.Log.WriteLog("Object remove: " + value);
		}
	}
}
