using System.Linq;
using System.Collections.Generic;
using System.IO;
using RocksDbSharp;
using System;
using System.Reflection;
using Org.BouncyCastle.Utilities.Encoders;
using Org.BouncyCastle.Utilities.Collections;
using System.Collections;

namespace UC.Net
{
	public enum Tables
	{
		Null,
		State,
		Accounts,
		Authors,
		Products,
		Realizations,
		Releases
	}

	public abstract class Table<E, K> : IEnumerable<E> where E : TableEntry<K>
	{
		public class Cluster
		{
			public int				Round;
			public ushort			Id;
			public int				MainLength;
			public static byte[]	ToBytes(ushort k) => new byte[]{(byte)(k>>8), (byte)k};
			public static ushort	ToId(byte[] k) => (ushort)(((ushort)k[0])<<8 | k[1]);
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
															e.Read(r);
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
	
					Hash = r.ReadSha3();
					MainLength = r.Read7BitEncodedInt();
				}
			}

			public void Save(WriteBatch batch)
			{
				var s = new MemoryStream();
				var w = new BinaryWriter(s);

				w.Write(Entries.OrderBy(i => Table.KeyToBytes(i.Key), new BytesComparer()));

				_Main = s.ToArray();

				Hash = Cryptography.Current.Hash(_Main);
				MainLength = _Main.Length;
				
				s.Position = 0;
				w.Write(Hash);
				w.Write7BitEncodedInt(_Main.Length);

				batch.Put(ToBytes(Id), s.ToArray(), Table.MetaColumn);
				batch.Put(ToBytes(Id), _Main,		Table.MainColumn);

				s.Position = 0;

				foreach(var i in Entries)
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
														e.Read(reader);
														return e;
													});;
			}

			public override string ToString()
			{
				return $"{Id:X2}, Entries={{{(_Entries != null ? _Entries.Count.ToString() : "")}}}, Hash={(Hash != null ? Hex.ToHexString(Hash) : "")}";
			}
		}

		const int						ClustersCacheLimit = 1000;

		//public byte[]					Hash { get; protected set; } 
		public List<Cluster>			Clusters = new();
		ColumnFamilyHandle				MetaColumn;
		ColumnFamilyHandle				MainColumn;
		ColumnFamilyHandle				MoreColumn;
		public static string			MetaColumnName => typeof(E).Name.Substring(0, typeof(E).Name.IndexOf("Entry")) + nameof(MetaColumn);
		public static string			MainColumnName => typeof(E).Name.Substring(0, typeof(E).Name.IndexOf("Entry")) + nameof(MainColumn);
		public static string			MoreColumnName => typeof(E).Name.Substring(0, typeof(E).Name.IndexOf("Entry")) + nameof(MoreColumn);
		RocksDb							Engine;
		protected Database				Database;

		protected abstract E			Create();
		protected abstract byte[]		KeyToBytes(K k);

		public Tables					Type => Enum.Parse<Tables>(GetType().Name.Replace("Table", "s"));

		public Table(Database chain)
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
			}

			public void Dispose()
			{
				Cluster.Dispose();
				Entity.Dispose();
			}

			public bool MoveNext()
			{
				var e = Entity.MoveNext();
				
				if(!e)
				{
					return Cluster.MoveNext();
				}
				
				return true;
			}

			public void Reset()
			{
				Cluster = Table.Clusters.GetEnumerator();
				Entity = Cluster.Current.Entries.GetEnumerator();
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

// 		public E GetEntry(K key)
// 		{
// 			var e = FindEntry(key);
// 		
// 			if(e == null)
// 			{
// 				e = Create(key);
// 				e.LastAccessed = DateTime.UtcNow;
// 				Entries.Add(e);
// 				Recycle();
// 			} 
// 		
// 			return e;
// 		}

		Cluster GetCluster(ushort id)
		{
			var c = Clusters.Find(i => i.Id == id);

			if(c != null)
				return c;

			c = new Cluster(this, id);
			Clusters.Add(c);

			Recycle();

			return c;
		}

		public E FindEntry(K key)
		{
			var bcid = KeyToBytes(key).Take(TableEntry<K>.ClusterKeyLength).ToArray();
			var cid = Cluster.ToId(bcid);

			var c = Clusters.Find(i => i.Id == cid);

			if(c == null)
				return null;

			var e = c.Entries.Find(i => i.Key.Equals(key));

			if(e != null)
			{
				e.LastAccessed = DateTime.UtcNow;
			}

			return e;
		}

		void Recycle()
		{
			if(Clusters.Count > ClustersCacheLimit)
			{
				foreach(var i in Clusters.OrderByDescending(i => i.Entries.Max(i => i.LastAccessed)).Skip(ClustersCacheLimit))
				{
					Clusters.Remove(i);
				}
			}
		}

		public void Save(WriteBatch batch, IEnumerable<E> items)
		{
			var cs = new HashSet<Cluster>();

			foreach(var i in items)
			{
				var c = GetCluster(Cluster.ToId(i.ClusterKey));

				c.Entries.Remove(c.Entries.Find(e => e.Key.Equals(i.Key)));
				c.Entries.Add(i);

				cs.Add(c);
			}

			foreach(var i in cs)
			{
				i.Save(batch);
			}
		}
	}
}
