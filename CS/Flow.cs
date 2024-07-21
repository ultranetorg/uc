using System;
using System.Diagnostics;
using System.Threading;

namespace Uccs
{
	public class Flow : IDisposable
	{
		CancellationTokenSource			CancellationSource;
		public CancellationToken		Cancellation => CancellationSource.Token;
		public Log						Log { get; set; }
		public bool						Aborted => Cancellation.IsCancellationRequested;
		public bool						Active => !Aborted;
		public string					Name;
		Flow							Parent;

		public Flow(string name)
		{
			Name = name;
			CancellationSource = new CancellationTokenSource();
		}

		public Flow(string name, Log log)
		{
			Name = name;
			CancellationSource = new CancellationTokenSource();
			Log = log;
		}

		Flow(string name, Log log, CancellationTokenSource cancellation)
		{
			Name = name;
			CancellationSource = cancellation;
			Log = log;
		}

		public override string ToString()
		{
			return Name + (Active ? " - Active" : " - Aborted");
		}

		public void CancelAfter(int timeout)
		{ 
			if(!Debugger.IsAttached)
				CancellationSource.CancelAfter(timeout);
		}
		
		public Flow CreateNested(string name, Log log = null)
		{
			var a = CancellationTokenSource.CreateLinkedTokenSource(CancellationSource.Token);
			
			return new Flow(name, log ?? Log, a) {Parent = this};
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
