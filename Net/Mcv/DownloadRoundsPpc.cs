using System.Diagnostics;

namespace Uccs.Net;

public class DownloadRoundsPpc : McvPpc<DownloadRoundsPpr>
{
	public int			From { get; set; }
	public int			SizeLimit { get; set; } = SizeMaximum;
	public const int	SizeMaximum = 512 * 1024;
	
	public override Result Execute()
	{
		RequireGraph();

		if(SizeLimit > SizeMaximum)
			throw new RequestException(RequestError.IncorrectRequest);

		lock(Mcv.Lock)
		{
			if(Mcv.LastNonEmptyRound == null)	
				throw new NodeException(NodeError.TooEearly);

			if(From > Mcv.LastConfirmedRound.Id)
				throw new RequestException(RequestError.OutOfRange);
		
			var rs = Enumerable.Range(From, Mcv.LastConfirmedRound.Id - From + 1).Select(Mcv.FindRound).WhereAggregateWhile(0, (a, i) => a + i.Raw.Length, a => a < SizeLimit);

			return	new DownloadRoundsPpr 
					{	
						LastNonEmptyRound	= Mcv.LastNonEmptyRound.Id,
						LastConfirmedRound	= Mcv.LastConfirmedRound.Id,
						BaseHash			= Mcv.GraphHash,
						Rounds				= [..rs.Select(i => i.Raw)]
					};
		}
	}
}

public class DownloadRoundsPpr : Result
{
	public int			LastNonEmptyRound { get; set; }
	public int			LastConfirmedRound { get; set; }
	public byte[]		BaseHash{ get; set; }
	public byte[][]		Rounds { get; set; }

	public Round[] Read(Mcv mcv, Constructor constructor)
	{
		if(Rounds == null)
			return [];

		return [..Rounds.Select(i =>	{
											var r = mcv.CreateRound();
											r.Restore(i);
											return r;
										})];
	}
}

public static class LinqExtensions
{
	public static IEnumerable<T> WhereAggregateWhile<T, A>(this IEnumerable<T> source, A seed,Func<A, T, A> func, Func<A, bool> predicate)
	{
		A accumulator = seed;

		foreach(var item in source)
		{
			accumulator = func(accumulator, item);

			if(!predicate(accumulator))
				yield break;

			yield return item;
		}
	}
}