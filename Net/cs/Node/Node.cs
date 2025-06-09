using System.Diagnostics;
using RocksDbSharp;

namespace Uccs.Net;

public delegate void NodeDelegate(Node node);

public class Node
{
	//public abstract long						Roles { get; }

	public string				Name;
	public Net					Net;
	public string				Profile;
	public Flow					Flow;
	public ApiSettings			ApiSettings;
	public UosApiClient			UosApi;
	public HttpClient			HttpClient;

	public const string			FailureExt = "failure";

	public RocksDb				Database;
	readonly DbOptions			DatabaseOptions	 = new DbOptions()	.SetCreateIfMissing(true)
																	.SetCreateMissingColumnFamilies(true);

	//protected virtual void		CreateTables(ColumnFamilies columns) {}

	//public UosApiClient

	public Node(string name, Net net, string profile, ApiSettings uosapisettings, ApiSettings apisettings, Flow flow)
	{
		Name = name ?? Guid.NewGuid().ToString();
		Net = net;
		Profile = profile;
		Flow = flow;
		ApiSettings = apisettings;

		var cf = new ColumnFamilies();

		//CreateTables(cf);

		if(RocksDb.TryListColumnFamilies(DatabaseOptions, Path.Join(profile, "Node"), out var cfn))
		{	
			foreach(var i in cfn)
			{	
				cf.Add(i, new ());
			}
		}

		Database = RocksDb.Open(DatabaseOptions, Path.Join(profile, "Node"), cf);

		if(uosapisettings != null)
		{
			var h = new HttpClientHandler();
			h.ServerCertificateCustomValidationCallback = (m, c, ch, e) => true;
			HttpClient = new HttpClient(h) { Timeout = Timeout.InfiniteTimeSpan };
	
			UosApi = new UosApiClient(HttpClient, uosapisettings.ListenAddress, uosapisettings.AccessKey);
		}

	}

	public virtual void Stop()
	{
		Database?.Dispose();
	
		Flow.Log?.Report(this, "Stopped");
	}

	public Thread CreateThread(Action action)
	{
		return new Thread(() => { 
									try
									{
										action();
									}
									catch(OperationCanceledException)
									{
									}
									catch(Exception ex) when (!Debugger.IsAttached)
									{
										if(Flow.Active)
											Abort(ex);
									}
								});
	}

	public void Abort(Exception ex)
	{
		File.WriteAllText(Path.Join(Profile, "Abort." + FailureExt), ex.ToString());
		Flow.Log?.ReportError(this, "Abort", ex);

		Stop();
	}
}
