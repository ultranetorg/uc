namespace Uccs.Net;

public interface IExpirable
{
	short	Expiration { get; set; }

	bool IsExpired(Time time) 
	{
		return time.Days > Expiration;
	}

	bool CanRenew(Time time, Time duration)
	{
		return !IsExpired(time) && Expiration + duration.Days - time.Days < Time.FromYears(10).Days;
	}
}
