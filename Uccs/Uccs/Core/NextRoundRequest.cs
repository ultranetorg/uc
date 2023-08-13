namespace Uccs.Net
{
	public class NextRoundRequest : RdcRequest
	{
		public AccountAddress Generator;

		public override RdcResponse Execute(Core core)
		{
			lock(core.Lock)
			{
				if(!core.Settings.Roles.HasFlag(Role.Base))					throw new RdcNodeException(RdcNodeError.NotBase);
				if(core.Synchronization != Synchronization.Synchronized)	throw new RdcNodeException(RdcNodeError.NotSynchronized);

				var r = core.Chainbase.LastConfirmedRound.Id + Chainbase.Pitch * 2;
				
				return new NextRoundResponse {NextRoundId = r};
			}
		}
	}
	
	public class NextRoundResponse : RdcResponse
	{
		public int NextRoundId { get; set; }
	}

}
