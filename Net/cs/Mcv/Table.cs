﻿using RocksDbSharp;

namespace Uccs.Net;

public abstract class TableBase
{
	public const int							BucketsCountMax = 4096 * 4096;
	public const int							ClustersCountMax = 4096;
	const int									ClustersCacheLimit = 1000;

	protected ColumnFamilyHandle				ClusterColumn;
	protected ColumnFamilyHandle				MetaColumn;
	protected ColumnFamilyHandle				MainColumn;
	//protected ColumnFamilyHandle				MoreColumn;
	protected RocksDb							Engine;
	protected Mcv								Mcv;
	public abstract int							Size { get; }
	public byte									Id => (byte)Array.IndexOf(Mcv.Tables, this);
	public abstract string						Name { get; }
	public virtual bool							IsIndex => false;
	public abstract IEnumerable<ClusterBase>	Clusters { get; }

	//public abstract BucketBase				AddCluster(short id);
	public abstract ClusterBase					GetCluster(short id);
	public abstract ClusterBase					FindCluster(short id);
	public abstract BucketBase					FindBucket(int id);
	public abstract void						Clear();
	public abstract void						Save(WriteBatch batch, System.Collections.ICollection entities, Round lastconfirmedround);
	public static short							ClusterFromBucket(int id) => (short)(id >> 12);

	public abstract class BucketBase
	{
		public int									Id;
		public short								SuperId => (short)(Id >> 12);
		public int									MainLength;
 		public abstract int							NextEid { get; set; }
		public byte[]								Hash { get; set; }
		public abstract byte[]						Main { get; }
		public abstract IEnumerable<ITableEntry>	Entries { get; }

		public abstract void						Save(WriteBatch batch);
		public abstract void						Save(WriteBatch batch, byte[] main);

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
		public short								Id;
		public byte[]								Hash;
		public int									MainLength;
		public abstract IEnumerable<BucketBase>		Buckets { get; }

		public abstract BucketBase					GetBucket(int id);
		public abstract void						Save(WriteBatch batch);
	}

// 		public void CalculateSuperClusters()
// 		{
// 			if(!Clusters.Any())
// 				return;
// 
// 			var ocs = Clusters.OrderBy(i => i.Id);
// 
// 			var b = ocs.First().SuperId;
// 			byte[] h = ocs.First().Hash;
// 
// 			foreach(var i in ocs.Skip(1))
// 			{
// 				if(b != i.SuperId)
// 				{
// 					SuperClusters[b] = h;
// 					b = i.SuperId;
// 					h = i.Hash;
// 				}
// 				else
// 					h = Cryptography.Hash(h, i.Hash);
// 			}
// 
// 			SuperClusters[b] = h;
// 		}
}

public abstract class Table<E> : TableBase where E : class, ITableEntry
{
	public class Bucket : BucketBase
	{
		Table<E>				Table;
		byte[]					_Main;
		int						_NextEntryId;
		List<E>					_Entries;
		public override byte[]	Main  => _Main ??= Table.Engine.Get(BaseId.BucketToBytes(Id), Table.MainColumn);

		public override List<E> Entries
		{
			get
			{
				if(_Entries == null)
					Load();

				return _Entries;
			}
		}

		public override int NextEid 
		{
			get
			{
				if(_Entries == null)
					Load();

				return _NextEntryId;
			}
			set
			{
				_NextEntryId = value;
			}
		}

		public Bucket(Table<E> table, int id)
		{
			Table = table;
			Id = id;

			var meta = Table.Engine.Get(BaseId.BucketToBytes(Id), Table.MetaColumn);

			if(meta != null)
			{
				var r = new BinaryReader(new MemoryStream(meta));
	
				Hash		= r.ReadHash();
				MainLength	= r.Read7BitEncodedInt();
			}
		}

		public override string ToString()
		{
			return $"{Id}, Entries={{{Entries?.Count}}}, Hash={Hash?.ToHex()}, NextEntryId={NextEid}";
		}

		void Load()
		{
			if(Main != null)
			{
				var s = new MemoryStream(Main);
				var r = new BinaryReader(s);
	
				_NextEntryId = r.Read7BitEncodedInt();

				var a = r.ReadList(()=>	{ 
											var e = Table.Create();
											e.ReadMain(r);
											return e;
										});
					
				_Entries = a;
			} 
			else
			{
				_NextEntryId = 0;
				_Entries = new List<E>();
			}
		}

		public override void Save(WriteBatch batch)
		{
			var s = new MemoryStream();
			var w = new BinaryWriter(s);

			///if(_Main == null || base.Entries != null)
			///{
			///
				_Entries = Entries.OrderBy(i => i.Key).ToList();

				w.Write7BitEncodedInt(NextEid);
				w.Write(_Entries, i =>	{
											i.WriteMain(w);
										});

				_Main = s.ToArray();
			///}

			batch.Put(BaseId.BucketToBytes(Id), _Main, Table.MainColumn);

			Hash = Cryptography.Hash(_Main);
			MainLength = _Main.Length;
			
			s.SetLength(0);

			w.Write(Hash);
			w.Write7BitEncodedInt(_Main.Length);

			batch.Put(BaseId.BucketToBytes(Id), s.ToArray(), Table.MetaColumn);
		}

		public override void Save(WriteBatch batch, byte[] main)
		{
			_Main = main;

			batch.Put(BaseId.BucketToBytes(Id), _Main, Table.MainColumn);

			Hash = Cryptography.Hash(_Main);
			MainLength = _Main.Length;

			var s = new MemoryStream();
			var w = new BinaryWriter(s);
			
			w.Write(Hash);
			w.Write7BitEncodedInt(MainLength);

			batch.Put(BaseId.BucketToBytes(Id), s.ToArray(), Table.MetaColumn);

			Table.IndexBucket(batch, this);
		}
	}


	public class Cluster : ClusterBase
	{
		Table<E>							Table;
		List<Bucket>						_Buckets;

		public static byte[]				ToBytes(short id) => [(byte)id, (byte)(id >> 8)];
		public static short					FromBytes(byte[] id) => (short)(id[0] | id[1] << 8);

		public override List<Bucket> Buckets
		{ 
			get
			{
				if(_Buckets == null)
				{
					var m = Table.Engine.Get(ToBytes(Id), Table.ClusterColumn);

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

		public Cluster(Table<E> table, short id)
		{
			Table = table;
			Id = id;

			var m = Table.Engine.Get(ToBytes(Id), Table.ClusterColumn);

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
			MainLength = buckets.First().MainLength;

			foreach(var i in buckets.Skip(1))
			{
				Hash = Cryptography.Hash(Hash, i.Hash);
				MainLength += i.MainLength;
			}

			var s = new MemoryStream();
			var w = new BinaryWriter(s);
			
			w.Write(Hash);
			w.Write7BitEncodedInt(MainLength);
			w.Write(buckets, i => w.Write7BitEncodedInt(i.Id));

			batch.Put(ToBytes(Id), s.ToArray(), Table.ClusterColumn);
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

	public override string			Name => typeof(E).Name.Replace("Entry", null);

	public override List<Cluster>	Clusters { get; }
	public override int				Size => Clusters.Sum(i => i.MainLength);
	public static string			ClusterColumnName	=> typeof(E).Name.Replace("Entry", null) + nameof(ClusterColumn);
	public static string			MetaColumnName		=> typeof(E).Name.Replace("Entry", null) + nameof(MetaColumn);
	public static string			MainColumnName		=> typeof(E).Name.Replace("Entry", null) + nameof(MainColumn);

	public abstract E				Create();
	public virtual void				IndexBucket(WriteBatch batch, Bucket bucket){}

	public Table(Mcv chain)
	{
		Mcv = chain;
		Engine = Mcv.Rocks;
		Clusters = new List<Cluster>();

		if(!Engine.TryGetColumnFamily(ClusterColumnName, out ClusterColumn))	ClusterColumn	= Engine.CreateColumnFamily(new (), ClusterColumnName);
		if(!Engine.TryGetColumnFamily(MetaColumnName,	 out MetaColumn))		MetaColumn		= Engine.CreateColumnFamily(new (), MetaColumnName);
		if(!Engine.TryGetColumnFamily(MainColumnName,	 out MainColumn))		MainColumn		= Engine.CreateColumnFamily(new (), MainColumnName);

		using(var i = Engine.NewIterator(ClusterColumn))
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

		Engine.DropColumnFamily(ClusterColumnName);
		Engine.DropColumnFamily(MetaColumnName);
		Engine.DropColumnFamily(MainColumnName);

		ClusterColumn	= Engine.CreateColumnFamily(new (), ClusterColumnName);
		MetaColumn		= Engine.CreateColumnFamily(new (), MetaColumnName);
		MainColumn		= Engine.CreateColumnFamily(new (), MainColumnName);
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

	public virtual E Find(EntityId id, int ridmax)
	{
  		foreach(var i in Mcv.Tail.Where(i => i.Id <= ridmax))
			if((i.AffectedByTable(this) as IDictionary<EntityId, E>).TryGetValue(id, out var r) && !r.Deleted)
    			return r;

		var eee = FindBucket(id.B)?.Entries;
		var j = eee?.BinarySearch(null, new BinaryComparer(x => ((EntityId)x.Key).E.CompareTo(id.E)));
		
		return j >= 0 ? eee[j.Value] : null;

		//return FindBucket(id.B)?.Entries.Find(i => ((EntityId)i.Key).E == id.E);
	}

	public virtual E Latest(EntityId id)
	{
		return Find(id, Mcv.LastConfirmedRound.Id);
	}

	public virtual E Find(RawId id, int ridmax)
	{
  		foreach(var i in Mcv.Tail.Where(i => i.Id <= ridmax))
			if((i.AffectedByTable(this) as IDictionary<RawId, E>).TryGetValue(id, out var r) && !r.Deleted)
    			return r;
				
		/// Use binary search!

		return FindBucket(id.B)?.Entries.Find(i => i.Key == id);
	}

	public virtual E Latest(RawId id)
	{
		return Find(id, Mcv.LastConfirmedRound.Id);
	}

///		public class Enumerator : IEnumerator<E>
///		{
///			public E Current => Entity.Current;
///			object IEnumerator.Current => Entity.Current;
///
///			IEnumerator<Cluster>	Cluster;
///			IEnumerator<E>			Entity;
///			Table<E>				Table;
///
///			public Enumerator(Table<E> table)
///			{
///				Table = table;
///
///				Cluster = Table.Clusters.GetEnumerator();
///			}
///
///			public void Dispose()
///			{
///				Cluster?.Dispose();
///				Entity?.Dispose();
///			}
///
///			public bool MoveNext()
///			{
///				start: 
///					if(Entity == null)
///					{
///						if(Cluster.MoveNext())
///						{
///							Entity = Cluster.Current.Entries.GetEnumerator();
///						}
///						else
///							return false;
///					}
///
///					if(Entity.MoveNext())
///					{
///						return true;
///					}
///					else
///					{
///						Entity = null;
///						goto start;
///					}
///			}
///
///			public void Reset()
///			{
///				Entity = null;
///				Cluster = Table.Clusters.GetEnumerator();
///			}
///		}
///
///		public IEnumerator<E> GetEnumerator()
///		{
///			return new Enumerator(this);
///		}
///
///		IEnumerator IEnumerable.GetEnumerator()
///		{
///			return new Enumerator(this);
///		}

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

	public override void Save(WriteBatch batch, System.Collections.ICollection entities, Round lastInCommit)
	{
		if(entities.Count == 0)
			return;

		var bs = new HashSet<Bucket>();
		var cs = new HashSet<Cluster>();

		foreach(var i in entities.Cast<E>())
		{
			var c = GetCluster(ClusterFromBucket(i.Key.B));
			var b = c.GetBucket(i.Key.B);

			var e = b.Entries.Find(e => e.Key == i.Key);
			
			if(e != null)
				b.Entries.Remove(e);

			if(!i.Deleted)
				b.Entries.Add(i);

			if(i.Key is EntityId x)
				if(b.NextEid < x.E + 1)
					b.NextEid = Math.Max(b.NextEid, x.E + 1);

			bs.Add(b);
			cs.Add(c);
		}

		foreach(var i in bs)
		{
			i.Cleanup(lastInCommit);
			i.Save(batch);
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
