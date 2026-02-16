namespace Uccs.Net;

public abstract class TableStateBase
{	
	public abstract void	Absorb(TableStateBase execution);
	public abstract void	StartRoundExecution(Round round);

	public virtual void Write(BinaryWriter writer)
	{
	}

	public virtual void Read(BinaryReader reader)
	{
	}
}

public class TableState<ID, E> : TableStateBase where ID : EntityId, new() where E : class, ITableEntry
{
	public Dictionary<ID, E>	Affected = [];
	public Table<ID, E>			Table;

	public TableState(Table<ID, E> table)
	{
		Table = table;
	}

	public override void StartRoundExecution(Round round)
	{
		if(round.Id > 0)
			Affected = new (round.Previous.FindState<TableState<ID, E>>(Table).Affected);
	}

	public override void Absorb(TableStateBase execution)
	{
		var e = execution as TableState<ID, E>;

		foreach(var i in e.Affected)	
			Affected[i.Key] = i.Value;
	}
}

public interface ITableExecution
{
	public AutoId	LastCreatedId { get; set; }
}

public abstract class TableExecution<ID, E> : TableState<ID, E>, ITableExecution where ID : EntityId, new() where E : class, ITableEntry
{
	public Execution				Execution;
	public TableExecution<ID, E>	Parent;
	public AutoId					LastCreatedId { get; set; }

	protected TableExecution(Table<ID, E> table, Execution execution) : base(table)
	{
		Execution = execution;
	}
	
	public E Find(ID id)
 	{
		id = (id == AutoId.LastCreated) ? LastCreatedId as ID : id;

		if(id == null)
			return null;

 		Affected.TryGetValue(id, out var a);
 		
		if(a != null)
			return a.Deleted ? null : a;

		if(Parent != null)
			return Parent.Find(id);

		Execution.Round.FindState<TableState<ID, E>>(Table).Affected.TryGetValue(id, out a);

		if(a != null)
			return a.Deleted ? null : a;
			
		return Table.Find(id);
 	}

	public virtual E Affect(ID id)
	{
		id = id == AutoId.LastCreated ? LastCreatedId as ID : id;
		
		if(Affected.TryGetValue(id, out var a))
			return a;

		if(Parent != null)
			a = Parent.Find(id);
		else if(!Execution.Round.FindState<TableState<ID, E>>(Table).Affected.TryGetValue(id, out a))
			a = Table.Find(id);

		return Affected[id] = a.Clone() as E;
	}
}

