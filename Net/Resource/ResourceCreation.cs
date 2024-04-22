using System;
using System.IO;
using System.Linq;

namespace Uccs.Net
{
	public class ResourceCreation : Operation
	{
		public ResourceAddress		Resource { get; set; }
		public ResourceChanges		Changes { get; set; }
		public ResourceData			Data { get; set; }

		public override bool		Valid => (!Changes.HasFlag(ResourceChanges.SetData) || (Data.Value.Length <= ResourceData.LengthMax)) &&
											 (!Changes.HasFlag(ResourceChanges.NullData));
		public override string		Description => $"{Resource}, [{Changes}]{(Data == null ? null : ", Data=" + Data)}";

		public ResourceCreation()
		{
		}

		public ResourceCreation(ResourceAddress resource, ResourceData data, bool seal)
		{
			Resource = resource;
			Data = data;

			if(Data != null)	Changes |= ResourceChanges.SetData;
			if(seal)			Changes |= ResourceChanges.Seal;
		}

		public override void ReadConfirmed(BinaryReader reader)
		{
			Resource	= reader.Read<ResourceAddress>();
			Changes		= (ResourceChanges)reader.ReadByte();

			if(Changes.HasFlag(ResourceChanges.SetData))	Data = reader.Read<ResourceData>();
		}

		public override void WriteConfirmed(BinaryWriter writer)
		{
			writer.Write(Resource);
			writer.Write((byte)Changes);

			if(Changes.HasFlag(ResourceChanges.SetData))	writer.Write(Data);
		}

		public override void Execute(Mcv mcv, Round round)
		{
			if(RequireDomain(round, Signer, Resource.Domain, out var a) == false)
				return;

			var r = a.Resources?.FirstOrDefault(i => i.Address == Resource);
					
			if(r != null)
			{
				Error = AlreadyExists;
				return;
			}

			a = Affect(round, Resource.Domain);
			r = a.AffectResource(Resource.Resource);

			if(Changes.HasFlag(ResourceChanges.SetData))
			{
				r.Data		= Data;
				r.Flags		|= ResourceFlags.Data;
				r.Updated	= round.ConsensusTime;
			}

			if(Changes.HasFlag(ResourceChanges.Seal))
			{
				r.Flags	|= ResourceFlags.Sealed;

				Pay(round, r.Length, Mcv.Forever);
			}
			else
				Allocate(round, a, r.Length);
		}
	}
}