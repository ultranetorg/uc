using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UC.Net
{
	public class PerformanceMeter
	{
		int			N;
		TimeSpan	Time;
		DateTime	Start;

		public void Reset()
		{
			N = 0;
			Time = TimeSpan.Zero;
		}

		public void Begin()
		{
			N++;
			Start = DateTime.UtcNow;
		}

		public void End()
		{
			Time += DateTime.UtcNow - Start;
		}

		public TimeSpan Avarage => N > 0 ? new TimeSpan(Time.Ticks/N) : TimeSpan.Zero;
	}
}
