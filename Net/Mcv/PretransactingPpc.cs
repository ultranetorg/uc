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
						LastConfirmedRid	= Mcv.LastConfirmedRound.Id,
						NextNonce			= u?.LastNonce + 1 ?? 0
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
	public int			LastConfirmedRid { get; set; }
	public int			NextNonce { get; set; }

	public override void Read(Reader reader)
	{
		LastConfirmedRid = reader.Read7BitEncodedInt();
		NextNonce = reader.Read7BitEncodedInt();
	}

	public override void Write(Writer writer)
	{
		writer.Write7BitEncodedInt(LastConfirmedRid);
		writer.Write7BitEncodedInt(NextNonce);
	}
}
