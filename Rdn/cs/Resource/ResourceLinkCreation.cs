namespace Uccs.Rdn
{
	public class ResourceLinkCreation : RdnOperation
	{
		public ResourceId			Source { get; set; }
		public ResourceId			Destination { get; set; }
		public ResourceLinkChanges	Changes  { get; set; }
		
		public override string	Description => $"Source={Source}, Destination={Destination}";
		public override bool	IsValid(Mcv mcv) => true;

		public ResourceLinkCreation()
		{
		}

		public ResourceLinkCreation(bool seal)
		{
			if(seal)
				Changes |= ResourceLinkChanges.Seal;
		}

		public ResourceLinkCreation(ResourceId source, ResourceId destination)
		{
			Source = source;
			Destination = destination;
		}

		public override void WriteConfirmed(BinaryWriter writer)
		{
			writer.Write(Source);
			writer.Write(Destination);
			writer.Write((byte)Changes);
		}
		
		public override void ReadConfirmed(BinaryReader reader)
		{
			Source		= reader.Read<ResourceId>();
			Destination	= reader.Read<ResourceId>();
			Changes		= (ResourceLinkChanges)reader.ReadByte();
		}

		public override void Execute(RdnMcv mcv, RdnRound round)
		{
			if(RequireSignerResource(round, Source, out var sd, out var sr) == false)
				return;

			if(RequireResource(round, Destination, out var dd, out var dr) == false)
				return;

			var ss = round.AffectSite(sd.Id);
			sr = ss.AffectResource(sd, sr.Address.Resource);
			sr.AffectOutbound(dr.Id);

			//dd = round.AffectDomain(dd.Id);
			var ds = round.AffectSite(dd.Id);
			dr = ds.AffectResource(dd, dr.Address.Resource);
			dr.AffectInbound(sr.Id);

			if(Changes.HasFlag(ResourceLinkChanges.Seal))
			{
				if(!sr.Flags.HasFlag(ResourceFlags.Sealed) || !dr.Flags.HasFlag(ResourceFlags.Sealed))
				{
					Error = NotSealed;
					return;
				}

				PayForSpacetime(Mcv.EntityLength, Mcv.Forever);
			}
			else
			{	
				sd = round.AffectDomain(sd.Id);
				Allocate(round, sd, Mcv.EntityLength);
			}
		}
	}
}
