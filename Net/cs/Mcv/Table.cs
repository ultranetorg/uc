using System.Collections;
using RocksDbSharp;

namespace Uccs.Net
{
	public abstract class TableBase
	{
		public const int					ClustersCountMax = 256 * 256;
		public const int					SuperClustersCountMax = 256;
		const int							ClustersCacheLimit = 1000;

		public Dictionary<byte, byte[]>		SuperClusters = new();
		public IEnumerable<ClusterBase>		Clusters { get; protected set; }
		protected ColumnFamilyHandle		MetaColumn;
		protected ColumnFamilyHandle		MainColumn;
		protected ColumnFamilyHandle		MoreColumn;
		protected RocksDb					Engine;
		protected Mcv						Mcv;
		public abstract int					Size { get; }
		public int							Id => Array.IndexOf(Mcv.Tables, this);

		public abstract ClusterBase			AddCluster(ushort id);
		public abstract void				Clear();
		public abstract void				Save(WriteBatch batch, IEnumerable<object> entities, Round lastconfirmedround);

		public abstract class ClusterBase
		{
			public ushort					Id;
			public byte						SuperId => (byte)(Id >> 8);
			public int						MainLength;
 			public int						NextEntryId;				
			public byte[]					Hash { get; protected set; }
			public abstract byte[]			Main { get; }
			public IEnumerable<ITableEntry> Entries { get; protected set; }

			public static byte[]			ToBytes(ushort id) => [(byte)(id >> 8), (byte)(id & 0xff)];
			public static ushort			FromBytes(byte[] id) => (ushort)(id[0]<< 8 | id[1]);
			public abstract void			Read(BinaryReader reader);
			public abstract void			Save(WriteBatch batch);

			public void Cleanup(Round lastInCommit)
			{
				foreach(var i in Entries)
				{
					i.Cleanup(lastInCommit);
				}
			}
		}

		public void CalculateSuperClusters()
		{
			if(!Clusters.Any())
				return;

			var ocs = Clusters.OrderBy(i => i.Id);

			byte b = ocs.First().SuperId;
			byte[] h = ocs.First().Hash;

			foreach(var i in ocs.Skip(1))
			{
				if(b != i.SuperId)
				{
					SuperClusters[b] = h;
					b = i.SuperId;
					h = i.Hash;
				}
				else
					h = Cryptography.Hash(h, i.Hash);
			}

			SuperClusters[b] = h;
		}
	}

	public abstract class Table<E> : TableBase, IEnumerable<E> where E : class, ITableEntry
	{
		public class Cluster : ClusterBase
		{
			//List<E>			_Entries;
			Table<E>		Table;
			byte[]			_Main;

			//public new IEnumerable<E> Entries => base.Entries as List<E>;

			public override byte[] Main
			{
				get
				{
					if(_Main == null)
					{
						_Main = Table.Engine.Get(ToBytes(Id), Table.MainColumn);
					}

					return _Main;
				}
			}

			public new List<E> Entries
			{
				get
				{
					if(base.Entries == null)
					{
						if(Main != null)
						{
							var s = new MemoryStream(Main);
							var r = new BinaryReader(s);
	
							var a = r.ReadList(() =>{ 
														var e = Table.Create(Id);
														e.ReadMain(r);
														return e;
													 });
					
							s = new MemoryStream(Table.Engine.Get(ToBytes(Id), Table.MoreColumn));
							r = new BinaryReader(s);
	
							for(int i = 0; i < a.Count; i++)
							{
								a[i].ReadMore(r);
							}
	
							base.Entries = a;
						}
						else
							base.Entries = new List<E>();
					}

					return base.Entries as List<E>;
				}
				set
				{
					base.Entries = value;
				}
			}

			public Cluster(Table<E> table, ushort id)
			{
				Table = table;
				Id = id;

				var m = Table.Engine.Get(ToBytes(id), Table.MetaColumn);

				if(m != null)
				{
					var r = new BinaryReader(new MemoryStream(m));
	
					Hash = r.ReadHash();
					MainLength = r.Read7BitEncodedInt();
					NextEntryId = r.Read7BitEncodedInt();
				}
			}

			public override void Save(WriteBatch batch)
			{
				var s = new MemoryStream();
				var w = new BinaryWriter(s);

				var entities = Entries;///.OrderBy(i => i.Id.Ei);

				w.Write(entities, i =>	{
											i.WriteMain(w);
										});

				_Main = s.ToArray();
				batch.Put(ToBytes(Id), _Main, Table.MainColumn);

				Hash = Cryptography.Hash(_Main);
				MainLength = _Main.Length;
				
				s.SetLength(0);
				w.Write(Hash);
				w.Write7BitEncodedInt(_Main.Length);
				w.Write7BitEncodedInt(NextEntryId);

				batch.Put(ToBytes(Id), s.ToArray(), Table.MetaColumn);

				s.SetLength(0);

				foreach(var i in entities)
					i.WriteMore(w);

				batch.Put(ToBytes(Id), s.ToArray(), Table.MoreColumn);
			}

			public void Write(BinaryWriter writer)
			{
				writer.Write(Main);
			}

			public override void Read(BinaryReader reader)
			{
				Entries = reader.ReadList(() => { 
													var e = Table.Create(Id);
													e.ReadMain(reader);
													return e;
												});;
			}

			public override string ToString()
			{
				return $"{Id}, Entries={{{Entries?.Count}}}, Hash={Hash?.ToHex()}";
			}
		}

		public new List<Cluster>					Clusters => base.Clusters as List<Cluster>;
		//public List<Cluster>						_Clusters = new();
		public static string						MetaColumnName => typeof(E).Name.Substring(0, typeof(E).Name.IndexOf("Entry")) + nameof(MetaColumn);
		public static string						MainColumnName => typeof(E).Name.Substring(0, typeof(E).Name.IndexOf("Entry")) + nameof(MainColumn);
		public static string						MoreColumnName => typeof(E).Name.Substring(0, typeof(E).Name.IndexOf("Entry")) + nameof(MoreColumn);

		protected abstract E						Create(ushort cid);
		//public abstract Span<byte>					KeyToCluster(K k);
		//public abstract bool						Equal(K a, K b);

		public override int							Size => Clusters.Sum(i => i.MainLength);

		public Table(Mcv chain)
		{
			Mcv = chain;
			Engine = Mcv.Database;
			MetaColumn = Engine.GetColumnFamily(MetaColumnName);
			MainColumn = Engine.GetColumnFamily(MainColumnName);
			MoreColumn = Engine.GetColumnFamily(MoreColumnName);

			base.Clusters = new List<Cluster>();

			using(var i = Engine.NewIterator(MetaColumn))
			{
				for(i.SeekToFirst(); i.Valid(); i.Next())
				{
	 				var c = new Cluster(this, Cluster.FromBytes(i.Key()));
					//c.Hash = i.Value();
	 				Clusters.Add(c);
				}
			}

			CalculateSuperClusters();
		}

		public override ClusterBase AddCluster(ushort id)
		{
			var c = new Cluster(this, id);
			Clusters.Add(c);
			
			return c;
		}

		public override void Clear()
		{
			Clusters.Clear();
			SuperClusters.Clear();

			Engine.DropColumnFamily(MetaColumnName);
			Engine.DropColumnFamily(MainColumnName);
			Engine.DropColumnFamily(MoreColumnName);

			MetaColumn = Engine.CreateColumnFamily(new (), MetaColumnName);
			MainColumn = Engine.CreateColumnFamily(new (), MainColumnName);
			MoreColumn = Engine.CreateColumnFamily(new (), MoreColumnName);
		}

		public class Enumerator : IEnumerator<E>
		{
			public E Current => Entity.Current;
			object IEnumerator.Current => Entity.Current;

			IEnumerator<Cluster>	Cluster;
			IEnumerator<E>			Entity;
			Table<E>				Table;

			public Enumerator(Table<E> table)
			{
				Table = table;

				Cluster = Table.Clusters.GetEnumerator();
			}

			public void Dispose()
			{
				Cluster?.Dispose();
				Entity?.Dispose();
			}

			public bool MoveNext()
			{
				start: 
					if(Entity == null)
					{
						if(Cluster.MoveNext())
						{
							Entity = Cluster.Current.Entries.GetEnumerator();
						}
						else
							return false;
					}

					if(Entity.MoveNext())
					{
						return true;
					}
					else
					{
						Entity = null;
						goto start;
					}
			}

			public void Reset()
			{
				Entity = null;
				Cluster = Table.Clusters.GetEnumerator();
			}
		}

		public IEnumerator<E> GetEnumerator()
		{
			return new Enumerator(this);
		}

 		IEnumerator IEnumerable.GetEnumerator()
 		{
 			return new Enumerator(this);
 		}

		public Cluster GetCluster(ushort id)
		{
			var c = Clusters.Find(i => i.Id == id);

			if(c != null)
				return c;

			c = new Cluster(this, id);
			Clusters.Add(c);

			//Recycle();

			return c;
		}

		public Cluster FindCluster(ushort id)
		{
			return Clusters.Find(i => i.Id == id);
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

		public override void Save(WriteBatch batch, IEnumerable<object> entities, Round lastInCommit)
		{
			if(!entities.Any())
				return;

			var cs = new HashSet<Cluster>();

			foreach(var i in entities.Cast<E>())
			{
				var c = GetCluster(i.BaseId.C);

				var e = c.Entries.Find(e => e.BaseId == i.BaseId);
				
				if(e != null)
					c.Entries.Remove(e);

				if(!i.Deleted)
					c.Entries.Add(i);

				cs.Add(c);
			}

			foreach(var i in cs)
			{
				i.Cleanup(lastInCommit);
				i.Save(batch);
			}

			CalculateSuperClusters();
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
}
