using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UC.Net
{
	public class TimeProvider
	{
		public virtual DateTime	Now {get;}
	}

	public class RealTimeProvider : TimeProvider
	{
		override public DateTime	Now => DateTime.UtcNow;
	}

	public class SimulationTimeProvider : TimeProvider
	{
		DateTime Time = DateTime.UtcNow;
		DateTime Last = DateTime.UtcNow;

		override public DateTime Now
		{
			get
			{
				Time += DateTime.UtcNow - Last;
				Last = DateTime.UtcNow;
				return Time;
			}
		}

		public void Add(TimeSpan s)
		{
			Time += s;
			Last = DateTime.UtcNow;
		}
	}
}
