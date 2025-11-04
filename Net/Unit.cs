using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Uccs.Net;

public struct Unit : IComparable, IComparable<Unit>, IEquatable<Unit>, IBinarySerializable, ITextSerialisable
{
	public const byte				Precision = 18;
	public static BigInteger		One = 1_000_000_000_000_000_000;
	public readonly static Unit		Zero = new Unit();
	public readonly static Unit		Min = new Unit{Attos = 1};
	public BigInteger				Attos;

	public static implicit operator Unit(long value) => new Unit(value);
	public static implicit operator Unit(double value) => new Unit(value);
	//public static implicit operator BigInteger(Money money) => money.Attos / One;

	public static Unit operator +	(Unit v)			=> v;
	public static Unit operator -	(Unit v)			=> new Unit(-v.Attos);
	public static Unit operator +	(Unit a, Unit b)	=> new Unit(a.Attos + b.Attos);
	public static Unit operator -	(Unit a, Unit b)	=> new Unit(a.Attos - b.Attos);
	public static Unit operator ++	(Unit v)			{v.Attos += One; return v; }
	public static Unit operator --	(Unit v)			{v.Attos -= One; return v; }
	public static Unit operator *	(Unit a, Unit b)	=> new Unit(a.Attos * b.Attos / One);
	public static Unit operator /	(Unit a, Unit b)	=> new Unit(One * a.Attos / b.Attos);
	public static Unit operator %	(Unit a, Unit b)	=> new Unit(a.Attos % b.Attos);
	public static bool operator ==	(Unit a, Unit b)	=> a.Attos == b.Attos;
	public static bool operator !=	(Unit a, Unit b)	=> a.Attos != b.Attos;
	public static bool operator <	(Unit a, Unit b)	=> a.Attos < b.Attos;
	public static bool operator >	(Unit a, Unit b)	=> a.Attos > b.Attos;
	public static bool operator <=	(Unit a, Unit b)	=> a.Attos <= b.Attos;
	public static bool operator >=	(Unit a, Unit b)	=> a.Attos >= b.Attos;

	public override string ToString()
	{
		//return Nethereum.Web3.Web3.Convert.FromWeiToBigDecimal(Attos).ToString();
		//return ((double)Attos / (double)One).ToString();
		var a = Attos.ToString().TrimStart('-');

		string o;
		
		if(a.Length <= Precision)
		{
			o = Attos == 0 ? "0" : ("0." + a.ToString().PadLeft(Precision, '0').TrimEnd('0'));
		}
		else
		{
			o = a.TakeLast(Precision).All(i => i == '0') ? a.Substring(0, a.Length - Precision) : a.Insert(a.Length - Precision, ".").TrimEnd('0');
		}

		if(Attos.Sign == -1)
			o = '-' + o;

		return o;
	}

	public void Read(string text)
	{
		var t = text;
		
		if(t[0] == '-')
			t = t.Substring(1);

		var i = t.IndexOf('.');

		if(i == -1)
		{
			Attos = BigInteger.Parse(t) * One;
		} 
		else
		{
			if(t.Length - i - 1 > Precision)
				throw new FormatException();

			Attos = BigInteger.Parse(t.Substring(0, i)) * One + BigInteger.Parse(t.Substring(i + 1).PadRight(Precision, '0'));
		}

		if(text[0] == '-')
			Attos *= -1;

		//Attos = Nethereum.Web3.Web3.Convert.ToWei(BigDecimal.Parse(text));
		//Attos = new BigInteger((double)One * double.Parse(text));
	}

	Unit(BigInteger a)
	{
		Attos = a;
	}

	public Unit(byte[] a)
	{
		Attos = new BigInteger(a);
	}

	public Unit(long n)
	{
		Attos = One * n;
	}

	public Unit(double n)
	{
		Attos = new BigInteger((double)One * n);
	}

	public static Unit FromWei(BigInteger a)
	{
		return new Unit(a);
	}

	public static Unit FromAtto(BigInteger a)
	{
		return new Unit(a);
	}

	public static Unit Parse(string text)
	{
		var m = new Unit();
		m.Read(text);
		return m; 
	}

	public Unit(BinaryReader r)
	{
		if (r == null)
		{
			throw new ArgumentNullException(nameof(r));
		}

		Attos = BigInteger.Zero;
		Read(r);
	}

	public static explicit operator decimal(Unit c)
	{
		return (decimal)c.Attos/(decimal)One;
	}

	public int CompareTo(object obj)
	{
		return Attos.CompareTo(((Unit)obj).Attos);
	}

	public int CompareTo(Unit o)
	{
		return Attos.CompareTo(o.Attos);
	}

	public bool Equals([AllowNull] Unit o)
	{
		return Attos.Equals(o.Attos);
	}

	public void Write(BinaryWriter w)
	{
		if(w == null)
		{
			throw new ArgumentNullException(nameof(w));
		}

		w.Write((byte)Attos.GetByteCount());
		w.Write(Attos.ToByteArray());
	}

	public void Read(BinaryReader r)
	{
		if(r == null)
		{
			throw new ArgumentNullException(nameof(r));
		}

		Attos = new BigInteger(r.ReadBytes(r.ReadByte()));
	}

	public override int GetHashCode()
	{
		return Attos.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		return Attos.Equals(((Unit)obj).Attos);
	}

	public Unit Pow(int p)
	{
		return new Unit(BigInteger.Pow(Attos, p) / BigInteger.Pow(One, p-1));
	}

	public Unit Floor => new Unit(Attos/ One * One);
}

public class UnitJsonConverter : JsonConverter<Unit>
{
	public override Unit Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		return Unit.FromAtto(BigInteger.Parse(reader.GetString()));
	}

	public override void Write(Utf8JsonWriter writer, Unit value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.Attos.ToString());
	}
}
