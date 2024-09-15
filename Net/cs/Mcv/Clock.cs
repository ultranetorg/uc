namespace Uccs.Net
{
	public class IClock
	{
		public virtual DateTime	Now {get;}
	}

	public class RealClock : IClock
	{
		override public DateTime Now => DateTime.UtcNow;
	}

	public class SimulationClock : IClock
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

		public void Add(TimeSpan interval)
		{
			Time += interval;
			Last = DateTime.UtcNow;
		}

		public void Add(Time interval)
		{
			Time += TimeSpan.FromDays(interval.Days);
			Last = DateTime.UtcNow;
		}
	}
}
