using Nethereum.Util;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Uccs.Net
{
	public struct Coin : IComparable, IComparable<Coin>, IEquatable<Coin>, IBinarySerializable
	{
		readonly static BigInteger			One = 1_000_000_000_000_000_000;

		public readonly static Coin			Zero = new Coin();
		public BigInteger					Attos;

		Coin(BigInteger a)
		{
			Attos = a;
		}

		public Coin(byte[] a)
		{
			Attos = new BigInteger(a);
		}

		public Coin(BigDecimal d)
		{
			Attos = Nethereum.Web3.Web3.Convert.ToWei(d);
		}

		public Coin(int n)
		{
			Attos = One * n;
		}

		public Coin(uint n)
		{
			Attos = One * n;
		}

		public Coin(decimal n)
		{
			Attos = Nethereum.Web3.Web3.Convert.ToWei(n);
		}

		public Coin(double n)
		{
			Attos = Nethereum.Web3.Web3.Convert.ToWei(n);
		}

		public static Coin FromWei(BigInteger a)
		{
			return new Coin(a);
		}

		public static Coin FromAtto(BigInteger a)
		{
			return new Coin(a);
		}

		public static Coin ParseDecimal(string text)
		{
			return new Coin(BigDecimal.Parse(text)); 
		}

		public Coin(BinaryReader r)
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
				
		public static implicit operator Coin(int value)
		{
			return new Coin(value);
		}

		public static explicit operator decimal(Coin c)
		{
			return (decimal)c.Attos/(decimal)One;
		}

		override public string ToString()
		{
			return ToHumanString();
		}
		
		public string ToHumanString()
		{
			return Nethereum.Web3.Web3.Convert.FromWeiToBigDecimal(Attos).ToString();
		}

		public int CompareTo(object obj)
		{
			return Attos.CompareTo(((Coin)obj).Attos);
		}

		public int CompareTo([AllowNull] Coin o)
		{
			return Attos.CompareTo(o.Attos);
		}

		public bool Equals([AllowNull] Coin o)
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
			return Attos.Equals(((Coin)obj).Attos);
		}

		public static Coin operator +	(Coin v)			=> v;
		public static Coin operator -	(Coin v)			=> new Coin(-v.Attos);
		public static Coin operator +	(Coin a, Coin b)	=> new Coin(a.Attos + b.Attos);
		public static Coin operator -	(Coin a, Coin b)	=> new Coin(a.Attos - b.Attos);
		public static Coin operator ++	(Coin v)			{v.Attos += One; return v; }
		public static Coin operator --	(Coin v)			{v.Attos -= One; return v; }
		public static Coin operator *	(Coin a, Coin b)	=> new Coin(a.Attos * b.Attos / One);
		public static Coin operator /	(Coin a, Coin b)	=> new Coin(One * a.Attos / b.Attos);
		public static Coin operator %	(Coin a, Coin b)	=> new Coin(a.Attos % b.Attos);
		public static bool operator ==	(Coin a, Coin b)	=> a.Attos == b.Attos;
		public static bool operator !=	(Coin a, Coin b)	=> a.Attos != b.Attos;
		public static bool operator <	(Coin a, Coin b)	=> a.Attos < b.Attos;
		public static bool operator >	(Coin a, Coin b)	=> a.Attos > b.Attos;
		public static bool operator <=	(Coin a, Coin b)	=> a.Attos <= b.Attos;
		public static bool operator >=	(Coin a, Coin b)	=> a.Attos >= b.Attos;

		public Coin Pow(int p)
		{
			return new Coin(BigInteger.Pow(Attos, p) / BigInteger.Pow(One, p-1));
		}

		public Coin Floor => new Coin(Attos/ One * One);
	}

	public class CoinJsonConverter : JsonConverter<Coin>
	{
		public override Coin Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			return Coin.FromAtto(BigInteger.Parse(reader.GetString()));
		}

		public override void Write(Utf8JsonWriter writer, Coin value, JsonSerializerOptions options)
		{
			writer.WriteStringValue(value.Attos.ToString());
		}
	}
}
