namespace Uccs.Rdn;

//public enum EntityTextField : byte
//{
//	UserName, 
//	DomainId,
//	DomainNameOwnerUserId,
//}

public class NameIndex : TextTable<TextToEntity> 
{
	public NameIndex(Mcv mcv) : base(mcv, RdnTable.UserName.ToString(), true)
	{
	}
	
	public override TextToEntity Create()
	{
		return new TextToEntity(Mcv);
	}
}

public class NameExecution : TextExecution<TextToEntity, NameIndex>
{
	public NameExecution(RdnExecution execution) : base(execution.Mcv.UserNames, execution)
	{
	}

	public TextToEntity Find(string t)
	{
		return Find(new StringId(t));
	}

	public override TextToEntity Affect(StringId id)
	{
		var a = Find(id);

		if(a == null)
		{
			a = Table.Create();
			a.Id = id;
		
			return Affected[id] = a;
		} 
		else
		{
			return Affected[id] = a.Clone() as TextToEntity;
		}
	}

	public void Register(string name, AutoId entity)
	{
		var id = new StringId(name);
		var w = Affect(id);
	
		w.Entity = entity;
	}

	public void Unregister(string name)
	{
		var id = new StringId(name);
		var w = Affect(id);

		w.Deleted = true;
	}
}