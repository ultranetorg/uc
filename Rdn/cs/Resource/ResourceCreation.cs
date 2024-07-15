namespace Uccs.Rdn
{
	public class ResourceCreation : RdnOperation
	{
		public Ura					Resource { get; set; }
		public ResourceChanges		Changes { get; set; }
		public ResourceData			Data { get; set; }

		public override bool		IsValid(Mcv mcv) => (!Changes.HasFlag(ResourceChanges.SetData) || (Data.Value.Length <= ResourceData.LengthMax)) &&
														(!Changes.HasFlag(ResourceChanges.NullData));
		public override string		Description => $"{Resource}, [{Changes}]{(Data == null ? null : ", Data=" + Data)}";

		public ResourceCreation()
		{
		}

		public ResourceCreation(Ura resource, ResourceData data, bool seal)
		{
			Resource = resource;
			Data = data;

			if(Data != null)	Changes |= ResourceChanges.SetData;
			if(seal)			Changes |= ResourceChanges.Seal;
		}

		public override void ReadConfirmed(BinaryReader reader)
		{
			Resource	= reader.Read<Ura>();
			Changes		= (ResourceChanges)reader.ReadByte();

			if(Changes.HasFlag(ResourceChanges.SetData))	Data = reader.Read<ResourceData>();
		}

		public override void WriteConfirmed(BinaryWriter writer)
		{
			writer.Write(Resource);
			writer.Write((byte)Changes);

			if(Changes.HasFlag(ResourceChanges.SetData))	writer.Write(Data);
		}

		public override void Execute(RdnMcv mcv, RdnRound round)
		{
			if(RequireSignerDomain(round, Resource.Domain, out var a) == false)
				return;

			var r = a.Resources?.FirstOrDefault(i => i.Address == Resource);
					
			if(r != null)
			{
				Error = AlreadyExists;
				return;
			}

			a = round.AffectDomain(Resource.Domain);
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

				PayForSpacetime(r.Length, Mcv.Forever);
			}
			else
				Allocate(round, a, r.Length);
		}
	}
}