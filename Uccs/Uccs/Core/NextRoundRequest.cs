namespace Uccs.Net
{
	public class NextRoundRequest : RdcRequest
	{
		public AccountAddress Generator;

		public override RdcResponse Execute(Sun sun)
		{
			lock(sun.Lock)
			{
				if(!sun.Settings.Roles.HasFlag(Role.Base))					throw new RdcNodeException(RdcNodeError.NotBase);
				if(sun.Synchronization != Synchronization.Synchronized)	throw new RdcNodeException(RdcNodeError.NotSynchronized);

				var r = sun.Mcv.LastConfirmedRound.Id + Mcv.Pitch * 2;
				
				return new NextRoundResponse {NextRoundId = r};
			}
		}
	}
	
	public class NextRoundResponse : RdcResponse
	{
		public int NextRoundId { get; set; }
	}

}
