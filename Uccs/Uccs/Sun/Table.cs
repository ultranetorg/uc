using System.Linq;
using System.Collections.Generic;
using System.IO;
using RocksDbSharp;
using System;
using System.Reflection;
using Org.BouncyCastle.Utilities.Encoders;
using Org.BouncyCastle.Utilities.Collections;
using System.Collections;

namespace Uccs.Net
{
	public enum Tables
	{
		None,
		Accounts,
		Authors,
		Analyses
	}

	public abstract class Table<E, K> : IEnumerable<E> where E : ITableEntry<K>
	{
		public class Cluster
		{
			public int				Round;
			public ushort			Id;
			public int				MainLength;
			public static byte[]	ToBytes(ushort k) => new byte[]{(byte)(k>>8), (byte)k};
			public static ushort	ToId(Span<byte> k) => (ushort)(((ushort)k[0])<<8 | k[1]);
			List<E>					_Entries;
			Table<E, K>				Table;
			byte[]					_Main;
			public byte[]			Hash { get; protected set; }

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
	
							var a = r.ReadArray<E>(() => { 
															var e = Table.Create();
															e.ReadMain(r);
															return e;
														 });
					
							s = new MemoryStream(Table.Engine.Get(ToBytes(Id), Table.MoreColumn));
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

			public byte[] Main
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

			public Cluster(Table<E, K> table, ushort id)
			{
				Table = table;
				Id = id;

				var m = Table.Engine.Get(ToBytes(Id), Table.MetaColumn);

				if(m != null)
				{
					var r = new BinaryReader(new MemoryStream(m));
	
					Hash = r.ReadHash();
					MainLength = r.Read7BitEncodedInt();
				}
			}

			public void Save(WriteBatch batch)
			{
				var s = new MemoryStream();
				var w = new BinaryWriter(s);

				var entities = Entries/*.OrderBy(i => Table.KeyToBytes(i.Key), new BytesComparer())*/;

				w.Write(entities, i => i.WriteMain(w));

				_Main = s.ToArray();
				batch.Put(ToBytes(Id), _Main, Table.MainColumn);

				Hash = Table.Database.Zone.Cryptography.Hash(_Main);
				MainLength = _Main.Length;
				
				s.SetLength(0);
				w.Write(Hash);
				w.Write7BitEncodedInt(_Main.Length);

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

			public void Read(BinaryReader reader)
			{
				_Entries = reader.ReadList<E>(() => { 
														var e = Table.Create();
														e.ReadMain(reader);
														return e;
													});;
			}

			public override string ToString()
			{
				return $"{Id:X2}, Entries={{{(_Entries != null ? _Entries.Count.ToString() : "")}}}, Hash={(Hash != null ? Hex.ToHexString(Hash) : "")}";
			}
		}

		public const int				ClustersKeyLength = 2;
		const int						ClustersCacheLimit = 1000;
		public Tables					Type
										{
											get
											{
												if(GetType() == typeof(AccountTable)) return Tables.Accounts;
												if(GetType() == typeof(AuthorTable)) return Tables.Authors;
												if(GetType() == typeof(AnalysisTable)) return Tables.Analyses;

												throw new IntegrityException();
											}
										}

		public Dictionary<byte, byte[]> SuperClusters = new();
		public List<Cluster>			Clusters = new();
		ColumnFamilyHandle				MetaColumn;
		ColumnFamilyHandle				MainColumn;
		ColumnFamilyHandle				MoreColumn;
		public static string			MetaColumnName => typeof(E).Name.Substring(0, typeof(E).Name.IndexOf("Entry")) + nameof(MetaColumn);
		public static string			MainColumnName => typeof(E).Name.Substring(0, typeof(E).Name.IndexOf("Entry")) + nameof(MainColumn);
		public static string			MoreColumnName => typeof(E).Name.Substring(0, typeof(E).Name.IndexOf("Entry")) + nameof(MoreColumn);
		RocksDb							Engine;
		protected Mcv					Database;

		protected abstract E			Create();
		protected abstract byte[]		KeyToBytes(K k);
		protected abstract bool			Equal(K a, K b);

		public Table(Mcv chain)
		{
			Database = chain;
			Engine = Database.Engine;
			MetaColumn = Engine.GetColumnFamily(MetaColumnName);
			MainColumn = Engine.GetColumnFamily(MainColumnName);
			MoreColumn = Engine.GetColumnFamily(MoreColumnName);

			using(var i = Engine.NewIterator(MetaColumn))
			{
				for(i.SeekToFirst(); i.Valid(); i.Next())
				{
	 				var c = new Cluster(this, Cluster.ToId(i.Key()));
					//c.Hash = i.Value();
	 				Clusters.Add(c);
				}
			}

			CalculateSuperClusters();
		}

		public void Clear()
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

		public void CalculateSuperClusters()
		{
			if(!Clusters.Any())
				return;

			var ocs = Clusters.OrderBy(i => i.Id);

			byte b = (byte)(ocs.First().Id >> 8);
			byte[] h = ocs.First().Hash;

			foreach(var i in ocs.Skip(1))
			{
				if(b != i.Id >> 8)
				{
					SuperClusters[b] = h;
					b = (byte)(i.Id >> 8);
					h = i.Hash;
				}
				else
					h = Database.Zone.Cryptography.Hash(Bytes.Xor(h, i.Hash));
			}

			SuperClusters[b] = h;
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

		Cluster GetCluster(ushort id)
		{
			var c = Clusters.Find(i => i.Id == id);

			if(c != null)
				return c;

			c = new Cluster(this, id);
			Clusters.Add(c);

			//Recycle();

			return c;
		}

		public E FindEntry(K key)
		{
			var bcid = KeyToBytes(key).Take(ClustersKeyLength).ToArray();
			var cid = Cluster.ToId(bcid);

			var c = Clusters.Find(i => i.Id == cid);

			if(c == null)
				return default(E);

			var e = c.Entries.Find(i => Equal(i.Key, key));

// 			if(e != null)
// 			{
// 				e.LastAccessed = DateTime.UtcNow;
// 			}

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

		public void Save(WriteBatch batch, IEnumerable<E> entities)
		{
			if(!entities.Any())
				return;

			var cs = new HashSet<Cluster>();

			foreach(var i in entities)
			{
				var c = GetCluster(Cluster.ToId(i.GetClusterKey(ClustersKeyLength)));

				c.Entries.Remove(c.Entries.Find(e => Equal(e.Key, i.Key)));
				c.Entries.Add(i);

				cs.Add(c);
			}

			foreach(var i in cs)
			{
				i.Save(batch);
			}

			CalculateSuperClusters();
		}
	}
}
