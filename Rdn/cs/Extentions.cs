namespace Uccs.Rdn
{
	public static class Extentions
	{
		public static bool IsSet(this long x, RdnRole bit)
		{
			return (x & (long)bit) != 0;
		}

	}
}
