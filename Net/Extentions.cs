using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Uccs.Net
{
	public static class Extentions
	{
		public static bool IsSet(this long x, Role bit)
		{
			return (x & (long)bit) != 0;
		}

		public static bool IsSet(this long x, RdnRole bit)
		{
			return (x & (long)bit) != 0;
		}

		public static Money SumMoney<T>(this IEnumerable<T> e, Func<T, Money> by)
		{
			var s = Money.Zero;

			foreach(var i in e)
			{
				s = s + by(i);
			}

			return s;
		}

		public static T NearestBy<T>(this IEnumerable<T> e, Func<T, AccountAddress> by, AccountAddress account)
		{
			return e.MinBy(m => Bytes.Xor(by(m).Bytes, account.Bytes), Bytes.Comparer);
		}

		public static IEnumerable<MembersResponse.Member> OrderByNearest(this IEnumerable<MembersResponse.Member> e, byte[] hash)
		{
			return e.OrderBy(i => Bytes.Xor(i.Account.Bytes, new Span<byte>(hash, 0, AccountAddress.Length)), Bytes.Comparer);
		}

		public static IEnumerable<Member> OrderByNearest(this IEnumerable<Member> e, byte[] hash)
		{
			return e.OrderBy(i => Bytes.Xor(i.Account.Bytes, new Span<byte>(hash, 0, AccountAddress.Length)), Bytes.Comparer);
		}

		public static byte[] ReadHash(this BinaryReader r)
		{
			return r.ReadBytes(Cryptography.HashSize);
		}

		public static byte[] ReadSignature(this BinaryReader r)
		{
			return r.ReadBytes(Cryptography.SignatureSize);
		}

		public static AccountAddress ReadAccount(this BinaryReader r)
		{
			var a = new AccountAddress();
			a.Read(r);
			return a;
		}

		public static void Write(this BinaryWriter w, AccountAddress a)
		{
			a.Write(w);
		}

		public static void Write(this BinaryWriter w, Money a)
		{
			a.Write(w);
		}
	}
}
