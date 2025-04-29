using System.Collections;
using System.Collections.Generic;

namespace Uccs.Fair;

public interface IMetric<D>
{
    int		ComputeDistance(D a, D b);
}

public abstract class HnswNode<D> : ITableEntry, IBinarySerializable
{
	public HnswId									Id { get; set; }
	public abstract D								Data { get; }
	public SortedDictionary<int, HnswId[]>			Connections { get; set; }
	
	public byte										Level => Id.Level;
	public EntityId									Key => Id;
	public bool										Deleted { get;  set; }

	public abstract void							Cleanup(Round lastInCommit);
	public abstract void							ReadMain(BinaryReader r);
	public abstract void							WriteMain(BinaryWriter r);

	public override string ToString()
	{
		return $"{Id}, {Data}, Connections={Connections.Count}";
	}

	public void AddConnection(int level, HnswNode<D> node)
	{
		Connections = new(Connections);

		if(Connections.ContainsKey(level))
			Connections[level] = [..Connections[level], node.Id];
		else
			Connections[level] = [node.Id];
	}

	public virtual void Write(BinaryWriter writer)
	{
		writer.Write(Id);
		//writer.Write(Hash);
		writer.Write(Connections, i => writer.Write((byte)i), i => writer.Write(i));
	}

	public virtual void Read(BinaryReader reader)
	{
		Id			= reader.Read<HnswId>();
		//Hash		= reader.ReadUInt64();
		Connections = reader.ReadSortedDictionary(() => (int)reader.ReadByte(), () => reader.ReadArray<HnswId>());
	}
}

public abstract class HnswTable<D, E> : Table<HnswId, E> where E : HnswNode<D>
{
	public IMetric<D>							Metric;
	public readonly int							MaxLevel;
	public readonly int							MaxConnections;
	public readonly int							EfConstruction;
	public readonly int							Threshold;
	public readonly int							MinDiversity;

	public List<E>								EntryPoints = new();

	public class LevelEnumerator : IEnumerator<E>
	{
		public E				Current => Entity.Current;
		object					IEnumerator.Current => Entity.Current;

		byte					Level;
		HnswTable<D, E>			Table;
		HashSet<E>				Unique;
		IEnumerator<Round>		Round;
		IEnumerator<E>			Entity;
		IEnumerator<Bucket>		Bucket;
		IEnumerator<Cluster>	Cluster;

		public LevelEnumerator(HnswTable<D, E> table, byte level, int rinmax, HashSet<E> unique)
		{
			Table = table;
			Level = level;
			Unique = unique ?? new HashSet<E>(EqualityComparer<E>.Create((a, b) => a.Id == b.Id, i => i.Id.GetHashCode()));

			Round = Table.Mcv.Tail.SkipWhile(i => i.Id > rinmax).GetEnumerator();
			Cluster = Table.Clusters.GetEnumerator();
		}

		public void Reset()
		{
			throw new NotImplementedException();
		}

		public void Dispose()
		{
			Round?.Dispose();
			Entity?.Dispose();
		}

		public bool MoveNext()
		{
			while(true)
			{
				if(Round != null)
				{
					if(Entity == null)
					{	
						if(!Round.MoveNext())
						{	
							Round = null;
							continue;
						}
						else
							while(!Round.Current.Confirmed) 
								if(!Round.MoveNext())
								{
									Round = null; /// No confirmed rounds
									break;
								}

						if(Round == null)  
							continue; /// No confirmed rounds
								
						Entity = Round.Current.AffectedByTable<HnswId, E>(Table).Values.GetEnumerator();
					}
	
					if(Entity.MoveNext())
					{
						if(Entity.Current.Level != Level || Unique.Contains(Entity.Current))
							continue;
						
						Unique.Add(Entity.Current);
						return true;
					}
					else
						Entity = null;
				}
				else
				{
					if(Bucket == null)
					{	
						if(!Cluster.MoveNext())
							return false;

						if(HnswId.BucketToLevel(Cluster.Current.Id) != Level)
							continue;

						Bucket = Cluster.Current.Buckets.GetEnumerator();
					}

					if(Entity == null)
					{	
						if(!Bucket.MoveNext())
						{	
							Bucket = null;
							continue;
						}
							
						Entity = Bucket.Current.Entries.GetEnumerator();
					}

					if(Entity.MoveNext())
					{
						if(Entity.Current.Level != Level || Unique.Contains(Entity.Current))
							continue;
						
						Unique.Add(Entity.Current);
						return true;
					}
					else
						Entity = null;
				}
			}
		}
	}

	public HnswTable(Mcv mcv, IMetric<D> metric, int maxLevel = 5, int maxConnections = 5, int efConstruction = 64, int threshold = 10, int minDiversity = 10) : base(mcv)
	{
		Metric = metric;
		MaxLevel = maxLevel;
		MaxConnections = maxConnections;
		EfConstruction = efConstruction;
		Threshold = threshold;
		MinDiversity = minDiversity;
	}

	public override void Load()
	{
		int maxlevel = 0;

		EntryPoints.Clear();

		foreach(var r in Mcv.Tail.SkipWhile(i => !i.Confirmed))
			foreach(var i in (r.AffectedByTable(this) as IDictionary<HnswId, E>).Values)
				if(i.Level > maxlevel)
					maxlevel = i.Level;

		foreach(var c in Clusters)
			if(HnswId.ClusterToLevel(c.Id) > maxlevel)
				maxlevel = HnswId.ClusterToLevel(c.Id);

		foreach(var c in Clusters.Where(i => HnswId.ClusterToLevel(i.Id) == maxlevel))
			foreach(var b in c.Buckets)
				EntryPoints.AddRange(b.Entries);

		foreach(var r in Mcv.Tail.SkipWhile(i => !i.Confirmed))
			foreach(var i in (r.AffectedByTable(this) as IDictionary<HnswId, E>).Values)
				if(i.Level == maxlevel && !EntryPoints.Any(j => j.Id == i.Id))
					EntryPoints.Add(i);
	}

	public override void StartExecution(Execution execution)
	{
		//Execution = execution;

		//EntryPoints = new(ConfirmedEntryPoints);
	}

// 	public EntityEnumeration GetLevelLatest(byte level)
// 	{
// 		return new EntityEnumeration(() => new LevelEnumerator(this, level, Mcv.LastConfirmedRound.Id, null));
// 	}

/// 	public void Remove(string data)
/// 	{
/// 		bool removedEntry = false;
/// 
/// 		foreach(var level in Layers.Keys.ToList())
/// 		{
/// 			var node = Layers[level].FirstOrDefault(n => n.Data == data);
/// 			if(node != null)
/// 			{
/// 				foreach(var conn in node.Connections.GetValueOrDefault(level, new()))
/// 					conn.Connections[level]?.RemoveAll(n => n.Data == data);
/// 
/// 				Layers[level].Remove(node);
/// 
/// 				if(Layers[level].Count == 0)
/// 					Layers.Remove(level);
/// 
/// 				if(EntryPoints.Contains(node))
/// 					removedEntry = true;
/// 			}
/// 		}
/// 
/// 		if(removedEntry)
/// 		{
/// 			var all = Layers.SelectMany(kvp => kvp.Value).ToList();
/// 			if(all.Any())
/// 			{
/// 				var newEntry = all.OrderByDescending(n => n.Level).First();
/// 				EntryPoints.Clear();
/// 				EntryPoints.Add(newEntry);
/// 			}
/// 			else
/// 			{
/// 				EntryPoints.Clear();
/// 			}
/// 		}
/// 	}

/// 	public void Rebuild()
/// 	{
/// 		Console.WriteLine("[HNSW] Rebuilding graph...");
/// 		var allData = Layers
/// 			.SelectMany(kv => kv.Value)
/// 			.Select(n => n.Data)
/// 			.Distinct()
/// 			.ToList();
/// 
/// 		Layers.Clear();
/// 		EntryPoints.Clear();
/// 
/// 		foreach(var item in allData)
/// 			Add(item);
/// 
/// 		Console.WriteLine($"[HNSW] Rebuild complete. Total nodes: {allData.Count}");
/// 	}

	public IEnumerable<E> Search(D query, int skip, int k, Func<E, bool> criteria, Func<HnswId, E> find, List<E> entrypoints, int efSearch = 32)
	{
		if(entrypoints.Count == 0)
			return [];

		//var queryHash = Metric.Hashify(query);
		var current = entrypoints[0];

		for(var level = current.Level; level >= 1; level--)
			current = SearchBestNeighbor(query, current, level, find);

		var resultNodes = EfSearch(query, current, 0, efSearch, skip, criteria, find);

		return resultNodes.Take(k);
	}

	public IEnumerable<E> EfSearch(D query, E entry, int level, int ef, int skip, Func<E, bool> criteria, Func<HnswId, E> find)
	{
		var visited = new HashSet<HnswId>();
		var candidates = new PriorityQueue<E, int>();

		// Сортировка по расстоянию (меньше — лучше)
		var topCandidates = new SortedSet<(int dist, E node)>(Comparer<(int dist, E node)>.Create((a, b) =>	{
																												int cmp = a.dist.CompareTo(b.dist);
																												
																												if(cmp != 0) 
																													return cmp;

																												// Учитываем Id, чтобы избежать конфликтов
																												return a.node.Id.CompareTo(b.node.Id);
																											}));

		int entryDist = Metric.ComputeDistance(query, entry.Data);
		candidates.Enqueue(entry, entryDist);
		visited.Add(entry.Id);
		topCandidates.Add((entryDist, entry));

		while(candidates.Count > 0 && topCandidates.Count < ef)
		{
			candidates.TryDequeue(out var current, out _);

			foreach(var neighborId in current.Connections.GetValueOrDefault(level, []))
			{
				if(visited.Contains(neighborId))
					continue;

				visited.Add(neighborId);

				var neighbor = find(neighborId);

				if(criteria != null && !criteria(neighbor))
					continue;

				if(skip > 0)
				{	
					skip--;
					continue;
				}

				int dist = Metric.ComputeDistance(query, neighbor.Data);

				var candidate = (dist, neighbor);
				topCandidates.Add(candidate);
				candidates.Enqueue(neighbor, dist);

				// Обрезаем, если превысили ef
				while(topCandidates.Count > ef)
					topCandidates.Remove(topCandidates.Max); // Удаляем наихудшего
			}
		}

		// Возвращаем как список
		return topCandidates.Select(x => x.node);
	}

	private E SearchBestNeighbor(D query, E start, int level, Func<HnswId, E> find)
	{
		var visited = new HashSet<HnswId>();
		var queue = new Queue<E>();
		queue.Enqueue(start);

		E best = start;
		int bestDist = Metric.ComputeDistance(query, best.Data);

		while(queue.Count > 0)
		{
			var current = queue.Dequeue();
			visited.Add(current.Id);

			foreach(var i in current.Connections.GetValueOrDefault(level, []))
			{
				if(!visited.Contains(i))
				{
					var neighbor = find(i);

					int dist = Metric.ComputeDistance(query, neighbor.Data);

					if(dist < bestDist)
					{
						best = neighbor;
						bestDist = dist;
						queue.Enqueue(neighbor);
					}
				}
			}
		}

		return best;
	}
}

public abstract class ExecutingHnswTable<D, E> where E : HnswNode<D>
{
	public abstract HnswTable<D, E>			Table { get; }
	public List<E>							EntryPoints;
	public FairExecution					Execution;
	public Dictionary<HnswId, E>			Affected = new();

	public abstract  E						Affect(HnswId id);

	protected ExecutingHnswTable(FairExecution execution)
	{
		Execution = execution;
	}

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

			r = Cryptography.Hash(r);
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
			var neighbors = EntryPoints.Count > 0 ? Table.EfSearch(node.Data, EntryPoints[0], l, Table.EfConstruction, 0, null, Find) : [];

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
}
