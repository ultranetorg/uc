namespace Uccs.Rdn
{
	public class ResourceUpdation : RdnOperation
	{
		public ResourceId			Resource { get; set; }
		public ResourceChanges		Changes	{ get; set; }
		public ResourceData			Data { get; set; }

		public override bool		IsValid(Mcv mcv) => (!Changes.HasFlag(ResourceChanges.SetData) || Data.Value.Length <= ResourceData.LengthMax) &&
														(!Changes.HasFlag(ResourceChanges.SetData) || !Changes.HasFlag(ResourceChanges.NullData));
		public override string		Description => $"{Resource}, [{Changes}], {(Data == null ? null : $", Data={{{Data}}}")}";

		public ResourceUpdation()
		{
		}

		public ResourceUpdation(ResourceId resource)
		{
			Resource = resource;
		}

		public void Change(ResourceData data)
		{
			Data = data;
			
			if(data != null)
				Changes = ResourceChanges.SetData;
			else
				Changes = ResourceChanges.NullData;
		}

		public void Seal()
		{
			Changes |= ResourceChanges.Seal;
		}

		public void MakeRecursive()
		{
			Changes |= ResourceChanges.Recursive;
		}

		public override void ReadConfirmed(BinaryReader reader)
		{
			Resource	= reader.Read<ResourceId>();
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
			var rs = new HashSet<int>();

			if(RequireSignerResource(round, Resource, out var d, out var x) == false)
				return;

			d = round.AffectDomain(d.Id);
			
			Transaction.ECSpent -= round.ConsensusExecutionFee; /// the first is alredy paid

			void execute(Ura resource)
			{
				Transaction.ECSpent += round.ConsensusExecutionFee;

				var r = round.AffectResource(d, resource.Resource);
	
				if(rs.Contains(r.Id.R))
					return;
				else
					rs.Add(r.Id.R);

				if(Changes.HasFlag(ResourceChanges.SetData))
				{
					if(r.Flags.HasFlag(ResourceFlags.Sealed))
					{
						Error = Sealed;
						return;
					}

					r.Flags		|= ResourceFlags.Data;
					r.Data		= Data;
					r.Updated	= round.ConsensusTime;
	
					Allocate(round, d, r.Data.Value.Length);
				}
				else if(Changes.HasFlag(ResourceChanges.NullData))
				{
					if(r.Flags.HasFlag(ResourceFlags.Sealed))
					{
						Error = Sealed;
						return;
					}

					if(!r.Flags.HasFlag(ResourceFlags.Data))
					{
						Error = NoData;
						return;
					}

					r.Flags	&= ~ResourceFlags.Data;
					Free(d, r.Data.Value.Length);
				}

				if(Changes.HasFlag(ResourceChanges.Seal))
				{
					if(r.Flags.HasFlag(ResourceFlags.Sealed))
					{
						Error = Sealed;
						return;
					}

					r.Flags	|= ResourceFlags.Sealed;

					Signer.BYBalance -= SpacetimeFee(r.Length, Mcv.Forever);
					Free(d, r.Length);
				}

				if(Changes.HasFlag(ResourceChanges.Recursive))
				{
					if(r.Outbounds != null)
					{
						foreach(var i in r.Inbounds)
						{
							if(i == d.Id)
							{
								var l = mcv.Resources.Find(i, round.Id);

								execute(l.Address);
							}
						}
					}
				} 
			}

			execute(x.Address);
		}
	}
}