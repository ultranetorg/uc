namespace Uccs.Net
{

	public abstract class UosException : Exception
	{
		public UosException()
		{
		}

		public UosException(string message) : base(message)
		{
		}
	}	
}
