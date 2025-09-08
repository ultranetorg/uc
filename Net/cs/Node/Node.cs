using System.Diagnostics;
using System.Net;
using RocksDbSharp;

namespace Uccs.Net;

public delegate void NodeDelegate(Node node);

public class UosApiSettings : Settings
{
	//public string			ListenAddress { get; set; }
	public IPAddress		LocalIP { get; set; } = new IPAddress([127, 0, 0, 100]);
	public IPAddress		PublicIP { get; set; }
	public string			PublicAccessKey { get; set; }
	public bool				Ssl { get; set; }

	public static ushort	MapPort(Zone zone) => (ushort)((ushort)zone + KnownSystem.UosApi);
	public static string	GetAddress(Zone zone, IPAddress ip, bool ssl) => $@"http{(ssl ? "s" : "")}://{ip}:{MapPort(zone)}";
	//public string			MapAddress(Zone zone) => MapAddress(zone, IP, Ssl);

	//public ApiSettings		ToApiSettings(Zone zone) => new ApiSettings {AccessKey = AccessKey, ListenAddress = MapAddress(zone)};

	public UosApiSettings() : base(XonTextValueSerializator.Default)
	{
	}
}

public class Node
{
	//public abstract long						Roles { get; }
	public delegate void		Delegate(Node d);

	public string				Name;
	public Net					Net;
	public string				Profile;
	public Flow					Flow;
	public UosApiClient			UosApi;
	public HttpClient			HttpClient;
	public Delegate				Stopped;

	public const string			FailureExt = "failure";

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

	protected void InitializeUosApi(IPAddress uoslocalip)
	{
		var h = new HttpClientHandler()
				{
					ServerCertificateCustomValidationCallback = (m, c, ch, e) => true
				};

		HttpClient = new HttpClient(h);
				 
		if(Debugger.IsAttached)
			HttpClient.Timeout = Timeout.InfiniteTimeSpan;
	
		UosApi = new UosApiClient(HttpClient, UosApiSettings.GetAddress(Net.Zone, uoslocalip, false), null);
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
		File.WriteAllText(Path.Join(Profile, "Abort." + FailureExt), ex.ToString());
		Flow.Log?.ReportError(this, "Abort", ex);

		Stop();
	}
}
