using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using UC.Net;

namespace UC
{
	public static class Extentions
	{
		public static bool Contains(this Exception e, Func<Exception, bool> p)
		{
			if(p(e))
				return true;

			if(e.InnerException != null && e.InnerException.Contains(p))
				return true;

			if(e is AggregateException ae)
				foreach(var i in ae.InnerExceptions)
					if(i.Contains(p))
						return true;

			return false;
		}

		public static byte[] ReadHash(this BinaryReader r)
		{
			return r.ReadBytes(Cryptography.HashSize);
		}

		public static byte[] ReadSignature(this BinaryReader r)
		{
			return r.ReadBytes(Cryptography.SignatureSize);
		}

		public static Account ReadAccount(this BinaryReader r)
		{
			var a = new Account();
			a.Read(r);
			return a;
		}

		public static ChainTime ReadTime(this BinaryReader r)
		{
			return new ChainTime(r.Read7BitEncodedInt64());
		}

		public static void Write(this BinaryWriter w, Account a)
		{
			a.Write(w);
		}

		public static void Write(this BinaryWriter w, Coin a)
		{
			a.Write(w);
		}

		public static void Write(this BinaryWriter w, BigInteger a)
		{
			var n = a.GetByteCount();

			if(n > byte.MaxValue)
				throw new IntegrityException("BigInteger is longer 256 bytes");

			w.Write((byte)n);
			w.Write(a.ToByteArray());
		}

		public static void WriteUtf8(this BinaryWriter w, string s)
		{
			var a = Encoding.UTF8.GetBytes(s);
			w.Write7BitEncodedInt(a.Length);
			w.Write(a);
		}

		public static void Write(this BinaryWriter w, ChainTime t)
		{
			w.Write7BitEncodedInt64(t.Ticks);
		}

		public static void Write(this BinaryWriter w, ProductAddress pa)
		{
			w.WriteUtf8(pa.Author);
			w.WriteUtf8(pa.Product);
		}

		public static ProductAddress ReadProductAddress(this BinaryReader r)
		{
			return new ProductAddress(r.ReadUtf8(), r.ReadUtf8());
		}

		public static void Write(this BinaryWriter w, ReleaseAddress a)
		{
			w.WriteUtf8(a.Author);
			w.WriteUtf8(a.Product);
			w.WriteUtf8(a.Platform);
			w.Write(a.Version);
		}

		public static ReleaseAddress ReadReleaseAddress(this BinaryReader r)
		{
			return new ReleaseAddress(r.ReadUtf8(), r.ReadUtf8(), r.ReadUtf8(), r.ReadVersion());
		}

		public static string ReadUtf8(this BinaryReader r)
		{
			return Encoding.UTF8.GetString(r.ReadBytes(r.Read7BitEncodedInt()));
		}

		public static Coin ReadCoin(this BinaryReader r)
		{
			return new Coin(r);
		}

		public static UC.Version ReadVersion(this BinaryReader r)
		{
			return Version.Read(r);
		}

		public static BigInteger ReadBigInteger(this BinaryReader r)
		{
			return new BigInteger(r.ReadBytes(r.ReadByte()));
		}

		public static void Write(this BinaryWriter w, UC.Version v)
		{
			w.Write(v.Era);
			w.Write(v.Generation);
			w.Write(v.Release);
			w.Write(v.Build);
		}

		public static void Union<T>(this List<T> items, IEnumerable<T> news, Func<T, T, bool> equal)
		{
			items.AddRange(news.Where(i => items.All(j => !equal(i, j))));
		}

		public static void Write<T>(this BinaryWriter w, IEnumerable<T> items, Action<T> a)
		{
			if(items != null)
			{
				w.Write7BitEncodedInt(items.Count());
			
				foreach(var i in items)
					a(i);
			}
			else
				w.Write7BitEncodedInt(0);
		}

		public static void Write<T>(this BinaryWriter w, IEnumerable<T> items) where T : IBinarySerializable
		{
			if(items != null)
			{
				w.Write7BitEncodedInt(items.Count());
			
				foreach(var i in items)
					i.Write(w);
			}
			else
				w.Write7BitEncodedInt(0);
		}

		public static void Write(this BinaryWriter w, IEnumerable<int> items)
		{
			if(items != null)
			{
				w.Write7BitEncodedInt(items.Count());
			
				foreach(var i in items)
					w.Write7BitEncodedInt(i);
			}
			else
				w.Write7BitEncodedInt(0);
		}

		public static List<T> ReadList<T>(this BinaryReader r, Func<T> a)
		{
			var o = new List<T>();

			var n = r.Read7BitEncodedInt();
			
			for(int i = 0; i < n; i++)
			{
				o.Add(a());
			}

			return o;
		}

		public static HashSet<T> ReadHashSet<T>(this BinaryReader r, Func<T> a)
		{
			var o = new HashSet<T>();

			var n = r.Read7BitEncodedInt();
			
			for(int i = 0; i < n; i++)
			{
				o.Add(a());
			}

			return o;
		}

		public static T[] ReadArray<T>(this BinaryReader r, Func<T> a)
		{
			var n = r.Read7BitEncodedInt();
			
			var o = new T[n];

			for(int i = 0; i < n; i++)
			{
				o[i] = a();
			}

			return o;
		}

		public static Dictionary<K, V> ReadDictionary<K, V>(this BinaryReader r, Func<KeyValuePair<K, V>> a)
		{
			var n = r.Read7BitEncodedInt();
			
			var o = new Dictionary<K, V>();

			for(int i = 0; i < n; i++)
			{
				var kv = a();
				o.Add(kv.Key, kv.Value);
			}

			return o;
		}

		public static void Read(this BinaryReader r, Action a)
		{
			var n = r.Read7BitEncodedInt();
			
			for(int i = 0; i < n; i++)
			{
				a();
			}
		}

		public static T[] ReadArray<T>(this BinaryReader r) where T : IBinarySerializable, new()
		{
			var n = r.Read7BitEncodedInt();
			
			var o = new T[n];

			for(int i = 0; i < n; i++)
			{
				var t = new T();
				t.Read(r);
				o[i] = t;
			}

			return o;
		}

		public static List<T> ReadList<T>(this BinaryReader r) where T : IBinarySerializable, new()
		{
			var n = r.Read7BitEncodedInt();
			
			var o = new List<T>();

			for(int i = 0; i < n; i++)
			{
				var t = new T();
				t.Read(r);
				o.Add(t);
			}

			return o;
		}


		public static void Write(this BinaryWriter w, IEnumerable<Account> items, Roundchain chain)
		{
			if(items != null)
			{
				w.Write7BitEncodedInt(items.Count());
			
				foreach(var i in items)
					w.Write(i, chain);
			}
			else
				w.Write7BitEncodedInt(0);
		}

		public static List<Account> ReadAccounts(this BinaryReader r, Roundchain chain)
		{
			var n = r.Read7BitEncodedInt();
			
			var o = new List<Account>();

			for(int i = 0; i < n; i++)
			{
				o.Add(r.ReadAccount(chain));
			}

			return o;
		}

		public static void Write(this BinaryWriter w, Account account, Roundchain chain)
		{
			///var r = chain.FindFirstTransactionRound(account);
			///
			///if(r == null || !r.Confirmed)
			///{
			///	w.Write((byte)0);
				w.Write(account);
			///} 
			///else
			///{
			///	w.Write((byte)1);
			///	w.Write7BitEncodedInt(r.Id);
			///	w.Write7BitEncodedInt(r.GetIndex(account));
			///}
		}

		public static Account ReadAccount(this BinaryReader r, Roundchain chain)
		{
			///var p = r.ReadByte();
			///
			///if(p == 0)
			///{
					return r.ReadAccount();
			///} 
			///else if(p == 1)
			///{
			///	var rid = r.Read7BitEncodedInt();
			///	var i = r.Read7BitEncodedInt();
			///
			///	return chain.FindRound(rid)?.FindAccount(i);
			///}
			///else
			///	throw new IntegrityException("Wrong Acccount packing prefix");
		}

	}
}
