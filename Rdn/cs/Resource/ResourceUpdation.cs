namespace Uccs.Rdn;

public class ResourceUpdation : RdnOperation
{
	public EntityId				Resource { get; set; }
	public ResourceChanges		Changes	{ get; set; }
	public ResourceData			Data { get; set; }

	public override bool		IsValid(McvNet net) => (!Changes.HasFlag(ResourceChanges.SetData) || Data.Value.Length <= ResourceData.LengthMax) &&
													(!Changes.HasFlag(ResourceChanges.SetData) || !Changes.HasFlag(ResourceChanges.NullData));
	public override string		Description => $"{Resource}, [{Changes}], {(Data == null ? null : $", Data={{{Data}}}")}";

	public ResourceUpdation()
	{
	}

	public ResourceUpdation(EntityId resource)
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
		Resource	= reader.Read<EntityId>();
		Changes		= (ResourceChanges)reader.ReadByte();
		
		if(Changes.HasFlag(ResourceChanges.SetData))	Data = reader.Read<ResourceData>();
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.Write(Resource);
		writer.Write((byte)Changes);

		if(Changes.HasFlag(ResourceChanges.SetData))	writer.Write(Data);
	}

	public override void Execute(RdnExecution execution)
	{
		var rs = new HashSet<int>();

		if(RequireSignerResource(execution, Resource, out var d, out var x) == false)
			return;

		d = execution.AffectDomain(d.Id);
		
		EnergyConsumed -= execution.Round.ConsensusECEnergyCost; /// the first is alredy paid

		void execute(Ura resource)
		{
			EnergyConsumed += execution.Round.ConsensusECEnergyCost;

			var r = execution.AffectResource(d, resource.Resource);

			if(rs.Contains(r.Id.E))
				return;
			else
				rs.Add(r.Id.E);

			if(Changes.HasFlag(ResourceChanges.SetData))
			{
				if(r.Flags.HasFlag(ResourceFlags.Sealed))
				{
					Error = Sealed;
					return;
				}

				if(r.Flags.HasFlag(ResourceFlags.Data))
				{
					Free(execution, Signer, d, r.Data.Value.Length);
				}

				r.Flags		|= ResourceFlags.Data;
				r.Data		= Data;
				r.Updated	= execution.Time;

				Allocate(execution, Signer, d, r.Data.Value.Length);
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

				Free(execution, Signer, d, r.Data.Value.Length);

				r.Flags	&= ~ResourceFlags.Data;
				r.Data = null;
			}

			if(Changes.HasFlag(ResourceChanges.Seal))
			{
				if(r.Flags.HasFlag(ResourceFlags.Sealed))
				{
					Error = Sealed;
					return;
				}

				r.Flags	|= ResourceFlags.Sealed;

				PayForForever(execution.Net.EntityLength + r.Length);
			}

			if(Changes.HasFlag(ResourceChanges.Recursive))
			{
				if(r.Outbounds != null)
				{
					foreach(var i in r.Inbounds)
					{
						if(i == d.Id)
						{
							var l = execution.FindResource(i);

							execute(l.Address);
						}
					}
				}
			} 
		}

		execute(x.Address);
	}
}