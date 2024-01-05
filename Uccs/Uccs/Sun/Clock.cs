using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uccs.Net
{
	public class Clock
	{
		public virtual DateTime	Now {get;}
	}

	public class RealClock : Clock
	{
		override public DateTime	Now => DateTime.UtcNow;
	}

	public class SimulationClock : Clock
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
