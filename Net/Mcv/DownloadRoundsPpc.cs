using System.Diagnostics;

namespace Uccs.Net;

public class DownloadRoundsPpc : McvPpc<DownloadRoundsPpr>
{
	public const int	SizeMaximum = 512 * 1024;
	
	public int			From { get; set; }
	public int			SizeLimit { get; set; } = SizeMaximum;
	
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
						GraphHash			= Mcv.GraphHash,
						Rounds				= [..rs.Select(i => i.Raw)]
					};
		}
	}

	public override void Write(Writer writer)
	{
		writer.Write7BitEncodedInt(From);
		writer.Write7BitEncodedInt(SizeLimit);
	}

	public override void Read(Reader reader)
	{
		From		= reader.Read7BitEncodedInt();
		SizeLimit	= reader.Read7BitEncodedInt();
	}
}

public class DownloadRoundsPpr : Result
{
	public int			LastNonEmptyRound { get; set; }
	public int			LastConfirmedRound { get; set; }
	public byte[]		GraphHash{ get; set; }
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

	public override void Read(Reader reader)
	{
		LastNonEmptyRound	= reader.Read7BitEncodedInt();
		LastConfirmedRound	= reader.Read7BitEncodedInt();
		GraphHash			= reader.ReadHash();
		Rounds				= reader.ReadArray(() => reader.ReadBytes());
	}

	public override void Write(Writer writer)
	{
		writer.Write7BitEncodedInt(LastNonEmptyRound);
		writer.Write7BitEncodedInt(LastConfirmedRound);
		writer.Write(GraphHash);
		writer.Write(Rounds, i => writer.WriteBytes(i));
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