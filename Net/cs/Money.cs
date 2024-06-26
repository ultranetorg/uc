using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Nethereum.Util;

namespace Uccs.Net
{
	public struct Money : IComparable, IComparable<Money>, IEquatable<Money>, IBinarySerializable, ITextSerialisable
	{
		readonly static BigInteger		One = 1_000_000_000_000_000_000;
		public readonly static Money	Zero = new Money();
		public readonly static Money	Min = new Money{Attos = 1};
		public BigInteger				Attos;

		public static implicit operator Money(long value) => new Money(value);
		public static implicit operator Money(double value) => new Money(value);
		//public static implicit operator BigInteger(Money money) => money.Attos / One;

		public static Money operator +	(Money v)			=> v;
		public static Money operator -	(Money v)			=> new Money(-v.Attos);
		public static Money operator +	(Money a, Money b)	=> new Money(a.Attos + b.Attos);
		public static Money operator -	(Money a, Money b)	=> new Money(a.Attos - b.Attos);
		public static Money operator ++	(Money v)			{v.Attos += One; return v; }
		public static Money operator --	(Money v)			{v.Attos -= One; return v; }
		public static Money operator *	(Money a, Money b)	=> new Money(a.Attos * b.Attos / One);
		public static Money operator /	(Money a, Money b)	=> new Money(One * a.Attos / b.Attos);
		public static Money operator %	(Money a, Money b)	=> new Money(a.Attos % b.Attos);
		public static bool operator ==	(Money a, Money b)	=> a.Attos == b.Attos;
		public static bool operator !=	(Money a, Money b)	=> a.Attos != b.Attos;
		public static bool operator <	(Money a, Money b)	=> a.Attos < b.Attos;
		public static bool operator >	(Money a, Money b)	=> a.Attos > b.Attos;
		public static bool operator <=	(Money a, Money b)	=> a.Attos <= b.Attos;
		public static bool operator >=	(Money a, Money b)	=> a.Attos >= b.Attos;

		static Money()
		{
		}

		public void Read(string text)
		{
			Attos = Nethereum.Web3.Web3.Convert.ToWei(BigDecimal.Parse(text));
		}

		Money(BigInteger a)
		{
			Attos = a;
		}

		public Money(byte[] a)
		{
			Attos = new BigInteger(a);
		}

		public Money(BigDecimal d)
		{
			Attos = Nethereum.Web3.Web3.Convert.ToWei(d);
		}

		public Money(long n)
		{
			Attos = One * n;
		}

		public Money(double n)
		{
			Attos = Nethereum.Web3.Web3.Convert.ToWei(n);
		}

		public static Money FromWei(BigInteger a)
		{
			return new Money(a);
		}

		public static Money FromAtto(BigInteger a)
		{
			return new Money(a);
		}

		public static Money ParseDecimal(string text)
		{
			return new Money(BigDecimal.Parse(text)); 
		}

		public Money(BinaryReader r)
		{
			if (r == null)
			{
				throw new ArgumentNullException(nameof(r));
			}

			Attos = BigInteger.Zero;
			Read(r);
		}

		public BigDecimal ToDecimal()
		{
			return Nethereum.Web3.Web3.Convert.FromWeiToBigDecimal(Attos);
		}

		public static explicit operator decimal(Money c)
		{
			return (decimal)c.Attos/(decimal)One;
		}

		override public string ToString()
		{
			return ToDecimalString();
		}
		
		public string ToDecimalString()
		{
			return Nethereum.Web3.Web3.Convert.FromWeiToBigDecimal(Attos).ToString();
		}

		public int CompareTo(object obj)
		{
			return Attos.CompareTo(((Money)obj).Attos);
		}

		public int CompareTo([AllowNull] Money o)
		{
			return Attos.CompareTo(o.Attos);
		}

		public bool Equals([AllowNull] Money o)
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
			return Attos.Equals(((Money)obj).Attos);
		}

		public Money Pow(int p)
		{
			return new Money(BigInteger.Pow(Attos, p) / BigInteger.Pow(One, p-1));
		}

		public Money Floor => new Money(Attos/ One * One);
	}

	public class CoinJsonConverter : JsonConverter<Money>
	{
		public override Money Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			return Money.FromAtto(BigInteger.Parse(reader.GetString()));
		}

		public override void Write(Utf8JsonWriter writer, Money value, JsonSerializerOptions options)
		{
			writer.WriteStringValue(value.Attos.ToString());
		}
	}
}
