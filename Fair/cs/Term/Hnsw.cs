using System.Collections;
using System.Collections.Generic;

namespace Uccs.Fair;

public interface IMetric<T>
{
    ulong	Hashify(T data);
    int		ComputeDistance(ulong a, ulong b);
}

public abstract class HnswNode : ITableEntry, IBinarySerializable
{
	public HnswId									Id { get; set; }
	public ulong									Hash { get; set; }
	public SortedDictionary<int, HnswId[]>			Connections { get; set; }
	
	public byte										Level => Id.Level;
	public BaseId									Key => Id;
	public bool										Deleted { get;  set; }

	public abstract void							Cleanup(Round lastInCommit);
	public abstract void							ReadMain(BinaryReader r);
	public abstract void							WriteMain(BinaryWriter r);

	public void AddConnection(int level, HnswNode node)
	{
		if(!Connections.ContainsKey(level))
			Connections[level] = [];

		Connections = new(Connections);
		Connections[level] = [..Connections[level], node.Id];
	}

	public virtual void Write(BinaryWriter writer)
	{
		writer.Write(Id);
		writer.Write(Hash);
		writer.Write(Connections, i => writer.Write((byte)i), i => writer.Write(i));
	}

	public virtual void Read(BinaryReader reader)
	{
		Id			= reader.Read<HnswId>();
		Hash		= reader.ReadUInt64();
		Connections = reader.ReadSortedDictionary(() => (int)reader.ReadByte(), () => reader.ReadArray<HnswId>());
	}
}

public abstract class HnswTable<D, E> : Table<E> where E : HnswNode
{
	protected IMetric<D>						Metric;
	readonly int								MaxLevel;
	readonly int								MaxConnections;
	readonly int								EfConstruction;
	readonly int								Threshold;
	readonly int								MinDiversity;

	List<E>										ConfirmedEntryPoints = new();
	List<E>										EntryPoints = new();

	public abstract ulong						Hashify(D data);
	public abstract E							Affect(HnswId id, FairExecution execution);

	public class LevelEnumerator : IEnumerator<E>
	{
		public E				Current => Entity.Current;
		object					IEnumerator.Current => Entity.Current;

		byte					Level;
		HnswTable<D, E>			Table;
		HashSet<E>				Unique = new HashSet<E>(EqualityComparer<E>.Create((a, b) => a.Id == b.Id, i => i.Id.GetHashCode()));
		IEnumerator<E>			Execution;
		IEnumerator<Round>		Round;
		IEnumerator<E>			Entity;
		IEnumerator<Bucket>		Bucket;
		IEnumerator<Cluster>	Cluster;

		public LevelEnumerator(HnswTable<D, E> table, byte level, Execution execution)
		{
			Table = table;
			Level = level;

			Execution = execution.Round.AffectedByTable<HnswId, E>(table).Values.GetEnumerator();
			Round = Table.Mcv.Tail.GetEnumerator();
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
				if(Execution != null)
				{
					if(Execution.MoveNext())
					{
						if(Execution.Current.Level != Level)
							continue;
						
						Unique.Add(Execution.Current);
						return true;
					}
					else
						Execution = null;
				}
				else if(Round != null)
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

		ConfirmedEntryPoints.Clear();

		foreach(var r in Mcv.Tail.Where(i => i.Confirmed))
			foreach(var i in (r.AffectedByTable(this) as IDictionary<HnswId, E>).Values)
				if(i.Level > maxlevel)
					maxlevel = i.Level;

		foreach(var c in Clusters)
			if(HnswId.ClusterToLevel(c.Id) > maxlevel)
				maxlevel = HnswId.ClusterToLevel(c.Id);

		foreach(var c in Clusters.Where(i => HnswId.ClusterToLevel(i.Id) == maxlevel))
			foreach(var b in c.Buckets)
				ConfirmedEntryPoints.AddRange(b.Entries);

		foreach(var r in Mcv.Tail.Where(i => i.Confirmed))
			foreach(var i in (r.AffectedByTable(this) as IDictionary<HnswId, E>).Values)
				if(i.Level == maxlevel && !ConfirmedEntryPoints.Any(j => j.Id == i.Id))
					ConfirmedEntryPoints.Add(i);
	}

	public override void StartExecution(Execution execution)
	{
		//Execution = execution;

		EntryPoints = new(ConfirmedEntryPoints);
	}

	public E Find(HnswId id, Execution execution)
 	{
		if(execution != null)
 			if(execution.Round.AffectedByTable<HnswId, E>(this).TryGetValue(id, out var a))
 				return a;
 
		var ridmax = execution?.Round.Id - 1 ?? Mcv.LastConfirmedRound.Id;

  		foreach(var i in Mcv.Tail.Where(i => i.Id <= ridmax))
			if(i.AffectedByTable<HnswId, E>(this).TryGetValue(id, out var r) && !r.Deleted)
    			return r;
	
		return Find(id);
 	}

	public EntityEnumeration GetLevel(byte level, Execution execution)
	{
		return new EntityEnumeration(() => new LevelEnumerator(this, level, execution));
	}

	public byte RandomLevel(byte[] start)
	{
		var r = start;
		
		uint p = 24109; /// 2^16 * 0.367879 => double p = 1.0 / Math.E;
		byte level = 0;

		while(((r[1]<<8 | r[0])) < p && level < MaxLevel)
		{	
			level++;

			r = Cryptography.Hash(r);
		}

		return level;
	}

	public void Add(E node, FairExecution execution)
	{
		for(byte l = 0; l <= node.Level; l++)
		{
			var neighbors = EntryPoints.Count > 0
				? EfSearch(node.Hash, EntryPoints[0], l, EfConstruction, null, execution)
				: new();

			if(neighbors.Count == 0)
			{
				var fallback = GlobalBestNeighbor(node, l, execution);
				
				if(fallback != null)
					neighbors.Add((fallback, Metric.ComputeDistance(node.Hash, fallback.Hash)));
			}

			var topNeighbors = ApplyEFHeuristic(neighbors, MaxConnections);

			foreach(var i in topNeighbors)
			{
				var neighbor = Affect(i.Id, execution);

				node.AddConnection(l, neighbor);
				neighbor.AddConnection(l, node);
			}
		}

		if(EntryPoints.Count == 0 || node.Level > EntryPoints[0].Level)
		{
			EntryPoints.Clear();
			EntryPoints.Add(node);
		}
	}

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

	public List<E> Search(D query, int k, Func<E, bool> criteria, int efSearch = 32)
	{
		if(EntryPoints.Count == 0)
			return new();

		var queryHash = Metric.Hashify(query);
		var current = EntryPoints[0];

		for(var level = current.Level; level >= 1; level--)
			current = SearchBestNeighbor(queryHash, current, level);

		var resultNodes = EfSearch(queryHash, current, 0, efSearch, criteria, null);

		return resultNodes	.Where(i => criteria(i.node))
							.OrderBy(x => x.dist)
							.Take(k)
							.Select(x => x.node)
							.ToList();
	}

	private List<(E node, int dist)> EfSearch(ulong queryHash, E entry, int level, int ef, Func<E, bool> criteria, Execution execution)
	{
		var visited = new HashSet<HnswId>();
		var candidates = new PriorityQueue<E, int>();

		// Сортировка по расстоянию (меньше — лучше)
		var topCandidates = new SortedSet<(int dist, E node)>(Comparer<(int dist, E node)>.Create((a, b) =>	{
																												int cmp = a.dist.CompareTo(b.dist);
																												if(cmp != 0) return cmp;

																												// Учитываем Id, чтобы избежать конфликтов
																												return a.node.Id.CompareTo(b.node.Id);
																											}));

		int entryDist = Metric.ComputeDistance(queryHash, entry.Hash);
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

				var neighbor = Find(neighborId, execution);

				if(criteria != null && !criteria(neighbor))
					continue;

				int dist = Metric.ComputeDistance(queryHash, neighbor.Hash);

				var candidate = (dist, neighbor);
				topCandidates.Add(candidate);
				candidates.Enqueue(neighbor, dist);

				// Обрезаем, если превысили ef
				if(topCandidates.Count > ef)
					topCandidates.Remove(topCandidates.Max); // Удаляем наихудшего
			}
		}

		// Возвращаем как список
		return topCandidates.Select(x => (x.node, x.dist)).ToList();
	}

	private List<E> ApplyEFHeuristic(List<(E node, int dist)> candidates, int maxConnections)
	{
		var selected = new List<E>();

		foreach(var (candidate, _) in candidates.OrderBy(x => x.dist))
		{
			bool diverse = true;

			// Проверяем на разнообразие
			foreach(var existing in selected)
			{
				if(Metric.ComputeDistance(candidate.Hash, existing.Hash) < MinDiversity)
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

	private E? GlobalBestNeighbor(HnswNode node, byte level, Execution execution)
	{
// 		if(!Layers.ContainsKey(level))
// 			return null;
// 
		int bestDist = int.MaxValue;
		E? best = null;

		foreach(var n in GetLevel(level, execution))
		{
			int dist = Metric.ComputeDistance(node.Hash, n.Hash);

			if(dist < bestDist)
			{
				bestDist = dist;
				best = n;
			}
		}

		return best;
	}

	private E SearchBestNeighbor(ulong queryHash, E start, int level)
	{
		var visited = new HashSet<HnswId>();
		var queue = new Queue<E>();
		queue.Enqueue(start);

		E best = start;
		int bestDist = Metric.ComputeDistance(queryHash, best.Hash);

		while(queue.Count > 0)
		{
			var current = queue.Dequeue();
			visited.Add(current.Id);

			foreach(var i in current.Connections.GetValueOrDefault(level, []))
			{
				if(!visited.Contains(i))
				{
					var neighbor = Find(i);

					int dist = Metric.ComputeDistance(queryHash, neighbor.Hash);

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
