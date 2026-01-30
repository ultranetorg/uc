namespace Uccs.Rdn;

public class ResourceCreation : RdnOperation
{
	public Ura					Address { get; set; }
	public ResourceChanges		Changes { get; set; }
	public ResourceData			Data { get; set; }

	public override bool		IsValid(McvNet net) =>	(!Changes.HasFlag(ResourceChanges.SetData) || (Data.Value.Length <= ResourceData.LengthMax)) &&
														!Changes.HasFlag(ResourceChanges.NullData);
	public override string		Explanation => $"{Address}, [{Changes}]{(Data == null ? null : ", Data=" + Data)}";

	public ResourceCreation()
	{
	}

	public ResourceCreation(Ura resource, ResourceData data, bool referancable)
	{
		Address = resource;
		Data = data;

		if(Data != null)	Changes |= ResourceChanges.SetData;
		if(referancable)	Changes |= ResourceChanges.Dependable;
	}

	public override void Read(BinaryReader reader)
	{
		Address	= reader.Read<Ura>();
		Changes	= (ResourceChanges)reader.ReadByte();

		if(Changes.HasFlag(ResourceChanges.SetData))	Data = reader.Read<ResourceData>();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Address);
		writer.Write((byte)Changes);

		if(Changes.HasFlag(ResourceChanges.SetData))	writer.Write(Data);
	}

	public override void Execute(RdnExecution execution)
	{
		if(RequireDomainAccess(execution, Address.Domain, out var d) == false)
			return;

		var r = execution.Resources.Find(Address);
				
		if(r != null)
		{
			Error = AlreadyExists;
			return;
		}

		r = execution.Resources.Affect(d, Address.Resource);

		if(Changes.HasFlag(ResourceChanges.SetData))
		{
			r.Data		= Data;
			r.Flags		|= ResourceFlags.Data;
			r.Updated	= execution.Time;
		}

		if(Changes.HasFlag(ResourceChanges.Dependable))
		{
			r.Flags	|= ResourceFlags.Dependable;
			///execution.PayForForever(execution.Net.EntityLength + r.Length);
		}
		else
		{	
			d = execution.Domains.Affect(d.Id);
			execution.Allocate(User, d, execution.Net.EntityLength + r.Length);
			d.ResetFreeIfNeeded(execution);
		}

		execution.PayOperationEnergy(User);
	}
}