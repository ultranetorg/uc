using System;
using System.Threading;

namespace Uccs.Net
{
	public class Workflow : IDisposable
	{
		CancellationTokenSource			CancellationSource;
		public CancellationToken		Cancellation => CancellationSource.Token;
		public Log						Log { get; set; }
		public bool						Aborted => Cancellation.IsCancellationRequested;
		public bool						Active => !Aborted;
		public string					Name;
		Workflow						Parent;

		public Workflow(string name)
		{
			Name = name;
			CancellationSource = new CancellationTokenSource();
		}

		public Workflow(string name, Log log)
		{
			Name = name;
			CancellationSource = new CancellationTokenSource();
			Log = log;
		}

		Workflow(string name, Log log, CancellationTokenSource cancellation)
		{
			Name = name;
			CancellationSource = cancellation;
			Log = log;
		}

		public override string ToString()
		{
			return Name;
		}

		public void CancelAfter(int timeout)
		{ 
			CancellationSource.CancelAfter(timeout);
		}
		
		public Workflow CreateNested(string name, Log log = null)
		{
			var a = CancellationTokenSource.CreateLinkedTokenSource(CancellationSource.Token);
			
			return new Workflow(name, log ?? Log, a) {Parent = this};
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

		public void Dispose()
		{
			CancellationSource.Dispose();
		}
	}
}
