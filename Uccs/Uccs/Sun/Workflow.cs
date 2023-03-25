using System;
using System.Threading;

namespace Uccs.Net
{
	public class Workflow
	{
		public CancellationTokenSource	Cancellation { get; }
		public Log						Log { get; }
		

		public Workflow()
		{
			Cancellation = new CancellationTokenSource();
		}

		public Workflow(Log log)
		{
			Cancellation = new CancellationTokenSource();
			Log = log;
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

		public void	ThrowIfAborted()
		{
			if(Cancellation.Token.IsCancellationRequested)
			{
				throw new OperationCanceledException(Cancellation.Token);
			}
		}
	}
}
