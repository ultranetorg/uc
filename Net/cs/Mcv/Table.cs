using System.Collections;
using RocksDbSharp;

namespace Uccs.Net;

public abstract class TableBase
{
	public const int							BucketsCountMax = 1 << BucketBase.Length;
	public const int							ClustersCountMax = 1 << ClusterBase.Length;
	const int									ClustersCacheLimit = 1000;

	protected ColumnFamilyHandle				ClusterColumn;
	protected ColumnFamilyHandle				StateColumn;
	protected ColumnFamilyHandle				BucketColumn;
	protected ColumnFamilyHandle				EntityColumn;
	//protected ColumnFamilyHandle				MoreColumn;
	protected RocksDb							Rocks;
	protected Mcv								Mcv;
	public abstract int							Size { get; }
	public byte									Id => (byte)Array.IndexOf(Mcv.Tables, this);
	public abstract string						Name { get; }
	public virtual bool							IsIndex => false;
	public abstract IEnumerable<ClusterBase>	Clusters { get; }

	public abstract ClusterBase					GetCluster(short id);
	public abstract ClusterBase					FindCluster(short id);
	public abstract BucketBase					FindBucket(int id);
	public abstract void						Clear();
	public abstract void						Commit(WriteBatch batch, IEnumerable<ITableEntry> entries, ITableState executed, Round lastconfirmedround);
	public static short							ClusterFromBucket(int id) => (short)(id >> ClusterBase.Length);
	public virtual void							Index(WriteBatch batch){}

	public abstract class BucketBase
	{
		public const int							Length = 20;

		public int									Id;
		public short								SuperId => (short)(Id >> Length);
		public int									Size;
 		public int									NextE { get; set; }
		public byte[]								Hash { get; set; }
		//public abstract byte[]						Main { get; }
		public abstract IEnumerable<ITableEntry>	Entries { get; }

		public abstract void						Commit(WriteBatch batch);
		public abstract void						Import(WriteBatch batch, byte[] main);
		public abstract byte[]						Export();

		public void Cleanup(Round lastInCommit)
		{
			foreach(var i in Entries)
			{
				i.Cleanup(lastInCommit);
			}
		}
	}

	public abstract class ClusterBase
	{
		public const int							Length = 10;

		public short								Id;
		public byte[]								Hash;
		public int									MainLength;
		public abstract IEnumerable<BucketBase>		Buckets { get; }

		public abstract BucketBase					GetBucket(int id);
		public abstract void						Save(WriteBatch batch);
	}
}

public abstract class Table<ID, E> : TableBase where E : class, ITableEntry where ID : EntityId, new()
{
	public class Bucket : BucketBase
	{
		class Item
		{
			public E		Entity;
			public byte[]	Main;

			public Item()
			{
			}
		}

		Table<ID, E>					Table;
		public override IEnumerable<E>	Entries => _Entries.Select(i => i.Value?.Entity ?? Find(i.Key));
		SortedDictionary<ID, Item>		_Entries = new SortedDictionary<ID, Item>();

		public Bucket(Table<ID, E> table, int id)
		{
			Table = table;
			Id = id;

			var meta = Table.Rocks.Get(EntityId.BucketToBytes(Id), Table.BucketColumn);

			if(meta != null)
			{
				var r = new BinaryReader(new MemoryStream(meta));
	
				Hash			= r.ReadHash();
				Size			= r.Read7BitEncodedInt();
				NextE			= r.Read7BitEncodedInt();
				_Entries		= r.ReadSortedDictionary(() => r.Read<ID>(), () => new Item());
			}
		}


		public override string ToString()
		{
			return $"{Id}, Entries={{{_Entries?.Count}}}, Hash={Hash?.ToHex()}, NextE={NextE}";
		}

		public E Find(ID id)
		{
			if(!_Entries.TryGetValue(id, out var e))
				return null;

			if(e.Entity != null)
				return e.Entity;

			e.Main ??= Table.Rocks.Get(id.Raw, Table.EntityColumn);
			e.Entity = Table.Create();
			e.Entity.ReadMain(new BinaryReader(new MemoryStream(e.Main)));

			return e.Entity;
		}

		public void Add(WriteBatch batch, E entity)
		{
			_Entries[entity.Key as ID] = new Item {Entity = entity};
			batch.Put(entity.Key.Raw, entity.ToMain(), Table.EntityColumn);
		}

		public void Remove(WriteBatch batch, ID id)
		{
			_Entries.Remove(id);
			batch.Delete(id.Raw, Table.EntityColumn);
		}

		public override void Commit(WriteBatch batch)
		{
			var s = new MemoryStream();
			var w = new BinaryWriter(s);
			
			w.Write7BitEncodedInt(NextE); /// hash this too
			w.Write7BitEncodedInt(_Entries.Count);

			Hash = Cryptography.Hash(s.ToArray());
			Size = 0;

			foreach(var i in _Entries)
			{
				if(i.Value.Main == null)
				{	
					if(i.Value.Entity == null)
						i.Value.Main = Table.Rocks.Get(i.Key.Raw, Table.EntityColumn);
					else
					{
						i.Value.Main = i.Value.Entity.ToMain();
						batch.Put(i.Key.Raw, i.Value.Main, Table.EntityColumn);
					}
				}

				Hash = Cryptography.Hash(Hash, i.Value.Main);
				Size += i.Value.Main.Length;
			}

			s.Position = 0;

			w.Write(Hash);
			w.Write7BitEncodedInt(Size);
			w.Write7BitEncodedInt(NextE);
			w.Write(_Entries.Keys);

			batch.Put(EntityId.BucketToBytes(Id), s.ToArray(), Table.BucketColumn);
		}

		public override byte[] Export()
		{
			var s = new MemoryStream();
			var w = new BinaryWriter(s);

			w.Write7BitEncodedInt(NextE); /// hash this too
			w.Write7BitEncodedInt(_Entries.Count);

			foreach(var i in _Entries)
			{
				w.Write(i.Key);

				if(i.Value.Main == null)
				{	
					if(i.Value.Entity == null)
						i.Value.Main = Table.Rocks.Get(i.Key.Raw, Table.EntityColumn);
					else
						i.Value.Main = i.Value.Entity.ToMain();
				}

				w.WriteBytes(i.Value.Main);
			}

			return s.ToArray();
		}

		public override void Import(WriteBatch batch, byte[] data)
		{
			var s = new MemoryStream(data);
			var r = new BinaryReader(s);

			NextE = r.Read7BitEncodedInt();
			var n = r.Read7BitEncodedInt();

			Hash = Cryptography.Hash(data[..(int)s.Position]);
			Size = 0;

			for(int i=0; i<n; i++)
			{
				//var e = new Item();

				var id = r.Read<ID>();
				var main = r.ReadBytes();
				//e.Entity = Table.Create();
				//e.Entity.ReadMain(new BinaryReader(new MemoryStream(e.Raw)));

				_Entries[id] = new Item {Main = main};

				batch.Put(id.Raw, main, Table.EntityColumn);

				Hash = Cryptography.Hash(Hash, main);
				Size += main.Length;
			}
			
			s = new MemoryStream();
			var w = new BinaryWriter(s);

			w.Write(Hash);
			w.Write7BitEncodedInt(Size);
			w.Write7BitEncodedInt(NextE);
			w.Write(_Entries.Keys);

			batch.Put(EntityId.BucketToBytes(Id), s.ToArray(), Table.BucketColumn);
		}

// 		void Load()
// 		{
// 			if(Main != null)
// 			{
// 				var s = new MemoryStream(Main);
// 				var r = new BinaryReader(s);
// 	
// 				_NextEntryId = r.Read7BitEncodedInt();
// 
// 				var a = r.ReadList(()=>	{ 
// 											var e = Table.Create();
// 											e.ReadMain(r);
// 											return e;
// 										});
// 					
// 				_Entries = a;
// 			} 
// 			else
// 			{
// 				_NextEntryId = 0;
// 				_Entries = new List<E>();
// 			}
// 		}

// 		public override void Save(WriteBatch batch)
// 		{
// 			var s = new MemoryStream();
// 			var w = new BinaryWriter(s);
// 
// 			///if(_Main == null || base.Entries != null)
// 			///{
// 			///
// 				_Entries = Entries.OrderBy(i => i.Key).ToList();
// 
// 				w.Write7BitEncodedInt(NextEid);
// 				w.Write(_Entries, i =>	{
// 											i.WriteMain(w);
// 										});
// 
// 				_Main = s.ToArray();
// 			///}
// 
// 			batch.Put(EntityId.BucketToBytes(Id), _Main, Table.EntityColumn);
// 
// 			Hash = Cryptography.Hash(_Main);
// 			Size = _Main.Length;
// 			
// 			s.SetLength(0);
// 
// 			w.Write(Hash);
// 			w.Write7BitEncodedInt(_Main.Length);
// 
// 			batch.Put(EntityId.BucketToBytes(Id), s.ToArray(), Table.BucketColumn);
// 		}
// 
// 		public override void Save(WriteBatch batch, byte[] main)
// 		{
// 			_Main = main;
// 
// 			batch.Put(EntityId.BucketToBytes(Id), _Main, Table.EntityColumn);
// 
// 			Hash = Cryptography.Hash(_Main);
// 			Size = _Main.Length;
// 
// 			var s = new MemoryStream();
// 			var w = new BinaryWriter(s);
// 			
// 			w.Write(Hash);
// 			w.Write7BitEncodedInt(Size);
// 
// 			batch.Put(EntityId.BucketToBytes(Id), s.ToArray(), Table.BucketColumn);
// 		}
	}


	public class Cluster : ClusterBase
	{
		Table<ID, E>						Table;
		List<Bucket>						_Buckets;

		public static byte[]				ToBytes(short id) => [(byte)id, (byte)(id >> 8)];
		public static short					FromBytes(byte[] id) => (short)(id[0] | id[1] << 8);

		public override List<Bucket> Buckets
		{ 
			get
			{
				if(_Buckets == null)
				{
					var m = Table.Rocks.Get(ToBytes(Id), Table.ClusterColumn);

					if(m != null)
					{
						var r = new BinaryReader(new MemoryStream(m));
	
						r.ReadHash();
						r.Read7BitEncodedInt();
						_Buckets = r.ReadList(() => new Bucket(Table, r.Read7BitEncodedInt()));
					}
					else
						_Buckets = [];
				}

				return _Buckets;
			}
		}

		public Cluster(Table<ID, E> table, short id)
		{
			Table = table;
			Id = id;

			var m = Table.Rocks.Get(ToBytes(Id), Table.ClusterColumn);

			if(m != null)
			{
				var r = new BinaryReader(new MemoryStream(m));

				Hash		= r.ReadHash();
				MainLength	= r.Read7BitEncodedInt();
			}
		}
		
		public override string ToString()
		{
			return $"{Id}, Buckets={{{Buckets?.Count}}}, Hash={Hash?.ToHex()}";
		}

		public override Bucket GetBucket(int id)
		{
			var c = FindBucket(id);

			if(c != null)
				return c;

			c = new Bucket(Table, id);
			Buckets.Add(c);

			//Recycle();

			return c;
		}

		public Bucket FindBucket(int id)
		{
			return Buckets.Find(i => i.Id == id);
		}

		public override void Save(WriteBatch batch)
		{
			var buckets = Buckets.OrderBy(i => i.Id).ToArray();

			Hash = buckets.First().Hash;
			MainLength = buckets.First().Size;

			foreach(var i in buckets.Skip(1))
			{
				Hash = Cryptography.Hash(Hash, i.Hash);
				MainLength += i.Size;
			}

			var s = new MemoryStream();
			var w = new BinaryWriter(s);
			
			w.Write(Hash);
			w.Write7BitEncodedInt(MainLength);
			w.Write(buckets, i => w.Write7BitEncodedInt(i.Id));

			batch.Put(ToBytes(Id), s.ToArray(), Table.ClusterColumn);
		}
	}

	public class TailGraphEnumerator : IEnumerator<E>
	{
		public E				Current => Entity.Current;
		object					IEnumerator.Current => Entity.Current;

		HashSet<E>				Unique = new HashSet<E>(EqualityComparer<E>.Create((a, b) => a.Key == b.Key, i => i.Key.GetHashCode()));
		IEnumerator<Round>		Round;
		IEnumerator<E>			Entity;
		Table<ID, E>			Table;

		public TailGraphEnumerator(Table<ID, E> table)
		{
			Table = table;

			Round = Table.Mcv.Tail.GetEnumerator();
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
							Entity = Table.GraphEntities.GetEnumerator();
							continue;
						}
						else
							while(!Round.Current.Confirmed)
								Round.MoveNext();
								
						Entity = Round.Current.AffectedByTable(Table).Values.GetEnumerator() as IEnumerator<E>;
					}
	
					if(Entity.MoveNext())
					{
						if(!Unique.Contains(Entity.Current))
						{
							Unique.Add(Entity.Current);
							return true;
						}
						else
							continue;
					}
					else
					{
						Entity = null;
					}
				} 
				else
				{
					if(Entity.MoveNext())
					{
						if(!Unique.Contains(Entity.Current))
						{
							Unique.Add(Entity.Current);
							return true;
						}
						else
							continue;
					}
					else
					{
						return false;
					}
				}
			}
		}

		public void Reset()
		{
			throw new NotImplementedException();
		}
	}

	public class GraphEnumerator : IEnumerator<E>
	{
		public E Current => Entity.Current;
		object IEnumerator.Current => Entity.Current;

		IEnumerator<Cluster>	Cluster;
		IEnumerator<Bucket>		Bucket;
		IEnumerator<E>			Entity;
		Table<ID, E>			Table;

		public GraphEnumerator(Table<ID, E> table)
		{
			Table = table;

			Cluster = Table.Clusters.GetEnumerator();
		}

		public void Dispose()
		{
			Cluster?.Dispose();
			Bucket?.Dispose();
			Entity?.Dispose();
		}

		public bool MoveNext()
		{
			while(true)
			{
				if(Bucket == null)
				{	
					if(!Cluster.MoveNext())
						return false;

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
					return true;
				}
				else
				{
					Entity = null;
				}
			}
		}

		public void Reset()
		{
			throw new NotImplementedException();
		}
	}

	public class BinaryComparer : IComparer<E>
	{
		Func<E, int> Labda;

		public BinaryComparer(Func<E, int> comparer)
		{
			Labda = comparer;
		}

		public int Compare(E x, E y)
		{
			return Labda(x);
		}
	}

	public class EntityEnumeration : IEnumerable<E>
	{
		Func<IEnumerator<E>> Create;

		public EntityEnumeration(Func<IEnumerator<E>> create)
		{
			Create = create;
		}

		public IEnumerator<E> GetEnumerator()
		{
			return Create();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return Create();
		}
	}

	public override string			Name => typeof(E).Name.Replace("Entry", null);

	public override List<Cluster>	Clusters { get; }
	public override int				Size => Clusters.Sum(i => i.MainLength);
	public static string			StateColumnName		=> typeof(E).Name.Replace("Entry", null) + nameof(StateColumn);
	public static string			ClusterColumnName	=> typeof(E).Name.Replace("Entry", null) + nameof(ClusterColumn);
	public static string			MetaColumnName		=> typeof(E).Name.Replace("Entry", null) + nameof(BucketColumn);
	public static string			MainColumnName		=> typeof(E).Name.Replace("Entry", null) + nameof(EntityColumn);

	public abstract E				Create();

	public EntityEnumeration		GraphEntities => new (() => new GraphEnumerator(this));
	public EntityEnumeration		TailGraphEntities => new (() => new TailGraphEnumerator(this));

	public Table(Mcv chain)
	{
		Mcv = chain;
		Rocks = Mcv.Rocks;
		Clusters = new List<Cluster>();

		if(!Rocks.TryGetColumnFamily(StateColumnName,	out StateColumn))	StateColumn		= Rocks.CreateColumnFamily(new (), StateColumnName);
		if(!Rocks.TryGetColumnFamily(ClusterColumnName,	out ClusterColumn))	ClusterColumn	= Rocks.CreateColumnFamily(new (), ClusterColumnName);
		if(!Rocks.TryGetColumnFamily(MetaColumnName,	out BucketColumn))	BucketColumn		= Rocks.CreateColumnFamily(new (), MetaColumnName);
		if(!Rocks.TryGetColumnFamily(MainColumnName,	out EntityColumn))	EntityColumn		= Rocks.CreateColumnFamily(new (), MainColumnName);

		using(var i = Rocks.NewIterator(ClusterColumn))
		{
			for(i.SeekToFirst(); i.Valid(); i.Next())
			{
 				var c = new Cluster(this, Cluster.FromBytes(i.Key()));
				//c.Hash = i.Value();
 				Clusters.Add(c);
			}
		}
	}

	public override void Clear()
	{
		Clusters.Clear();

		Rocks.DropColumnFamily(StateColumnName);
		Rocks.DropColumnFamily(ClusterColumnName);
		Rocks.DropColumnFamily(MetaColumnName);
		Rocks.DropColumnFamily(MainColumnName);

		StateColumn		= Rocks.CreateColumnFamily(new (), StateColumnName);
		ClusterColumn	= Rocks.CreateColumnFamily(new (), ClusterColumnName);
		BucketColumn	= Rocks.CreateColumnFamily(new (), MetaColumnName);
		EntityColumn	= Rocks.CreateColumnFamily(new (), MainColumnName);
	}

	public override Cluster FindCluster(short id)
	{
		return Clusters.Find(i => i.Id == id);
	}

	public override Cluster GetCluster(short id)
	{
		var c = FindCluster(id);

		if(c != null)
			return c;

		c = new Cluster(this, id);
		Clusters.Add(c);

		//Recycle();

		return c;
	}

	public override Bucket FindBucket(int id)
	{
		var cid = ClusterFromBucket(id);
		var c = Clusters.Find(i => i.Id == cid);

		return c?.FindBucket(id);
	}
	
	public virtual E Find(ID id)
	{
		return FindBucket(id.B)?.Find(id);
		//var j = eee?.BinarySearch(null, new BinaryComparer(x => x.Key.CompareTo(id)));
		//
		//return j >= 0 ? eee[j.Value] : null;

		//return FindBucket(id.B)?.Entries.Find(i => ((EntityId)i.Key).E == id.E);
	}

	public virtual E Find(ID id, int ridmax)
	{
  		foreach(var i in Mcv.Tail.Where(i => i.Id <= ridmax))
			if(i.AffectedByTable<ID, E>(this).TryGetValue(id, out var r) && !r.Deleted)
    			return r;

		return Find(id);
	}

	public virtual E Latest(ID id)
	{
		return Find(id, Mcv.LastConfirmedRound.Id);
	}

	void Recycle()
	{
		//if(Clusters.Count > ClustersCacheLimit)
		//{
		//	foreach(var i in Clusters.OrderByDescending(i => i.Entries.Max(i => i.LastAccessed)).Skip(ClustersCacheLimit))
		//	{
		//		Clusters.Remove(i);
		//	}
		//}
	}

	public override void Commit(WriteBatch batch, IEnumerable<ITableEntry> entities, ITableState executed,  Round lastInCommit)
	{
		if(!entities.Any())
			return;

		var bs = new HashSet<Bucket>();
		var cs = new HashSet<Cluster>();

		foreach(var i in entities.DistinctBy(i => i.Key).Cast<E>())
		{
			var c = GetCluster(ClusterFromBucket(i.Key.B));
			var b = c.GetBucket(i.Key.B);

			if(!i.Deleted)
				b.Add(batch, i);
			else
				b.Remove(batch, i.Key as ID);

			if(i.Key is AutoId id)
				if(b.NextE < id.E + 1)
					b.NextE = id.E + 1;

			bs.Add(b);
			cs.Add(c);
		}

		foreach(var i in bs)
		{
			i.Cleanup(lastInCommit);
			i.Commit(batch);
		}

		foreach(var i in cs)
			i.Save(batch);

		//CalculateSuperClusters();
	}

	///public override long MeasureChanges(IEnumerable<Round> tail)
	///{
	///	var affected = new Dictionary<K, E>();
	///
	///	foreach(var r in tail)
	///	{
	///		foreach(var i in r.AffectedByTable(this).Cast<E>())
	///			if(!affected.ContainsKey(i.Key))
	///				affected.Add(i.Key, i);
	///	}
	///
	///
	///	var se = new FakeStream();
	///	var we = new BinaryWriter(se);
	///	we.Write7BitEncodedInt(_Clusters.Count);
	///
	///	var si = new FakeStream();
	///	var wi = new BinaryWriter(si);
	///
	///	int n = 0;
	///
	///	foreach(var i in affected.Values)
	///	{
	///		var c = _Clusters.Find(j => j.Id.SequenceEqual(i.Id.Ci));
	///
	///		var e = c?.Entries.Find(e => e.Id.Ei == i.Id.Ei);
	///		
	///		if(e != null)
	///		{
	///			we.Write7BitEncodedInt(e.Id.Ei);
	///			e.WriteMain(we);
	///		}
	///		else
	///			n++;
	///
	///		wi.Write7BitEncodedInt(i.Id.Ei);
	///		i.WriteMain(wi);
	///	}
	///
	///	wi.Write7BitEncodedInt(_Clusters.Count + n);
	///
	///	return si.Length - se.Length ;
	///}
}

public interface ITableState
{
	void Absorb(ITableState execution);
	void StartRoundExecution(Round round);
}

public class TableState<ID, E> : ITableState where ID : EntityId, new() where E : class, ITableEntry
{
	public Dictionary<ID, E>	Affected = new();
	public Table<ID, E>			Table;

	public TableState(Table<ID, E> table)
	{
		Table = table;
	}

	public virtual void StartRoundExecution(Round round)
	{
		Affected.Clear();
	}

	public virtual void Absorb(ITableState execution)
	{
		var e = execution as TableState<ID, E>;

		foreach(var i in e.Affected)	
			Affected[i.Key] = i.Value;
	}
}

public abstract class TableExecution<ID, E> : TableState<ID, E> where ID : EntityId, new() where E : class, ITableEntry
{
	public Execution	Execution;

	protected TableExecution(Table<ID, E> table, Execution execution) : base(table)
	{
		Execution = execution;
	}
	
	public E Find(ID id)
 	{
 		if(Affected.TryGetValue(id, out var a))
 			return a;
 		
		return Table.Find(id, Execution.Round.Id);
 	}

	public virtual E Affect(ID id)
	{
		if(Affected.TryGetValue(id, out var a))
			return a;
			
		a = Table.Find(id, Execution.Round.Id);

		if(a == null)
			throw new IntegrityException();
		
		return Affected[id] = a.Clone() as E;
	}
}