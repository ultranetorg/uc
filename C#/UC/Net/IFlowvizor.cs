using System.Threading;

namespace UC.Net
{
	public class Flowvizor
	{
		public CancellationTokenSource	Cancellation { get; }
		public Log						Log { get; }
		
		public bool						_IsAborted;
		public bool						IsAborted { get => _IsAborted || (Parent != null && Parent.IsAborted);  }
		public Flowvizor				Parent;

		public Flowvizor(Log log)
		{
			Cancellation = new CancellationTokenSource();
			Log = log;
		}

		public Flowvizor(int cancelafter)
		{
			Cancellation = new CancellationTokenSource(cancelafter);
		}

		public Flowvizor(Log log, CancellationTokenSource cancellation)
		{
			Cancellation = cancellation;
			Log = log;
		}

		public Flowvizor CreateNested()
		{
			var a = CancellationTokenSource.CreateLinkedTokenSource(Cancellation.Token);

			return new Flowvizor(Log, a){Parent = this};
		}

		public void Abort()
		{
			Cancellation.Cancel();
			_IsAborted = true;
		}
	}
}
