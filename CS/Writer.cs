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
	
	public void WriteVirtual<T>(T item) where T : IBinarySerializable, ITypeCode
	{
		Write(Constructor.TypeToCode(item.GetType())); 
		item.Write(this);
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

	public void WriteVirtual<T>(IEnumerable<T> items) where T : IBinarySerializable, ITypeCode
	{
		if(items != null)
		{
			Write7BitEncodedInt(items.Count());
		
			foreach(var i in items)
			{	
				Write(Constructor.TypeToCode(i.GetType())); 
				i.Write(this);
			}
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
