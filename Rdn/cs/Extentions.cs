using Uccs.Net;

namespace Uccs.Rdn
{
	public static class Extentions
	{
		public static bool IsSet(this long x, RdnRole bit)
		{
			return (x & (long)bit) != 0;
		}

  		//public static IEnumerable<RdnMember> OrderByNearest(this IEnumerable<RdnMember> e, byte[] hash)
  		//{
  		//	return e.OrderBy(i => Bytes.Xor(i.Account.Bytes, new Span<byte>(hash, 0, AccountAddress.Length)), Bytes.Comparer);
  		//}

	}
}
