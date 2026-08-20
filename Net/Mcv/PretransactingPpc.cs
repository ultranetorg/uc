namespace Uccs.Net;

public class PretransactingPpc : McvPpc<PretransactingPpr>
{
	public string User { get; set; }

	public override Result Execute()
	{
		RequireGraph();
		
		lock(Mcv)
		{
			var u = Mcv.Users.Latest(User);

			return  new PretransactingPpr
					{
						LastConfirmedRound	= Mcv.LastConfirmedRound.Id,
						NextNonce			= u?.LastNonce + 1 ?? 0,
						Bandwidth			= u?.Bandwidth ?? 0
					};
		}
	}

	public override void Read(Reader reader)
	{
		User = reader.ReadASCII();
	}

	public override void Write(Writer writer)
	{
		writer.WriteASCII(User);
	}
}

public class PretransactingPpr : Result
{
	public int			LastConfirmedRound { get; set; }
	public int			NextNonce { get; set; }
	public int			Bandwidth { get; set; }

	public override void Read(Reader reader)
	{
		LastConfirmedRound	= reader.Read7BitEncodedInt();
		NextNonce			= reader.Read7BitEncodedInt();
		Bandwidth			= reader.Read7BitEncodedInt();
	}

	public override void Write(Writer writer)
	{
		writer.Write7BitEncodedInt(LastConfirmedRound);
		writer.Write7BitEncodedInt(NextNonce);
		writer.Write7BitEncodedInt(Bandwidth);
	}
}
