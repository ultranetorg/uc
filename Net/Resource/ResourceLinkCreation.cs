using System.IO;

namespace Uccs.Net
{
	public class ResourceLinkCreation : Operation
	{
		public ResourceAddress	Source { get; set; }
		public ResourceAddress	Destination { get; set; }
		
		public override string	Description => $"Source={Source}, Destination={Destination}";
		public override bool	Valid => true;

		public ResourceLinkCreation()
		{
		}

		public ResourceLinkCreation(ResourceAddress source, ResourceAddress destination)
		{
			Source = source;
			Destination = destination;
		}

		public override void WriteConfirmed(BinaryWriter writer)
		{
			writer.Write(Source);
			writer.Write(Destination);
		}
		
		public override void ReadConfirmed(BinaryReader reader)
		{
			Source	= reader.Read<ResourceAddress>();
			Destination	= reader.Read<ResourceAddress>();
		}

		public override void Execute(Mcv mcv, Round round)
		{
			var s = mcv.Accounts.Find(Signer, round.Id);
					
			if(s == null)
			{
				Error = NotFound;
				return;
			}

			if(Require(round, Signer, Source, out var sa, out var sr) == false)
				return;

			if(Require(round, null, Destination, out var da, out var dr) == false)
				return;

			sa = Affect(round, Source.Author);
			sr = sa.AffectResource(Source.Resource);
			sr.AffectOutbound(dr.Id);

			da = Affect(round, Destination.Author);
			dr = da.AffectResource(Destination.Resource);
			dr.AffectInbound(sr.Id);

			PayForEntity(round, round.ConsensusTime - sa.Expiration);
		}
	}
}
