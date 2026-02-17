using System.Collections;
using RocksDbSharp;

namespace Uccs.Net;

public abstract class TableBase
{
	public const int							BucketsCountMax = 1 << BucketBase.Length;
	public const int							ClustersCountMax = 1 << ClusterBase.Length;
	const int									ClustersCacheLimit = 1000;

	protected ColumnFamilyHandle				ClusterColumn;
	protected ColumnFamilyHandle				BucketColumn;
	protected ColumnFamilyHandle				EntityColumn;
	protected ColumnFamilyHandle				MetaColumn;
	public string								ClusterColumnName	=> Name + nameof(ClusterColumn);
	public string								BucketColumnName	=> Name + nameof(BucketColumn);
	public string								EntityColumnName	=> Name + nameof(EntityColumn);
	public string								MetaColumnName		=> Name + nameof(MetaColumn);
	
	public abstract string						Name { get; }
	public int									Size => Clusters.Sum(i => i.MainLength);
	public byte									Id => (byte)Array.IndexOf(Mcv.Tables, this);
	public virtual bool							IsIndex => false;
	public abstract IEnumerable<ClusterBase>	Clusters { get; }
	protected RocksDb							Rocks;
	protected Mcv								Mcv;

	public abstract ClusterBase					GetCluster(short id);
	public abstract ClusterBase					FindCluster(short id);
	public abstract BucketBase					FindBucket(int id);
	public abstract void						Clear();
	public abstract void						Commit(WriteBatch batch, IEnumerable<ITableEntry> entries, TableStateBase executed, Round lastconfirmedround);
	public virtual void							Index(WriteBatch batch, Round lastincommit){}
	public static short							ClusterFromBucket(int id) => (short)(id >> ClusterBase.Length);

	public abstract class BucketBase
	{
		public const int							Length = 20;

		public int									Id;
		public int									Size;
 		public int									NextI { get; set; }
		public byte[]								Hash { get; set; }
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
		public abstract void						Commit(WriteBatch batch);
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
		public override IEnumerable<E>	Entries =>  _Entries.Select(i => i.Value?.Entity ?? Find(i.Key));
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
				NextI			= r.Read7BitEncodedInt();
				_Entries		= r.ReadSortedDictionary(() => r.Read<ID>(), () => new Item());
			}
		}

		public override string ToString()
		{
			return $"{Id}, Entries={{{_Entries?.Count}}}, Hash={Hash?.ToHex()}, NextE={NextI}";
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
			
			w.Write7BitEncodedInt(NextI); /// hash this too
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
			w.Write7BitEncodedInt(NextI);
			w.Write(_Entries.Keys);

			batch.Put(EntityId.BucketToBytes(Id), s.ToArray(), Table.BucketColumn);
		}

		public override byte[] Export()
		{
			var s = new MemoryStream();
			var w = new BinaryWriter(s);

			w.Write7BitEncodedInt(NextI); /// hash this too
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

			NextI = r.Read7BitEncodedInt();
			var n = r.Read7BitEncodedInt();

			Hash = Cryptography.Hash(data[..(int)s.Position]);
			Size = 0;

			_Entries.Clear();

			for(int i=0; i<n; i++)
			{
				var id = r.Read<ID>();
				var main = r.ReadBytes();

				_Entries[id] = new Item {Main = main};

				batch.Put(id.Raw, main, Table.EntityColumn);

				Hash = Cryptography.Hash(Hash, main);
				Size += main.Length;
			}
			
			s = new MemoryStream();
			var w = new BinaryWriter(s);

			w.Write(Hash);
			w.Write7BitEncodedInt(Size);
			w.Write7BitEncodedInt(NextI);
			w.Write(_Entries.Keys);

			batch.Put(EntityId.BucketToBytes(Id), s.ToArray(), Table.BucketColumn);
		}
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

		public override void Commit(WriteBatch batch)
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
		public E				Current => Entity.Current;
		object					IEnumerator.Current => Entity.Current;

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

	public override List<Cluster>	Clusters { get; }
	protected TableStateBase		Assosiated;
	public EntityEnumeration		GraphEntities => new (() => new GraphEnumerator(this));
	public EntityEnumeration		TailGraphEntities => new (() => new TailGraphEnumerator(this));

	public abstract E				Create();
	public virtual TableStateBase	CreateAssosiated() => new TableState<ID, E>(this);

	public Table(Mcv chain)
	{
		Mcv = chain;
		Rocks = Mcv.Rocks;
		Clusters = new List<Cluster>();

		if(!Rocks.TryGetColumnFamily(MetaColumnName,	out MetaColumn))	MetaColumn		= Rocks.CreateColumnFamily(new (), MetaColumnName);
		if(!Rocks.TryGetColumnFamily(ClusterColumnName,	out ClusterColumn))	ClusterColumn	= Rocks.CreateColumnFamily(new (), ClusterColumnName);
		if(!Rocks.TryGetColumnFamily(BucketColumnName,	out BucketColumn))	BucketColumn	= Rocks.CreateColumnFamily(new (), BucketColumnName);
		if(!Rocks.TryGetColumnFamily(EntityColumnName,	out EntityColumn))	EntityColumn	= Rocks.CreateColumnFamily(new (), EntityColumnName);

		using(var i = Rocks.NewIterator(ClusterColumn))
		{
			for(i.SeekToFirst(); i.Valid(); i.Next())
			{
 				var c = new Cluster(this, Cluster.FromBytes(i.Key()));
				//c.Hash = i.Value();
 				Clusters.Add(c);
			}
		}

		Assosiated = CreateAssosiated();
		
		var s = Rocks.Get([0], MetaColumn);
		
		if(s != null)
		{
			var r = new BinaryReader(new MemoryStream(s));
			Assosiated.Read(r);
		}
	}

	public override void Commit(WriteBatch batch, IEnumerable<ITableEntry> entities, TableStateBase executed,  Round lastInCommit)
	{
		if(!entities.Any())
			return;

		var bs = new HashSet<Bucket>();
		var cs = new HashSet<Cluster>();

		foreach(var i in entities.Cast<E>())
		{
			var c = GetCluster(ClusterFromBucket(i.Key.B));
			var b = c.GetBucket(i.Key.B);

			if(!i.Deleted)
				b.Add(batch, i);
			else
				b.Remove(batch, i.Key as ID);

			if(i.Key is AutoId id)
				if(b.NextI < id.I + 1)
					b.NextI = id.I + 1;

			bs.Add(b);
			cs.Add(c);
		}

		foreach(var i in bs)
		{
			i.Cleanup(lastInCommit);
			i.Commit(batch);
		}

		foreach(var i in cs)
			i.Commit(batch);

		if(executed != null) /// null == Account
		{
			Assosiated = executed;
	
			var s = new MemoryStream();
			var w = new BinaryWriter(s);
			
			Assosiated.Write(w);
		
			batch.Put(new byte[]{0}, s.ToArray(), MetaColumn);
		}
	}

	public override void Clear()
	{
		Clusters.Clear();

		Rocks.DropColumnFamily(MetaColumnName);
		Rocks.DropColumnFamily(ClusterColumnName);
		Rocks.DropColumnFamily(BucketColumnName);
		Rocks.DropColumnFamily(EntityColumnName);

		MetaColumn		= Rocks.CreateColumnFamily(new (), MetaColumnName);
		ClusterColumn	= Rocks.CreateColumnFamily(new (), ClusterColumnName);
		BucketColumn	= Rocks.CreateColumnFamily(new (), BucketColumnName);
		EntityColumn	= Rocks.CreateColumnFamily(new (), EntityColumnName);
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
	}

	public virtual E Latest(ID id)
	{
		if(Mcv.LastConfirmedRound.AffectedByTable<ID, E>(this).TryGetValue(id, out var e))
			return e.Deleted ? null : e;

		return Find(id);
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

