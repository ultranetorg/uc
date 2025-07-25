﻿using System.Diagnostics;
using System.Net;
using System.Numerics;
using System.Text;

namespace Uccs;

public static class Extentions
{
	public static bool IsSet(this long x, long i)
	{
		return (x & i) != 0;
	}

	public static string ToHex(this byte[] e)
	{
		return Convert.ToHexString(e);
	}

	public static string ToHex(this byte[] e, int max)
	{
		return e.Length <= max ? Convert.ToHexString(e) : (Convert.ToHexString(e, 0, max/2 - 1 + max%2) + "...." + Convert.ToHexString(e, e.Length - max/2 + 1, max/2 - 1));
	}

	public static byte[] FromHex(this string e)
	{
		return Convert.FromHexString(e);
	}

	public static string ToHexPrefix(this byte[] e)
	{
		return Convert.ToHexString(e, 0, 4);
	}

	public static T Random<T>(this IEnumerable<T> e)
	{
		return e.OrderByRandom().First();
	}

	public static T RandomOrDefault<T>(this IEnumerable<T> e)
	{
		return e.Any() ? e.OrderByRandom().First() : default;
	}

	public static IEnumerable<T> OrderByRandom<T>(this IEnumerable<T> e)
	{
		return e.OrderBy(i => Guid.NewGuid());
	}

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

	public static void WriteBytes(this BinaryWriter w, byte [] data)
	{
		if(data != null && data.Length > 0)
		{
			w.Write7BitEncodedInt(data.Length);
			w.Write(data);
		}
		else
			w.Write7BitEncodedInt(0);
	}

	public static byte[] ReadBytes(this BinaryReader r)
	{
		var n = r.Read7BitEncodedInt();
		
		if(n > 0)
			return r.ReadBytes(n);
		else
			return null;
	}

	public static void Write(this BinaryWriter w, BigInteger a)
	{
		var n = a.GetByteCount();

		if(n > byte.MaxValue)
			throw new ArgumentException("BigInteger is longer 256 bytes");

		w.Write((byte)n);
		w.Write(a.ToByteArray());
	}

	public static void Write<E>(this BinaryWriter writer, E a) where E : unmanaged, System.Enum
	{
		var t = Enum.GetUnderlyingType(typeof(E));

		if(t == typeof(byte))	writer.Write((byte)(object)a); else
		if(t == typeof(ushort))	writer.Write((ushort)(object)a); else
		if(t == typeof(uint))	writer.Write((uint)(object)a); else
		
		writer.Write((ulong)(object)a);
	}

	public static E Read<E>(this BinaryReader reader, E _ignore = default) where E : unmanaged, System.Enum
	{
		var t = Enum.GetUnderlyingType(typeof(E));

		if(t == typeof(byte))	return (E)Enum.ToObject(typeof(E), reader.ReadByte());
		if(t == typeof(ushort))	return (E)Enum.ToObject(typeof(E), reader.ReadUInt16());
		if(t == typeof(uint))	return (E)Enum.ToObject(typeof(E), reader.ReadUInt32());
		
		return (E)Enum.ToObject(typeof(E), reader.ReadUInt64());
	}

// 		public static void Write(this BinaryWriter w, ProductAddress pa)
// 		{
// 			w.WriteUtf8(pa.Domain);
// 			w.WriteUtf8(pa.Product);
// 		}
// 
// 		public static ProductAddress ReadProductAddress(this BinaryReader r)
// 		{
// 			return new ProductAddress(r.ReadUtf8(), r.ReadUtf8());
// 		}

	public static string ReadUtf8(this BinaryReader r)
	{
		return Encoding.UTF8.GetString(r.ReadBytes(r.Read7BitEncodedInt()));
	}

	public static void WriteUtf8(this BinaryWriter w, string s)
	{
		var a = Encoding.UTF8.GetBytes(s);
		w.Write7BitEncodedInt(a.Length);
		w.Write(a);
	}

	public static string ReadUtf8Nullable(this BinaryReader r)
	{
		var n = r.Read7BitEncodedInt();

		if(n == 0)
			return null;
		else
			return Encoding.UTF8.GetString(r.ReadBytes(n));
	}

	public static void WriteUtf8Nullable(this BinaryWriter w, string s)
	{
		if(s == null)
		{
			w.Write7BitEncodedInt(0);
		} 
		else
		{
			var a = Encoding.UTF8.GetBytes(s);
			w.Write7BitEncodedInt(a.Length);
			w.Write(a);
		}
	}

	public static string ReadASCII(this BinaryReader r)
	{
		return Encoding.ASCII.GetString(r.ReadBytes(r.Read7BitEncodedInt()));
	}

	public static void WriteASCII(this BinaryWriter w, string s)
	{
		var a = Encoding.ASCII.GetBytes(s);
		w.Write7BitEncodedInt(a.Length);
		w.Write(a);
	}

	public static BigInteger ReadBigInteger(this BinaryReader r)
	{
		return new BigInteger(r.ReadBytes(r.ReadByte()));
	}

	public static void Write(this BinaryWriter w, Guid v)
	{
		w.Write(v.ToByteArray());
	}

	public static Guid ReadGuid(this BinaryReader r)
	{
		return new Guid(r.ReadBytes(16));
	}

	public static void Write(this BinaryWriter w, IPAddress v)
	{
		w.Write(v.MapToIPv4().GetAddressBytes());
	}

	public static IPAddress ReadIPAddress(this BinaryReader r)
	{
		return new IPAddress(r.ReadBytes(4));
	}

	public static T Read<T>(this BinaryReader r) where T : IBinarySerializable, new()
	{
		var o = new T();
		o.Read(r);
		return o;
	}

	public static T ReadNullable<T>(this BinaryReader r) where T : IBinarySerializable, new()
	{
		if(r.ReadBoolean())
		{
			var o = new T();
			o.Read(r);
			return o;
		}
		else
			return default;
	}

	//public static T ReadVirtual<T>(this BinaryReader r) where T : IBinarySerializable, ITypeCode
	//{
	//	var o = (T)ITypeCode.Contructors[typeof(T)][r.ReadByte()].Invoke(null);
	//	o.Read(r);
	//	return o;
	//}

	public static T Read<T>(this BinaryReader r, Func<byte, T> construct) where T : class, IBinarySerializable
	{
		var o = construct(r.ReadByte()) as T;
		o.Read(r);
		return o;
	}

	public static void Write(this BinaryWriter w, IBinarySerializable o)
	{
		if(o is ITypeCode c)
			Debugger.Break();

		o.Write(w);
	}

	public static void WriteNullable(this BinaryWriter w, IBinarySerializable o)
	{
		w.Write(o != null);

		if(o != null)
		{
			if(o is ITypeCode c)
				//w.Write(ITypeCode.Codes[o.GetType()]);
				Debugger.Break();
					
			o.Write(w);
		}
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

	public static void Write<K, V>(this BinaryWriter w, IDictionary<K, V> items, Action<K> writek, Action<V> writev)
	{
		if(items != null)
		{
			w.Write7BitEncodedInt(items.Count());
		
			foreach(var i in items)
			{	
				writek(i.Key);
				writev(i.Value);
			}
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

	public static int[] ReadIntArray(this BinaryReader r)
	{
		var n = r.Read7BitEncodedInt();

		var o = new int[n];

		for(int i = 0; i < n; i++)
		{
			o[i] = r.Read7BitEncodedInt();
		}

		return o;
	}

	public static IEnumerable<T> Read<T>(this BinaryReader r, Func<T> read)
	{
		var n = r.Read7BitEncodedInt();
		
		for(int i = 0; i < n; i++)
		{
			yield return read();;
		}
	}

	public static IEnumerable<T> Read<T>(this BinaryReader r, Action<T> read)  where T : new()
	{
		var n = r.Read7BitEncodedInt();
		
		for(int i = 0; i < n; i++)
		{
			var e = new T();
			read(e);
			yield return e;
		}
	}

	public static IEnumerable<T> Read<T>(this BinaryReader r, Func<T> create, Action<T> read)
	{
		var n = r.Read7BitEncodedInt();
		
		for(int i = 0; i < n; i++)
		{
			var e = create();
			read(e);
			yield return e;
		}
	}

	public static List<T> ReadList<T>(this BinaryReader r, Func<T> read)
	{
		var n = r.Read7BitEncodedInt();
	
		var o = new List<T>(n);
				
		for(int i = 0; i < n; i++)
		{
			o.Add(read());
		}
	
		return o;
	}

	public static HashSet<T> ReadHashSet<T>(this BinaryReader r, Func<T> a)
	{
		var n = r.Read7BitEncodedInt();
		
		var o = new HashSet<T>(n);
					
		for(int i = 0; i < n; i++)
		{
			o.Add(a());
		}

		return o;
	}

	public static SortedSet<T> ReadSortedSet<T>(this BinaryReader r, Func<T> a)
	{
		var n = r.Read7BitEncodedInt();
		
		var o = new SortedSet<T>();
					
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

	public static Dictionary<K, V> ReadDictionary<K, V>(this BinaryReader r, Func<K> k, Func<V> v)
	{
		var n = r.Read7BitEncodedInt();
		
		var o = new Dictionary<K, V>(n);

		for(int i = 0; i < n; i++)
		{
			o.Add(k(), v());
		}

		return o;
	}

	public static SortedDictionary<K, V> ReadSortedDictionary<K, V>(this BinaryReader r, Func<K> getk, Func<V> getv)
	{
		var n = r.Read7BitEncodedInt();
		
		var o = new SortedDictionary<K, V>();

		for(int i = 0; i < n; i++)
		{
			o.Add(getk(), getv());
		}

		return o;
	}

	public static OrderedDictionary<K, V> ReadOrderedDictionary<K, V>(this BinaryReader r, Func<K> getk, Func<V> getv)
	{
		var n = r.Read7BitEncodedInt();
		
		var o = new OrderedDictionary<K, V>();

		for(int i = 0; i < n; i++)
		{
			o.Add(getk(), getv());
		}

		return o;
	}


	//public static void Read(this BinaryReader r, Action a)
	//{
	//	var n = r.Read7BitEncodedInt();
	//	
	//	for(int i = 0; i < n; i++)
	//	{
	//		a();
	//	}
	//}

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
		
		var o = new List<T>(n);

		for(int i = 0; i < n; i++)
		{
			var t = new T();
			t.Read(r);
			o.Add(t);
		}

		return o;
	}
	
	public static void Write(this BinaryWriter w, IEnumerable<string> items)
	{
		if(items != null)
		{
			w.Write7BitEncodedInt(items.Count());
		
			foreach(var i in items)
				w.WriteUtf8(i);
		}
		else
			w.Write7BitEncodedInt(0);
	}

	public static string[] ReadStrings(this BinaryReader r)
	{
		var n = r.Read7BitEncodedInt();
		
		var o = new string[n];

		for(int i = 0; i < n; i++)
		{
			o[i] = r.ReadUtf8();
		}

		return o;
	}
}
