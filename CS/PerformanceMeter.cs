namespace Uccs.Net;

public class PerformanceMeter
{
	public int	N { get; protected set; }
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
