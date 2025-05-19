using System.Collections;
using System.Text;

namespace Uccs.Fair;

public abstract class HnswExecution<D, E> : HnswTableState<D, E>  where E : HnswNode<D>
{
	public FairExecution					Execution;

	//public abstract  E						Affect(HnswId id);
	protected abstract int					DataToBucket(D data);

	public class LevelEnumerator : IEnumerator<E>
	{
		public E							Current => (Affected ?? TailGraph).Current;
		object								IEnumerator.Current => Current;

		byte								Level;
		HnswTable<D, E>						Table;
		IEnumerator<E>						Affected;
		HnswTable<D, E>.LevelEnumerator		TailGraph;
		HashSet<E>							Unique = new HashSet<E>(EqualityComparer<E>.Create((a, b) => a.Id == b.Id, i => i.Id.GetHashCode()));


		public LevelEnumerator(HnswTable<D, E> table, IEnumerable<E> affected, byte level, int ridmax)
		{
			Table = table;
			Level = level;

			Affected = affected.GetEnumerator();
			TailGraph = new (Table, Level, ridmax, Unique);
		}

		public void Reset()
		{
			throw new NotImplementedException();
		}

		public void Dispose()
		{
			Affected?.Dispose();
			TailGraph?.Dispose();
		}

		public bool MoveNext()
		{
			while(true)
			{
				if(Affected != null)
				{
					if(Affected.MoveNext())
					{
						if(Affected.Current.Level != Level)
							continue;
						
						Unique.Add(Affected.Current);
						return true;
					}
					else
					{
						Affected = null;
						continue;
					}
				}
				else
				{
					return TailGraph.MoveNext();
				}
			}
		}
	}

	protected HnswExecution(FairExecution execution, HnswTable<D, E> table) : base(table)
	{
		Execution = execution;
		EntryPoints = execution.Round.FindState<HnswTableState<D, E>>(Table).EntryPoints ?? Table.Assosiated.EntryPoints;
	}
	
	public List<E> AffectEntryPoints()
	{
		EntryPoints = new(EntryPoints);

		return EntryPoints;
	}

	public byte RandomLevel(byte[] start)
	{
		var r = start;
		
		uint p = 24109; /// = 2^16 * 0.367879 ~> double p = 1.0 / Math.E;
		byte level = 0;

		while(((r[1]<<8 | r[0])) < p && level < Table.MaxLevel)
		{	
			level++;

			r = Cryptography.Hash(2, r);
		}

		return level;
	}

	public Table<HnswId, E>.EntityEnumeration GetLevel(byte level)
	{
		return new Table<HnswId, E>.EntityEnumeration(() => new LevelEnumerator(Table, Affected.Values, level, Execution.Round.Id));
	}

	public E Find(HnswId id)
 	{
 		if(Affected.TryGetValue(id, out var a))
 			return a;
 		
		return Table.Find(id, Execution.Round.Id);
 	}

	public void Add(E node)
	{
		for(byte l = 0; l <= node.Level; l++)
		{
			var neighbors = EntryPoints.Count > 0 ? Table.EfSearch(node.Data, EntryPoints[0], l, Table.EfConstruction, null, Find) : [];

			if(neighbors.Count() == 0)
			{
				var fallback = GlobalBestNeighbor(node, l);
				
				if(fallback != null)
					neighbors = [fallback];
			}

			var topNeighbors = ApplyEFHeuristic(neighbors, Table.MaxConnections);

			foreach(var i in topNeighbors)
			{
				var neighbor = Affect(i.Id);

				node.AddConnection(l, neighbor);
				neighbor.AddConnection(l, node);
			}
		}

		if(EntryPoints.Count == 0 || node.Level > EntryPoints[0].Level)
		{
			AffectEntryPoints();
			EntryPoints.Clear();
			EntryPoints.Add(node);
		}
	}
	
	private E? GlobalBestNeighbor(HnswNode<D> node, byte level)
	{
// 		if(!Layers.ContainsKey(level))
// 			return null;
// 
		int bestDist = int.MaxValue;
		E? best = null;

		foreach(var n in GetLevel(level))
		{
			if(node == n) /// we already have n in Affected
				continue;

			int dist = Table.Metric.ComputeDistance(node.Data, n.Data);

			if(dist < bestDist)
			{
				bestDist = dist;
				best = n;
			}
		}

		return best;
	}
	
	private List<E> ApplyEFHeuristic(IEnumerable<E> candidates, int maxConnections)
	{
		var selected = new List<E>();

		foreach(var candidate in candidates)
		{
			bool diverse = true;

			// Проверяем на разнообразие
			foreach(var existing in selected)
			{
				if(Table.Metric.ComputeDistance(candidate.Data, existing.Data) < Table.MinDiversity)
				{
					diverse = false;
					break;
				}
			}

			// Добавляем, если прошёл по разнообразию или уже нужно просто добрать
			if(diverse || selected.Count < maxConnections)
			{
				selected.Add(candidate);
			}

			// Ограничиваем количество соединений
			if(selected.Count >= maxConnections)
				break;
		}

		return selected;
	}

	public virtual E Create(HnswId id)
	{
 		var a = Table.Create();
 		a.Id = id;
 		a.Connections = [];
 		
 		return Affected[id] = a;
	}

	public virtual E Affect(HnswId id)
	{
 		if(Affected.TryGetValue(id, out var a))
 			return a;
 		
 		a = Table.Find(id, Execution.Round.Id);
 
		a = a.Clone() as E;

		var e = EntryPoints.Find(i => i.Id == a.Id);
			
		if(e != null)
		{
			AffectEntryPoints();
			EntryPoints.Remove(e);
			EntryPoints.Add(a);
		}

 		return Affected[id] = a;
	}

	public E Find(D data)
 	{
		var e = Affected.Values.FirstOrDefault(i => i.Data.Equals(data));

 		if(e != null)
			if(!e.Deleted)
    			return e;
			else
				return null;

  		foreach(var i in Execution.Mcv.Tail.Where(i => i.Id <= Execution.Round.Id))
		{	
			e = i.FindState<HnswTableState<D, E>>(Table).Affected.Values.FirstOrDefault(i => i.Data.Equals(data));

			if(e != null)
				if(!e.Deleted)
    				return e;
				else
					return null;
		}
 		
		e = Table.FindBucket(DataToBucket(data))?.Entries.FirstOrDefault(i => i.Data.Equals(data));

		if(e != null)
			if(!e.Deleted)
    			return e;
			else
				return null;

		return null;
 	}
}

public class StringHnswTableExecution<E> : HnswExecution<string, E>  where E : StringHnswEntity
{
	public StringHnswTableExecution(FairExecution execution, HnswTable<string, E> table) : base(execution, table)
	{
	}

	protected override int DataToBucket(string data)
	{
		var x = Encoding.UTF8.GetBytes(data, 0, Math.Min(data.Length, 32));
 		return HnswId.ToBucket(RandomLevel(Cryptography.Hash(2, x)), x);
	} 

  	public virtual E Index(AutoId entity, string text)
  	{
 		text = text.ToLowerInvariant();
 
 		var e =	Find(text);
 
  		if(e == null)
  		{
			var b = DataToBucket(text);

  			var id = new HnswId(b, Execution.GetNextEid(Table, b));
  	
  			e = Create(id);
  	
  			e.Text  = text;
 			//e.Hash = Metric.Hashify(text);
  			
  			Add(e);
  		}
  		else
 			e = Affect(e.Id);
 
		return e;
  	}
	
  	public E Affect(string text)
  	{
 		text = text.ToLowerInvariant();
 
  		var e =	Find(text);
 	
 		e = Affect(e.Id);

		return e;
  	}
}


