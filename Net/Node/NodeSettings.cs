namespace Uccs.Net;

public class PeeringSettings : Settings
{
	public Endpoint			Endpoint { get; set; }
	public int				PermanentMin { get; set; } = 6;
	public int				PermanentInboundMax { get; set; } = 128;
	public int				InboundMax { get; set; } = 16 * 1024;
	public bool				InitialRandomization { get; set; } = true;

	public PeeringSettings() : base(NetXonTextValueSerializator.Default)
	{
	}
}

public class SecretSettings
{
	public string			Password;

	public SecretSettings()
	{
	}

	public SecretSettings(string path)
	{
		var d = new Xon(File.ReadAllText(path));
		
		Password = d.Get<string>("Password");
	}
}

public class NodeGlobals
{
	public static bool					InfiniteTimeouts;
	public static bool					NoWait;
	public static bool					DumpOnError;
	public static int					TimeoutAfterTransactingError = 1000;
	public static bool					CatchCorrupted = true;
	public static bool					ForceApproveOutworlds;
	public static bool					ForceSynchronized;
	public static bool					DumpRoundOnHashify;
	public static Action<Peering>		NodePreRun;

	public NodeGlobals()
	{
	}

	public static string ToString()
	{
		return string.Join(", ", typeof(NodeGlobals).GetFields().Where(i => i.FieldType == typeof(bool) || i.FieldType == typeof(int)).Select(i => $"{i.Name}={i.GetValue(null)}"));
	}
}
