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
}

public class PretransactingPpr : Result
{
	public int			LastConfirmedRid { get; set; }
	public int			NextNonce { get; set; }
}
