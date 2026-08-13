namespace Uccs.Rdn;

public enum EntityTextField : byte
{
	UserName, 
	DomainName, 
}

public class NameIndex : TextTable<TextToFields<EntityTextField>> 
{
	public NameIndex(Mcv mcv) : base(mcv, RdnTable.Name.ToString(), true)
	{
	}
	
	public override TextToFields<EntityTextField> Create()
	{
		return new TextToFields<EntityTextField>(Mcv);
	}
}

public class NameExecution : TextExecution<TextToFields<EntityTextField>, NameIndex>
{
	public NameExecution(RdnExecution execution) : base(execution.Mcv.Names, execution)
	{
	}

	public override TextToFields<EntityTextField> Affect(StringId id)
	{
		if(Affected.TryGetValue(id, out var a))
			return a;
			
		if(Parent != null)
			a = Parent.Find(id);
		else if(!(Execution.Round as RdnRound).Names.Affected.TryGetValue(id, out a))
			a = Table.Find(id);

		if(a == null)
		{
			a = Table.Create();
			a.Id = id;
			a.Entities = [];
		
			return Affected[id] = a;
		} 
		else
		{
			return Affected[id] = a.Clone() as TextToFields<EntityTextField>;
		}
	}

	public void Register(string name, EntityTextField field, AutoId entity)
	{
		var id = TextTable<TextToFields<EntityTextField>>.GetId(name);
		var w = Affect(id);
	
		w.Entities = [..w.Entities, new EntityField<EntityTextField>{Id = entity, Field = field}];
	}

	public void Unregister(string name, EntityTextField field)
	{
		var id = TextTable<TextToFields<EntityTextField>>.GetId(name);
		var w = Affect(id);

		if(w.Entities.Count > 1)
		{
			var e = w.Entities.Find(i => i.Field == field);
			w.Entities = w.Entities.Remove(e);
		} 
		else
		{
			w.Deleted = true;
		}
	}
}