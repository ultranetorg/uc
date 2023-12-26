using System;
using System.Linq;
using Uccs.Net;

namespace Tests
{
	class Program
	{
		public static void Main(string[] args)
		{
			var r = new Random();
			var h = new byte[32];
			var x = Enumerable.Range(0, 100).Select(i => new Member {Account = AccountKey.Create()}).OrderBy(i => i.Account).ToList();

			while(true)
			{
				r.NextBytes(h);

				var n = x.OrderByNearest(h).Take(3).ToArray();

				Console.WriteLine($"{x.IndexOf(n[0]),10}{x.IndexOf(n[1]),10}{x.IndexOf(n[2]),10}");
			}
		}
	}
}
