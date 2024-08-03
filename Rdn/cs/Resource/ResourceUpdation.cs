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

			if(RequireSignerResource(round, Resource, out var a, out var x) == false)
				return;

			a = round.AffectDomain(a.Id);
			
			Transaction.EUSpent -= round.ConsensusExeunitFee; /// the first is alredy paid

			void execute(Ura resource)
			{
				Transaction.EUSpent += round.ConsensusExeunitFee;

				var r = a.AffectResource(resource.Resource);
	
				if(rs.Contains(r.Id.Ri))
					return;
				else
					rs.Add(r.Id.Ri);

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
	
					Allocate(round, a, r.Data.Value.Length);
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
					Free(a, r.Data.Value.Length);
				}

				if(Changes.HasFlag(ResourceChanges.Seal))
				{
					if(r.Flags.HasFlag(ResourceFlags.Sealed))
					{
						Error = Sealed;
						return;
					}

					r.Flags	|= ResourceFlags.Sealed;

					Signer.STBalance -= SpacetimeFee(r.Length, Mcv.Forever);
					Free(a, r.Length);
				}

				if(Changes.HasFlag(ResourceChanges.Recursive))
				{
					if(r.Outbounds != null)
					{
						foreach(var i in r.Inbounds)
						{
							if(i.Ci.SequenceEqual(a.Id.Ci) && i.Di == a.Id.Ei)
							{
								var l = mcv.Domains.FindResource(i, round.Id);

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