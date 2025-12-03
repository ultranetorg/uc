using System.Diagnostics;

namespace Uccs;

public class Flow : IDisposable
{
	public CancellationTokenSource			CancellationSource;
	public CancellationToken		Cancellation;
	public Log						Log { get; set; }
	public bool						Aborted => Cancellation.IsCancellationRequested;
	public bool						Active => !Aborted;
	public string					Name;
	public string					WorkDirectory;
	Flow							Parent;

	public Flow(string name)
	{
		Name = name;
		CancellationSource = new CancellationTokenSource();
	}

	public Flow(int timeout)
	{
		if(!Debugger.IsAttached)
			CancellationSource.CancelAfter(timeout);
	}

	public Flow(CancellationToken cancellationToken)
	{
		Cancellation = cancellationToken;
	}

	public Flow(string name, Log log)
	{
		Name = name;
		Log = log;
		CancellationSource = new CancellationTokenSource();
		Cancellation = CancellationSource.Token;
	}

	Flow(string name, Log log, CancellationTokenSource cancellation)
	{
		Name = name;
		Log = log;
		CancellationSource = cancellation;
		Cancellation = CancellationSource.Token;
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
	
	public Flow CreateNested(string name = null, Log log = null)
	{
		var a = CancellationTokenSource.CreateLinkedTokenSource(CancellationSource.Token);
		
		return new Flow(name, log ?? Log, a) {Parent = this, WorkDirectory = WorkDirectory};
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
		CancellationSource?.Dispose();
	}
}
