namespace Uccs.Rdn;

public class ResourceCreation : RdnOperation
{
	public Ura					Address { get; set; }
	public ResourceChanges		Changes { get; set; }
	public ResourceData			Data { get; set; }

	public override bool		IsValid(McvNet net) => (!Changes.HasFlag(ResourceChanges.SetData) || (Data.Value.Length <= ResourceData.LengthMax)) &&
													(!Changes.HasFlag(ResourceChanges.NullData));
	public override string		Description => $"{Address}, [{Changes}]{(Data == null ? null : ", Data=" + Data)}";

	public ResourceCreation()
	{
	}

	public ResourceCreation(Ura resource, ResourceData data, bool seal)
	{
		Address = resource;
		Data = data;

		if(Data != null)	Changes |= ResourceChanges.SetData;
		if(seal)			Changes |= ResourceChanges.Seal;
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

		var r = execution.FindResource(Address);
				
		if(r != null)
		{
			Error = AlreadyExists;
			return;
		}

		r = execution.AffectResource(d, Address.Resource);

		if(Changes.HasFlag(ResourceChanges.SetData))
		{
			r.Data		= Data;
			r.Flags		|= ResourceFlags.Data;
			r.Updated	= execution.Time;
		}

		if(Changes.HasFlag(ResourceChanges.Seal))
		{
			r.Flags	|= ResourceFlags.Sealed;

			PayForForever(execution.Net.EntityLength + r.Length);
		}
		else
		{	
			d = execution.AffectDomain(d.Id);
			Allocate(execution, Signer, d, execution.Net.EntityLength + r.Length);
		}
	}
}