using System;

namespace Uccs.Net
{
	public class Clock
	{
		public virtual DateTime	Now {get;}
	}

	public class RealClock : Clock
	{
		override public DateTime Now => DateTime.UtcNow;
	}

	public class SimulationClock : Clock
	{
		DateTime	Time = DateTime.UtcNow;
		DateTime	Last = DateTime.UtcNow;
		float		Speed = 1f;

		public SimulationClock()
		{
		}

		public SimulationClock(float speed)
		{
			Speed = speed;
		}

		override public DateTime Now
		{
			get
			{
				Time += (DateTime.UtcNow - Last) * Speed;
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
