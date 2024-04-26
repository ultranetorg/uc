using System.IO;

namespace Uccs.Net
{
	public class ResourceLinkCreation : Operation
	{
		public Ura		Source { get; set; }
		public Ura		Destination { get; set; }
		public ResourceLinkChanges	Changes  { get; set; }
		
		public override string	Description => $"Source={Source}, Destination={Destination}";
		public override bool	Valid => true;

		public ResourceLinkCreation()
		{
		}

		public ResourceLinkCreation(bool seal)
		{
			if(seal)
				Changes |= ResourceLinkChanges.Seal;
		}

		public ResourceLinkCreation(Ura source, Ura destination)
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
			Source		= reader.Read<Ura>();
			Destination	= reader.Read<Ura>();
			Changes		= (ResourceLinkChanges)reader.ReadByte();
		}

		public override void Execute(Mcv mcv, Round round)
		{
			if(Require(round, Signer, Source, out var sa, out var sr) == false)
				return;

			if(Require(round, null, Destination, out var da, out var dr) == false)
				return;

			sa = Affect(round, Source.Domain);
			sr = sa.AffectResource(Source.Resource);
			sr.AffectOutbound(dr.Id);

			da = Affect(round, Destination.Domain);
			dr = da.AffectResource(Destination.Resource);
			dr.AffectInbound(sr.Id);

			if(Changes.HasFlag(ResourceLinkChanges.Seal))
			{
				if(!sr.Flags.HasFlag(ResourceFlags.Sealed) || !dr.Flags.HasFlag(ResourceFlags.Sealed))
				{
					Error = NotSealed;
					return;
				}

				Pay(round, Mcv.EntityLength, Mcv.Forever);
			}
			else
				Allocate(round, sa, Mcv.EntityLength);
		}
	}
}
