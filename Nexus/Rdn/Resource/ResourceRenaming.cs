namespace Uccs.Rdn;

public class ResourceRenaming : RdnOperation
{
	public AutoId				Resource { get; set; }
	public string				NewName { get; set; }

	public override string		Explanation => $"{nameof(Resource)}={Resource}, {nameof(NewName)}={NewName}";
	
	public override bool		IsValid(McvNet net) => true;

	public ResourceRenaming()
	{
	}


	public override void Read(Reader reader)
	{
		Resource = reader.Read<AutoId>();
		NewName = reader.ReadASCII();
	}

	public override void Write(Writer writer)
	{
		writer.Write(Resource);
		writer.WriteASCII(NewName);
	}

	public override void Execute(RdnExecution execution)
	{
		if(!RequireResourceAccess(execution, Resource, out var d, out var r))
			return;

		execution.ResourceNames.Unregister(d.Name, r.Name);

		r = execution.Resources.Affect(r.Id);
		r.Name = NewName;
	
		execution.ResourceNames.Register(d.Name, NewName, r.Id);

		execution.PayOperationEnergy(User);
	}
}
