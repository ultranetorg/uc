namespace Uccs.Net
{
	public class IntegrityException : Exception
	{
		public IntegrityException()
		{
		}

		public IntegrityException(string m) : base(m)
		{
		}

		public IntegrityException(string m, Exception inner) : base(m, inner)
		{
		}
	}
}
