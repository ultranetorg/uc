using System.Collections;
using System.Collections.Generic;
using System.Text;
using RocksDbSharp;

namespace Uccs.Fair;

public interface IMetric<D>
{
    int		ComputeDistance(D a, D b);
}

public abstract class HnswNode<D> : ITableEntry, IBinarySerializable
{
	public HnswId									Id { get; set; }
	public SortedDictionary<int, HnswId[]>			Connections { get; set; }

	public abstract D								Data { get; }
	public byte										Level => Id.Level;
	public EntityId									Key => Id;
	public bool										Deleted { get;  set; }

	bool											ConnectionsCloned = false;
	
	public abstract object							Clone();

	public override string ToString()
	{
		return $"{Id}, {Data}, Level={Id.Level}, Connections={Connections.Count}";
	}

	public virtual void WriteMain(Writer writer)
	{
		Write(writer);
	}

	public virtual void ReadMain(Reader reader)
	{
		Read(reader);
	}

	public virtual void Cleanup(Round lastInCommit)
	{
	}

	public void AddConnection(int level, HnswNode<D> node)
	{
		if(!ConnectionsCloned)
		{	
			Connections = new(Connections);
			ConnectionsCloned = true;
		}

		if(Connections.ContainsKey(level))
			Connections[level] = [..Connections[level], node.Id];
		else
			Connections[level] = [node.Id];
	}

	public virtual void Write(Writer writer)
	{
		writer.Write(Id);
		//writer.Write(Hash);
		writer.Write(Connections, i => writer.Write((byte)i), i => writer.Write(i));
	}

	public virtual void Read(Reader reader)
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
	public readonly int							SearchThreshold;
	public readonly int							MinDiversity;

	public new HnswTableState<D, E>				Assosiated => base.Assosiated as HnswTableState<D, E>;

	public override bool						IsIndex => true;
	public new FairMcv							Mcv => base.Mcv as FairMcv;
	public IEnumerable<FairRound>				Tail => Mcv.Tail.Cast<FairRound>();

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

	public HnswTable(Mcv mcv, IMetric<D> metric, int maxLevel = 1 << HnswId.LevelBits, int maxConnections = 5, int efConstruction = 64, int searchthreshold = 200, int minDiversity = 100) : base(mcv)
	{
		Metric = metric;
		MaxLevel = maxLevel;
		MaxConnections = maxConnections;
		EfConstruction = efConstruction;
		SearchThreshold = searchthreshold;
		MinDiversity = minDiversity;
	}

	public override void Commit(WriteBatch batch, IEnumerable<ITableEntry> entities, TableStateBase assosiated, Round lastInCommit)
	{
		base.Commit(batch, entities, assosiated, lastInCommit);
	}

	public IEnumerable<E> Search(D query, int skip, int take, Func<E, bool> criteria, Func<HnswId, E> latest)
	{
		var entrypoints = latest(HnswId.Entry)?.Connections[-1].Select(i => latest(i)).ToArray();

		if(entrypoints == null)
			return [];

		//var queryHash = Metric.Hashify(query);
		var current = entrypoints[0];

		for(var level = current.Level; level >= 1; level--)
			current = SearchBestNeighbor(query, current, level, latest);

		var resultNodes = EfSearch(query, current, 0, (skip + take) * 2, criteria, latest);

		return resultNodes.Skip(skip).Take(take);
	}

	public IEnumerable<E> EfSearch(D query, E entry, int level, int ef, Func<E, bool> criteria, Func<HnswId, E> find)
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
		
		if(criteria == null || criteria(entry))
			topCandidates.Add((entryDist, entry));

		while(candidates.Count > 0 && topCandidates.Count < ef)
		{
			candidates.TryDequeue(out var current, out _);

			foreach(var id in current.Connections.GetValueOrDefault(level, []))
			{
				if(visited.Contains(id))
					continue;

				visited.Add(id);

				var neighbor = find(id);

				int dist = Metric.ComputeDistance(query, neighbor.Data);

				if(dist > SearchThreshold)
					continue;

				var candidate = (dist, neighbor);
				candidates.Enqueue(neighbor, dist);

				if(criteria == null || criteria(neighbor))
					topCandidates.Add(candidate);

				// Обрезаем, если превысили ef
				while(topCandidates.Count > ef)
					topCandidates.Remove(topCandidates.Max); // Удаляем наихудшего
			}
		}

		return topCandidates.Select(x => x.node);
	}

	E SearchBestNeighbor(D query, E start, int level, Func<HnswId, E> find)
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

public class HnswTableState<D, E> : TableState<HnswId, E> where E : HnswNode<D>
{
	public new HnswTable<D, E>		Table => base.Table as HnswTable<D, E>;

	public HnswTableState(HnswTable<D, E> table) : base(table)
	{
	}
}
