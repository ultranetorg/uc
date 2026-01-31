namespace Uccs.Rdn;

public class ResourceUpdation : RdnOperation
{
	public AutoId				Resource { get; set; }
	public ResourceChanges		Changes	{ get; set; }
	public ResourceData			Data { get; set; }

	public override bool		IsValid(McvNet net) => (!Changes.HasFlag(ResourceChanges.SetData) || Data.Value.Length <= ResourceData.LengthMax) &&
													(!Changes.HasFlag(ResourceChanges.SetData) || !Changes.HasFlag(ResourceChanges.NullData));
	public override string		Explanation => $"{Resource}, [{Changes}], {(Data == null ? null : $", Data={{{Data}}}")}";

	public ResourceUpdation()
	{
	}

	public ResourceUpdation(AutoId resource)
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

	public void MakeDependable()
	{
		Changes |= ResourceChanges.Dependable;
	}

	public void MakeRecursive()
	{
		Changes |= ResourceChanges.Recursive;
	}

	public override void Read(BinaryReader reader)
	{
		Resource	= reader.Read<AutoId>();
		Changes		= (ResourceChanges)reader.ReadByte();
		
		if(Changes.HasFlag(ResourceChanges.SetData))	Data = reader.Read<ResourceData>();
	}

	public override void Write(BinaryWriter writer)
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

		d = execution.Domains.Affect(d.Id);
		
		void execute(Ura resource)
		{
			var r = execution.Resources.Affect(d, resource.Resource);

			if(rs.Contains(r.Id.E))
				return;
			else
				rs.Add(r.Id.E);

			if(Changes.HasFlag(ResourceChanges.SetData))
			{
				if(r.IsLocked(execution))
				{
					Error = Locked;
					return;
				}

				if(r.Flags.HasFlag(ResourceFlags.Data))
				{
					execution.Free(User, d, r.Data.Value.Length);
				}

				r.Flags		|= ResourceFlags.Data;
				r.Data		= Data;
				r.Updated	= execution.Time;

				execution.Allocate(User, d, r.Data.Value.Length);
			}
			else if(Changes.HasFlag(ResourceChanges.NullData))
			{
				if(r.IsLocked(execution))
				{
					Error = Locked;
					return;
				}

				if(!r.Flags.HasFlag(ResourceFlags.Data))
				{
					Error = NoData;
					return;
				}

				execution.Free(User, d, r.Data.Value.Length);

				r.Flags	&= ~ResourceFlags.Data;
				r.Data = null;
			}

			if(Changes.HasFlag(ResourceChanges.Dependable))
			{
				if(r.Flags.HasFlag(ResourceFlags.Dependable))
				{
					Error = AlreadySet;
					return;
				}

				r.Flags	|= ResourceFlags.Dependable;

				//execution.PayForForever(execution.Net.EntityLength + r.Length);
			}

			if(Changes.HasFlag(ResourceChanges.Recursive))
			{
				if(r.Outbounds != null)
				{
					foreach(var i in r.Inbounds)
					{
						if(i == d.Id)
						{
							var l = execution.Resources.Find(i);

							execute(l.Address);
						}
					}
				}
			} 

			execution.PayOperationEnergy(User);
		}

		execute(x.Address);
	}
}