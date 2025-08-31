using System.Diagnostics;
using System.Net;
using RocksDbSharp;

namespace Uccs.Net;

public delegate void NodeDelegate(Node node);

public class UosApiSettings : Settings
{
	//public string			ListenAddress { get; set; }
	public string			AccessKey { get; set; }
	public IPAddress		IP { get; set; }
	public ushort			PortPostfix  { get; set; }
	public bool				Ssl  { get; set; }

	public static ushort	MapPort(Zone zone, ushort postfix) => (ushort)((ushort)zone + KnownSystem.UosApi + postfix);
	public static string	MapAddress(Zone zone, IPAddress ip, ushort portpostfix, bool ssl) => $@"http{(ssl ? "s" : "")}://{ip}:{MapPort(zone, portpostfix)}";

	public ushort			MapPort(Zone zone) => MapPort(zone, PortPostfix);
	public string			MapAddress(Zone zone) => MapAddress(zone, IP, PortPostfix, Ssl);

	public ApiSettings		ToApiSettings(Zone zone) => new ApiSettings {AccessKey = AccessKey, ListenAddress = MapAddress(zone)};

	public UosApiSettings() : base(XonTextValueSerializator.Default)
	{
	}
}

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
