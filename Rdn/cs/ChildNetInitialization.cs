namespace Uccs.Rdn
{
	public class ChildNetInitialization : RdnOperation
	{
		public EntityId		Domain  { get; set; }
		public NtnState		Net { get; set; }

		public override string		Description => $"Hash={Net.Hash.ToHex()}, Pees={Net.Peers.Length}";
		
		public ChildNetInitialization ()
		{
		}
		
		public override bool IsValid(Mcv mcv)
		{ 
			if(Net.Hash.Length > 1024)
				return false;
			
			if(Net.Peers.Length > 1000)
				return false;

			return true;
		}

		public override void ReadConfirmed(BinaryReader reader)
		{
			Domain	= reader.Read<EntityId>();
			Net		= reader.Read<NtnState>();
		}

		public override void WriteConfirmed(BinaryWriter writer)
		{
			writer.Write(Domain);
			writer.Write(Net);
		}

		public override void Execute(RdnMcv mcv, RdnRound round)
		{
			if(RequireSignerDomain(round, Domain, out var e) == false)
				return;

			if(e.NtnChildNet != null)
			{
				Error = AlreadyExists;
				return;
			}

			if(!Uccs.Rdn.Domain.IsRoot(e.Address))
			{
				Error = NotRoot;
				return;
			}

			e = round.AffectDomain(Domain);

 			e.NtnChildNet = Net;
		}
	}
}
