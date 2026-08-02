namespace Uccs.Net;

public class PretransactingPpc : McvPpc<PretransactingPpr>, IBinarySerializable
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

	public void Read(Reader reader)
	{
		User = reader.ReadASCII();
	}

	public void Write(Writer writer)
	{
		writer.WriteASCII(User);
	}
}

public class PretransactingPpr : Result, IBinarySerializable
{
	public int			LastConfirmedRid { get; set; }
	public int			NextNonce { get; set; }

	public void Read(Reader reader)
	{
		LastConfirmedRid = reader.Read7BitEncodedInt();
		NextNonce = reader.Read7BitEncodedInt();
	}

	public void Write(Writer writer)
	{
		writer.Write7BitEncodedInt(LastConfirmedRid);
		writer.Write7BitEncodedInt(NextNonce);
	}
}
