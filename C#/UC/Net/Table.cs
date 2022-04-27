using System.Linq;
using System.Collections.Generic;
using System.IO;
using RocksDbSharp;
using System;

namespace UC.Net
{
	public abstract class Table<E, K> where E : Entry<K>
	{
		protected List<E>				Entries = new();
		protected Roundchain			Chain;
		protected ColumnFamilyHandle	ColumnFamily;
		protected RocksDb				Database;
		const int						CacheLimit = 1000;

		protected abstract E			Create(K key);
		protected abstract byte[]		KeyToBytes(K k);

		public Table(Roundchain chain, ColumnFamilyHandle cfh)
		{
			Chain = chain;
			Database = Chain.Database;
			ColumnFamily = cfh;
		}

		public E GetEntry(K key)
		{
			var e = FindEntry(key);
		
			if(e == null)
			{
				e = Create(key);
				e.LastAccessed = DateTime.UtcNow;
				Entries.Add(e);
				Recycle();
			} 
		
			return e;
		}

		public E FindEntry(K key)
		{
			var e = Entries.Find(i => i.Key.Equals(key));

			if(e == null)
			{
				var d = Database.Get(KeyToBytes(key), ColumnFamily);

				if(d != null)
				{
					e = Create(key);
					e.Read(new BinaryReader(new MemoryStream(d)));
					Entries.Add(e);
				} 
			}

			if(e != null)
			{
				e.LastAccessed = DateTime.UtcNow;
				Recycle();
			}
			
			return e;
		}

		void Recycle()
		{
			if(Entries.Count > CacheLimit)
			{
				foreach(var i in Entries.OrderByDescending(i => i.LastAccessed).Skip(CacheLimit))
				{
					Entries.Remove(i);
				}
			}
		}

		public IEnumerable<Round> FindRounds(IEnumerable<int> rids)
		{
			return rids.Select(i => Chain.FindRound(i));
		}

// 		public void Update(Round round, WriteBatch batch)
// 		{
// 			var affected = new HashSet<E>();
// 
// 			foreach(var p in round.ConfirmedPayloads.AsEnumerable().Reverse())
// 			{
// 				Update(round, p, affected);
// 
// 				foreach(var t in p.SuccessfulTransactions.AsEnumerable().Reverse())
// 				{
// 					Update(round, t, affected);
// 
// 					foreach(var o in t.SuccessfulOperations.AsEnumerable().Reverse())
// 						Update(round, o, affected);
// 				}
// 			}
// 
// 			Save(batch, affected);
// 		}

		public void Save(WriteBatch batch, IEnumerable<E> items)
		{
			foreach(var i in items)
			{
				Entries.Remove(Entries.Find(e => e.Key.Equals(i.Key)));
				Entries.Add(i);

				var s = new MemoryStream();
				var w = new BinaryWriter(s);

				i.Write(w);

				batch.Put(KeyToBytes(i.Key), s.ToArray(), ColumnFamily);
			}
		}
	}
}
