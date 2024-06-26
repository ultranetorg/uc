namespace Uccs.Rdn
{
	public class HubException : Exception
	{
		public HubException(string m) : base(m)
		{
		}

		public HubException(string m, Exception inner) : base(m, inner)
		{
		}
	}
}
