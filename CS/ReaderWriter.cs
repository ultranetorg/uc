using System.Diagnostics;
using System.Net;
using System.Numerics;
using System.Text;

namespace Uccs;

public interface ITypeCode
{
}

public interface IBinarySerializable
{
	void			Write(Writer writer);
	void			Read(Reader reader);

	public byte[] ToRaw()	
	{
		var s = new MemoryStream();
		var w = new Writer(s);
								
		Write(w);
								
		return s.ToArray();
	}
}

public class Writer : BinaryWriter
{
	public Constructor Constructor;

	public Writer(Stream stream) : base(stream)
	{
	}

	public Writer(Stream stream, Constructor constructor) : base(stream)
	{
		Constructor = constructor;
	}

	public new void Write(bool value) => base.Write(value);
	public new void Write(byte[] value) => base.Write(value);
	public new void Write(byte value) => base.Write(value);
	public new void Write(sbyte value) => base.Write(value);
	public new void Write(short value) => base.Write(value);
	public new void Write(ushort value) => base.Write(value);
	public new void Write(int value) => base.Write(value);
	public new void Write(uint value) => base.Write(value);
	public new void Write(long value) => base.Write(value);
	public new void Write(ulong value) => base.Write(value);

	public void WriteBytes(byte [] data)
	{
		if(data != null && data.Length > 0)
		{
			Write7BitEncodedInt(data.Length);
			base.Write(data);
		}
		else
			Write7BitEncodedInt(0);
	}

	public void Write(BigInteger a)
	{
		var n = a.GetByteCount();
	
		if(n > byte.MaxValue)
			throw new ArgumentException("BigInteger is longer 256 bytes");
	
		base.Write((byte)n);
		base.Write(a.ToByteArray());
	}

	public void WriteUtf8(string s)
	{
		if(s == null)
		{
			Write7BitEncodedInt(0);
		} 
		else
		{
			var a = Encoding.UTF8.GetBytes(s);
			Write7BitEncodedInt(a.Length);
			base.Write(a);
		}
	}

	public void WriteASCII(string s)
	{
		if(s == null)
		{
			Write7BitEncodedInt(0);
		} 
		else
		{
			var a = Encoding.ASCII.GetBytes(s);
			Write7BitEncodedInt(a.Length);
			base.Write(a);
		}
	}

	public void Write(Guid v)
	{
		base.Write(v.ToByteArray());
	}

	public void Write(IPAddress v)
	{
		base.Write(v.MapToIPv4().GetAddressBytes());
	}

	public void WriteNullable(IBinarySerializable o)
	{
		base.Write(o != null);

		if(o != null)
		{
			if(o is ITypeCode c)
				//w.Write(ITypeCode.Codes[o.GetType()]);
				Debugger.Break();
					
			o.Write(this);
		}
	}

	public void Write(IBinarySerializable o)
	{
		if(o is ITypeCode c)
			Debugger.Break();

		o.Write(this);
	}

	public void Write<T>(IEnumerable<T> items, Action<T> a)
	{
		if(items != null)
		{
			Write7BitEncodedInt(items.Count());
		
			foreach(var i in items)
				a(i);
		}
		else
			Write7BitEncodedInt(0);
	}

	public void Write<T>(IEnumerable<T> items) where T : IBinarySerializable
	{
		if(items != null)
		{
			Write7BitEncodedInt(items.Count());
		
			foreach(var i in items)
				i.Write(this);
		}
		else
			Write7BitEncodedInt(0);
	}

	public void Write<K, V>(IDictionary<K, V> items, Action<K> writek, Action<V> writev)
	{
		if(items != null)
		{
			Write7BitEncodedInt(items.Count());
		
			foreach(var i in items)
			{	
				writek(i.Key);
				writev(i.Value);
			}
		}
		else
			Write7BitEncodedInt(0);
	}

	public void Write(IEnumerable<int> items)
	{
		if(items != null)
		{
			Write7BitEncodedInt(items.Count());
		
			foreach(var i in items)
				Write7BitEncodedInt(i);
		}
		else
			Write7BitEncodedInt(0);
	}

	public void Write<E>(E a) where E : unmanaged, System.Enum
	{
		var t = Enum.GetUnderlyingType(typeof(E));

		if(t == typeof(byte))	base.Write((byte)(object)a); else
		if(t == typeof(sbyte))	base.Write((sbyte)(object)a); else
		if(t == typeof(ushort))	base.Write((ushort)(object)a); else
		if(t == typeof(uint))	base.Write((uint)(object)a); 
		else
			base.Write((ulong)(object)a);
	}
	
	public void Write(IEnumerable<string> items)
	{
		if(items != null)
		{
			Write7BitEncodedInt(items.Count());
		
			foreach(var i in items)
				WriteUtf8(i);
		}
		else
			Write7BitEncodedInt(0);
	}
}

public class Reader : BinaryReader
{
	public Constructor Constructor;

	public Reader(Stream stream) : base(stream)
	{
	}

	public Reader(Stream stream, Constructor constructor) : base(stream)
	{
		Constructor = constructor;
	}

	public Reader(byte[] bytes) : base(new MemoryStream(bytes))
	{
	}

	public byte[] ReadBytes()
	{
		var n = Read7BitEncodedInt();

		if(n > 0)
			return base.ReadBytes(n);
		else
			return null;
	}

	public E Read<E>(E _ignore = default) where E : unmanaged, System.Enum
	{
		var t = Enum.GetUnderlyingType(typeof(E));

		if(t == typeof(byte)) return (E)Enum.ToObject(typeof(E), ReadByte());
		if(t == typeof(sbyte)) return (E)Enum.ToObject(typeof(E), ReadSByte());
		if(t == typeof(ushort)) return (E)Enum.ToObject(typeof(E), ReadUInt16());
		if(t == typeof(uint)) return (E)Enum.ToObject(typeof(E), ReadUInt32());

		return (E)Enum.ToObject(typeof(E), ReadUInt64());
	}

	public string ReadUtf8()
	{
		var n = Read7BitEncodedInt();

		if(n == 0)
			return null;
		else
			return Encoding.UTF8.GetString(ReadBytes(n));
	}

	public string ReadASCII()
	{
		var n = Read7BitEncodedInt();

		if(n == 0)
			return null;
		else
			return Encoding.ASCII.GetString(ReadBytes(n));
	}

	public BigInteger ReadBigInteger()
	{
		return new BigInteger(ReadBytes(ReadByte()));
	}

	public Guid ReadGuid()
	{
		return new Guid(ReadBytes(16));
	}

	public IPAddress ReadIPAddress()
	{
		return new IPAddress(ReadBytes(4));
	}

	public T Read<T>() where T : IBinarySerializable, new()
	{
		var o = new T();
		o.Read(this);
		return o;
	}

	public T ReadNullable<T>() where T : IBinarySerializable, new()
	{
		if(ReadBoolean())
		{
			var o = new T();
			o.Read(this);
			return o;
		}
		else
			return default;
	}

	//public T ReadVirtual<T>() where T : IBinarySerializable, ITypeCode
	//{
	//	var o = (T)ITypeCode.Contructors[typeof(T)][ReadByte()].Invoke(null);
	//	o.Read(r);
	//	return o;
	//}

	public T Read<T>(Func<byte, T> construct) where T : class, IBinarySerializable
	{
		var o = construct(ReadByte()) as T;
		o.Read(this);
		return o;
	}

	public T ReadVirtual<T>() where T : class, IBinarySerializable
	{
		var o = Constructor.Construct(typeof(T), ReadByte()) as T;
		o.Read(this);
		return o;
	}

	public int[] ReadIntArray()
	{
		var n = Read7BitEncodedInt();

		var o = new int[n];

		for(int i = 0; i < n; i++)
		{
			o[i] = Read7BitEncodedInt();
		}

		return o;
	}

	public IEnumerable<T> Read<T>(Func<T> read)
	{
		var n = Read7BitEncodedInt();

		for(int i = 0; i < n; i++)
		{
			yield return read(); ;
		}
	}

	public IEnumerable<T> Read<T>(Action<T> read) where T : new()
	{
		var n = Read7BitEncodedInt();

		for(int i = 0; i < n; i++)
		{
			var e = new T();
			read(e);
			yield return e;
		}
	}

	public IEnumerable<T> Read<T>(Func<T> create, Action<T> read)
	{
		var n = Read7BitEncodedInt();

		for(int i = 0; i < n; i++)
		{
			var e = create();
			read(e);
			yield return e;
		}
	}

	public List<T> ReadList<T>(Func<T> read)
	{
		var n = Read7BitEncodedInt();

		var o = new List<T>(n);

		for(int i = 0; i < n; i++)
		{
			o.Add(read());
		}

		return o;
	}

	public HashSet<T> ReadHashSet<T>(Func<T> a)
	{
		var n = Read7BitEncodedInt();

		var o = new HashSet<T>(n);

		for(int i = 0; i < n; i++)
		{
			o.Add(a());
		}

		return o;
	}

	public SortedSet<T> ReadSortedSet<T>(Func<T> a)
	{
		var n = Read7BitEncodedInt();

		var o = new SortedSet<T>();

		for(int i = 0; i < n; i++)
		{
			o.Add(a());
		}

		return o;
	}

	public T[] ReadArray<T>(Func<T> a)
	{
		var n = Read7BitEncodedInt();

		var o = new T[n];

		for(int i = 0; i < n; i++)
		{
			o[i] = a();
		}

		return o;
	}

	public Dictionary<K, V> ReadDictionary<K, V>(Func<K> k, Func<V> v)
	{
		var n = Read7BitEncodedInt();

		var o = new Dictionary<K, V>(n);

		for(int i = 0; i < n; i++)
		{
			o.Add(k(), v());
		}

		return o;
	}

	public SortedDictionary<K, V> ReadSortedDictionary<K, V>(Func<K> getk, Func<V> getv)
	{
		var n = Read7BitEncodedInt();

		var o = new SortedDictionary<K, V>();

		for(int i = 0; i < n; i++)
		{
			o.Add(getk(), getv());
		}

		return o;
	}

	public OrderedDictionary<K, V> ReadOrderedDictionary<K, V>(Func<K> getk, Func<V> getv)
	{
		var n = Read7BitEncodedInt();

		var o = new OrderedDictionary<K, V>();

		for(int i = 0; i < n; i++)
		{
			o.Add(getk(), getv());
		}

		return o;
	}


	//public void Read(Action a)
	//{
	//	var n = Read7BitEncodedInt();
	//	
	//	for(int i = 0; i < n; i++)
	//	{
	//		a();
	//	}
	//}

	public T[] ReadArray<T>() where T : IBinarySerializable, new()
	{
		var n = Read7BitEncodedInt();

		var o = new T[n];

		for(int i = 0; i < n; i++)
		{
			var t = new T();
			t.Read(this);
			o[i] = t;
		}

		return o;
	}

	public List<T> ReadList<T>() where T : IBinarySerializable, new()
	{
		var n = Read7BitEncodedInt();

		var o = new List<T>(n);

		for(int i = 0; i < n; i++)
		{
			var t = new T();
			t.Read(this);
			o.Add(t);
		}

		return o;
	}

	public string[] ReadStrings()
	{
		var n = Read7BitEncodedInt();

		var o = new string[n];

		for(int i = 0; i < n; i++)
		{
			o[i] = ReadUtf8();
		}

		return o;
	}
}