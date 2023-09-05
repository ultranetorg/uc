using System;
using System.Threading;

namespace Uccs.Net
{
	public class Workflow
	{
		CancellationTokenSource			CancellationSource;
		public CancellationToken		Cancellation => CancellationSource.Token;
		public Log						Log { get; }
		public bool						Aborted => Cancellation.IsCancellationRequested;
		public bool						Active => !Aborted;

		public Workflow()
		{
			CancellationSource = new CancellationTokenSource();
		}

		public Workflow(Log log)
		{
			CancellationSource = new CancellationTokenSource();
			Log = log;
		}

		public Workflow(Log log, CancellationTokenSource cancellation)
		{
			CancellationSource = cancellation;
			Log = log;
		}

		public Workflow CreateNested()
		{
			var a = CancellationTokenSource.CreateLinkedTokenSource(CancellationSource.Token);
			
			return new Workflow(Log, a);
		}

		public void Abort()
		{
			CancellationSource.Cancel();
		}

		public void	ThrowIfAborted()
		{
			if(CancellationSource.Token.IsCancellationRequested)
			{
				throw new OperationCanceledException(CancellationSource.Token);
			}
		}

		public void	Wait(int timeout)
		{
			if(CancellationSource.Token.IsCancellationRequested)
			{
				throw new OperationCanceledException(CancellationSource.Token);
			}
			else
				Thread.Sleep(timeout);
		}
	}
}
