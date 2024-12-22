namespace Uccs.Net;

public static class Extentions
{
	public static bool IsSet(this long x, Role bit)
	{
		return (x & (long)bit) != 0;
	}

	public static Unit SumMoney<T>(this IEnumerable<T> e, Func<T, Unit> by)
	{
		var s = Unit.Zero;

		foreach(var i in e)
		{
			s = s + by(i);
		}

		return s;
	}

	public static T NearestBy<T>(this IEnumerable<T> e, Func<T, AccountAddress> by, AccountAddress account)
	{
		return e.MinBy(m => Cryptography.Hash(by(m).Bytes, account.Bytes), Bytes.Comparer);
	}
 
	public static IEnumerable<T> OrderByHash<T>(this IEnumerable<T> e, Func<T, byte[]> by, byte[] hash)
	{
		return e.OrderBy(i => Cryptography.Hash(by(i), hash), Bytes.Comparer);
	}
//  
// 		public static IEnumerable<Generator> OrderByHash(this IEnumerable<Generator> e, byte[] hash)
// 		{
// 			return e.OrderBy(i => Cryptography.Hash(i.Address.Bytes, hash), Bytes.Comparer);
// 		}
//  
// 		public static IEnumerable<Vote> OrderByHash(this IEnumerable<Vote> e, byte[] hash)
// 		{
// 			return e.OrderBy(i => Cryptography.Hash(i.Generator.Bytes, hash), Bytes.Comparer);
// 		}

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
}
