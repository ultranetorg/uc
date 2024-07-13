using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using RocksDbSharp;

namespace Uccs.Net
{
	public abstract class TableBase
	{
		public const int							SuperClustersCountMax = byte.MaxValue + 1;
		const int									ClustersCacheLimit = 1000;

		public Dictionary<byte, byte[]>				SuperClusters = new();
		public abstract IEnumerable<ClusterBase>	Clusters { get; }
		protected ColumnFamilyHandle				MetaColumn;
		protected ColumnFamilyHandle				MainColumn;
		protected ColumnFamilyHandle				MoreColumn;
		protected RocksDb							Engine;
		protected Mcv								Mcv;
		public abstract int							Size { get; }
		public int									Id => Array.IndexOf(Mcv.Tables, this);

		public abstract class ClusterBase
		{
			public const int		IdLength = 2;

			public byte[]			Id;
			public byte				SuperId => Id[1];
			public int				MainLength;
 			public int				NextEntityId;				
			public byte[]			Hash { get; protected set; }
			public abstract byte[]	Main { get; }

			public abstract			IEnumerable<ITableEntryBase> BaseEntries { get; }

			public abstract void	Read(BinaryReader reader);
			public abstract void	Save(WriteBatch batch);
		}

		public abstract ClusterBase		AddCluster(byte[] id);
		public abstract long			MeasureChanges(IEnumerable<Round> tail);
		public abstract void			Save(WriteBatch batch, IEnumerable<object> entities);

		public void CalculateSuperClusters()
		{
			if(!Clusters.Any())
				return;

			var ocs = Clusters.OrderBy(i => i.Id, Bytes.Comparer);

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
					h = Cryptography.Hash(Bytes.Xor(h, i.Hash));
			}

			SuperClusters[b] = h;
		}
	}

	public abstract class Table<E, K> : TableBase, IEnumerable<E> where E : class, ITableEntry<K>
	{
		public class Cluster : ClusterBase
		{
			List<E>			_Entries;
			Table<E, K>		Table;
			byte[]			_Main;

			public override IEnumerable<ITableEntryBase> BaseEntries => Entries;

			public List<E> Entries
			{
				get
				{
					if(_Entries == null)
					{
						if(Main != null)
						{
							var s = new MemoryStream(Main);
							var r = new BinaryReader(s);
	
							var a = r.ReadArray(() =>{ 
														var e = Table.Create();
														e.Id = new EntityId(Id, r.Read7BitEncodedInt());
														e.ReadMain(r);
														return e;
													 });
					
							s = new MemoryStream(Table.Engine.Get(Id, Table.MoreColumn));
							r = new BinaryReader(s);
	
							for(int i = 0; i < a.Length; i++)
							{
								a[i].ReadMore(r);
							}
	
							_Entries = a.ToList();
						}
						else
							_Entries = new ();
					}

					return _Entries;
				}
			}

			public override byte[] Main
			{
				get
				{
					if(_Main == null)
					{
						_Main = Table.Engine.Get(Id, Table.MainColumn);
					}

					return _Main;
				}
			}

			public Cluster(Table<E, K> table, byte[] id)
			{
				Table = table;
				Id = id;

				var m = Table.Engine.Get(Id, Table.MetaColumn);

				if(m != null)
				{
					var r = new BinaryReader(new MemoryStream(m));
	
					Hash = r.ReadHash();
					MainLength = r.Read7BitEncodedInt();
					NextEntityId = r.Read7BitEncodedInt();
				}
			}

			public override void Save(WriteBatch batch)
			{
				var s = new MemoryStream();
				var w = new BinaryWriter(s);

				var entities = Entries;///.OrderBy(i => i.Id.Ei);

				w.Write(entities, i =>	{
											w.Write7BitEncodedInt(i.Id.Ei);
											i.WriteMain(w);
										});

				_Main = s.ToArray();
				batch.Put(Id, _Main, Table.MainColumn);

				Hash = Cryptography.Hash(_Main);
				MainLength = _Main.Length;
				
				s.SetLength(0);
				w.Write(Hash);
				w.Write7BitEncodedInt(_Main.Length);
				w.Write7BitEncodedInt(NextEntityId);

				batch.Put(Id, s.ToArray(), Table.MetaColumn);

				s.SetLength(0);

				foreach(var i in entities)
					i.WriteMore(w);

				batch.Put(Id, s.ToArray(), Table.MoreColumn);
			}

			public void Write(BinaryWriter writer)
			{
				writer.Write(Main);
			}

			public override void Read(BinaryReader reader)
			{
				_Entries = reader.ReadList<E>(() => { 
														var e = Table.Create();
														e.Id = new EntityId(Id, reader.Read7BitEncodedInt());
														e.ReadMain(reader);
														return e;
													});;
			}

			public override string ToString()
			{
				return $"{Id.ToHex()}, Entries={{{(_Entries != null ? _Entries.Count.ToString() : "")}}}, Hash={Hash?.ToHex()}";
			}
		}

		public override IEnumerable<ClusterBase>	Clusters => _Clusters;
		public List<Cluster>						_Clusters = new();
		public static string						MetaColumnName => typeof(E).Name.Substring(0, typeof(E).Name.IndexOf("Entry")) + nameof(MetaColumn);
		public static string						MainColumnName => typeof(E).Name.Substring(0, typeof(E).Name.IndexOf("Entry")) + nameof(MainColumn);
		public static string						MoreColumnName => typeof(E).Name.Substring(0, typeof(E).Name.IndexOf("Entry")) + nameof(MoreColumn);

		protected abstract E						Create();
		public abstract Span<byte>					KeyToCluster(K k);
		public abstract bool						Equal(K a, K b);

		public override int							Size => Clusters.Sum(i => i.MainLength);

		public Table(Mcv chain)
		{
			Mcv = chain;
			Engine = Mcv.Database;
			MetaColumn = Engine.GetColumnFamily(MetaColumnName);
			MainColumn = Engine.GetColumnFamily(MainColumnName);
			MoreColumn = Engine.GetColumnFamily(MoreColumnName);

			using(var i = Engine.NewIterator(MetaColumn))
			{
				for(i.SeekToFirst(); i.Valid(); i.Next())
				{
	 				var c = new Cluster(this, i.Key());
					//c.Hash = i.Value();
	 				_Clusters.Add(c);
				}
			}

			CalculateSuperClusters();
		}

		public override ClusterBase AddCluster(byte[] id)
		{
			if(!Monitor.IsEntered(Mcv.Lock))
				Debugger.Break();

			var c = new Cluster(this, id);
			_Clusters.Add(c);
			
			return c;
		}

		public void Clear()
		{
			if(Mcv.Node != null)
				if(!Monitor.IsEntered(Mcv.Lock))
					Debugger.Break();

			_Clusters.Clear();
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
			Table<E, K>				Table;

			public Enumerator(Table<E, K> table)
			{
				Table = table;

				Cluster = Table._Clusters.GetEnumerator();
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
				Cluster = Table._Clusters.GetEnumerator();
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

		Cluster GetCluster(byte[] id)
		{
			if(Mcv.Node != null)
				if(!Monitor.IsEntered(Mcv.Lock))
					Debugger.Break();

			var c = _Clusters.Find(i => i.Id.SequenceEqual(id));

			if(c != null)
				return c;

			c = new Cluster(this, id);
			_Clusters.Add(c);

			//Recycle();

			return c;
		}

		public E FindEntry(K key)
		{
			var cid = KeyToCluster(key).ToArray();
			//var cid = Cluster.ToId(bcid);

			var c = _Clusters.Find(i => i.Id.SequenceEqual(cid));

			if(c == null)
				return default(E);

			var e = c.Entries.Find(i => Equal(i.Key, key));

// 			if(e != null)
// 			{
// 				e.LastAccessed = DateTime.UtcNow;
// 			}

			return e;
		}

		public E FindEntry(EntityId id)
		{
			var c = _Clusters.Find(i => i.Id.SequenceEqual(id.Ci));

			if(c == null)
				return default(E);

			var e = c.Entries.Find(i => i.Id.Ei == id.Ei);

			return e;
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

		public override void Save(WriteBatch batch, IEnumerable<object> entities)
		{
			if(!entities.Any())
				return;

			var cs = new HashSet<Cluster>();

			foreach(var i in entities.Cast<E>())
			{
				var c = GetCluster(i.Id.Ci);

				var e = c.Entries.Find(e => Equal(e.Key, i.Key));
				
				if(e != null)
					c.Entries.Remove(e);

				c.Entries.Add(i);

				cs.Add(c);
			}

			foreach(var i in cs)
			{
				i.Save(batch);
			}

			CalculateSuperClusters();
		}

		public override long MeasureChanges(IEnumerable<Round> tail)
		{
			if(!Monitor.IsEntered(Mcv.Lock))
				Debugger.Break();

			var affected = new Dictionary<K, E>();

			foreach(var r in tail)
			{
				foreach(var i in r.AffectedByTable(this).Cast<E>())
					if(!affected.ContainsKey(i.Key))
						affected.Add(i.Key, i);
			}


			var se = new FakeStream();
			var we = new BinaryWriter(se);
			we.Write7BitEncodedInt(_Clusters.Count);

			var si = new FakeStream();
			var wi = new BinaryWriter(si);

			int n = 0;

			foreach(var i in affected.Values)
			{
				var c = _Clusters.Find(j => j.Id.SequenceEqual(i.Id.Ci));

				var e = c?.Entries.Find(e => Equal(e.Key, i.Key));
				
				if(e != null)
				{
					we.Write7BitEncodedInt(e.Id.Ei);
					e.WriteMain(we);
				}
				else
					n++;

				wi.Write7BitEncodedInt(i.Id.Ei);
				i.WriteMain(wi);
			}

			wi.Write7BitEncodedInt(_Clusters.Count + n);

			return si.Length - se.Length ;
		}
	}
}
