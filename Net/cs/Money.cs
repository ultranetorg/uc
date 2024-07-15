using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Uccs.Net
{
	public struct Money : IComparable, IComparable<Money>, IEquatable<Money>, IBinarySerializable, ITextSerialisable
	{
		public static BigInteger			One = 1_000_000_000_000_000_000;
		public const byte					Precision = 18;
		public readonly static Money		Zero = new Money();
		public readonly static Money		Min = new Money{Attos = 1};
		public BigInteger					Attos;

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

		Money(BigInteger a)
		{
			Attos = a;
		}

		public Money(byte[] a)
		{
			Attos = new BigInteger(a);
		}

		public Money(long n)
		{
			Attos = One * n;
		}

		public Money(double n)
		{
			Attos = new BigInteger((double)One * n);
		}

		public static Money FromWei(BigInteger a)
		{
			return new Money(a);
		}

		public static Money FromAtto(BigInteger a)
		{
			return new Money(a);
		}

		public static Money Parse(string text)
		{
			var m = new Money();
			m.Read(text);
			return m; 
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

		public static explicit operator decimal(Money c)
		{
			return (decimal)c.Attos/(decimal)One;
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
