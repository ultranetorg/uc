using System.Threading;

namespace UC.Net
{
	public class Workflow
	{
		public CancellationTokenSource	Cancellation { get; }
		public Log						Log { get; }
		
		public bool						IsAborted { get => Cancellation.Token.IsCancellationRequested; }

		public Workflow(Log log)
		{
			Cancellation = new CancellationTokenSource();
			Log = log;
		}

		public Workflow(int cancelafter)
		{
			Cancellation = new CancellationTokenSource(cancelafter);
		}

		public Workflow(Log log, CancellationTokenSource cancellation)
		{
			Cancellation = cancellation;
			Log = log;
		}

		public Workflow CreateNested()
		{
			var a = CancellationTokenSource.CreateLinkedTokenSource(Cancellation.Token);
			
			return new Workflow(Log, a);
		}

		public void Abort()
		{
			Cancellation.Cancel();
		}
	}
}
