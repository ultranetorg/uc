using System.Diagnostics;
using System.Net;
using System.Reflection;
using RocksDbSharp;

namespace Uccs.Net;

public delegate void NodeDelegate(Node node);

public class Node
{
	//public abstract long						Roles { get; }
	public delegate void		Delegate(Node d);

	public string				Name;
	public Net					Net;
	public string				Profile;
	public Flow					Flow;
	public VaultApiClient		VaultApi;
	public HttpClient			HttpClient;
	public Delegate				Stopped;
	public string				ExeDirectory;

	public RocksDb				Database;
	readonly DbOptions			DatabaseOptions	 = new DbOptions()	.SetCreateIfMissing(true)
																	.SetCreateMissingColumnFamilies(true);

	//protected virtual void		CreateTables(ColumnFamilies columns) {}

	//public UosApiClient

	public Node(string name, Net net, string profile, Flow flow)
	{
		Name = name ?? Guid.NewGuid().ToByteArray().ToHex();
		Net = net;
		Profile = profile;
		Flow = flow;
		ExeDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

		var cf = new ColumnFamilies();

		if(RocksDb.TryListColumnFamilies(DatabaseOptions, Path.Join(profile, "Node"), out var cfn))
		{	
			foreach(var i in cfn)
			{	
				cf.Add(i, new ());
			}
		}

		Database = RocksDb.Open(DatabaseOptions, Path.Join(profile, "Node"), cf);
	}

	protected void InitializeUosApi(IPAddress host)
	{
		var h = new HttpClientHandler()
				{
					ServerCertificateCustomValidationCallback = (m, c, ch, e) => true
				};

		HttpClient = new HttpClient(h);
				 
		if(Debugger.IsAttached)
			HttpClient.Timeout = Timeout.InfiniteTimeSpan;
	
		VaultApi = new VaultApiClient(HttpClient, ApiClient.GetAddress(Net.Zone, host, false, KnownSystem.VaultApi), null);
	}

	public virtual void Stop()
	{
		Database?.Dispose();
	
		Flow.Log?.Report(this, "Stopped");

		Stopped?.Invoke(this);
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
		File.WriteAllText(Path.Join(Profile, "Abort." + Cli.FailureExt), ex.ToString());
		Flow.Log?.ReportError(this, "Abort", ex);

		Stop();
	}
}
