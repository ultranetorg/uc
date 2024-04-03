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

		public override bool		Valid => !Changes.HasFlag(ResourceChanges.SetData) || (Data.Value.Length <= ResourceData.LengthMax);
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
			if(RequireAuthor(round, Signer, Resource.Author, out var a) == false)
				return;

			var r = a.Resources?.FirstOrDefault(i => i.Address == Resource);
					
			if(r != null)
			{
				Error = AlreadyExists;
				return;
			}

			a = Affect(round, Resource.Author);
			r = a.AffectResource(Resource.Resource);

			PayForEntity(round, a.Expiration - round.ConsensusTime);
			
			if(Changes.HasFlag(ResourceChanges.SetData))
			{
				r.Updated	= round.ConsensusTime;
				r.Data		= Data;

				if(Data != null)
				{
					r.Flags |= ResourceFlags.Data;

					if(a.SpaceReserved < a.SpaceUsed + r.Data.Value.Length)
					{
						Expand(round, a, r.Data.Value.Length);
					}
				} 
			}

			if(Changes.HasFlag(ResourceChanges.Seal))
			{
				if(r.Flags.HasFlag(ResourceFlags.Sealed))
				{
					Error = Sealed;
					return;
				}

				r.Flags	|= ResourceFlags.Sealed;

				PayForEntity(round, Time.FromYears(10));
			}
		}
	}
}