namespace Uccs.Net
{
	public class SynchronizationException : Exception
	{
		public SynchronizationException(string m) : base(typeof(SynchronizationException).Name + " - " + m)
		{
		}

		public SynchronizationException(string m, Exception ex) : base(typeof(SynchronizationException).Name + " - " + m, ex)
		{
		}
	}
}
