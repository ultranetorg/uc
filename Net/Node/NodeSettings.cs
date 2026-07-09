using System.Net;
using System.Reflection;

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
	public const string		FileName = "Secrets.globals";

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
	public static bool				InfiniteTimeouts;
	public static bool				NoWait;
	public static bool				DumpOnError;
	public static int				TimeoutOnError = 1000;
	public static bool				ThrowOnCorrupted;
	public static bool				ForceApproveOutwards;

	public static bool				Any => Fields.Any(i => (bool)i.GetValue(null));
	static IEnumerable<FieldInfo>	Fields => typeof(NodeGlobals).GetFields().Where(i => i.FieldType == typeof(bool));

	public static string			AsString => string.Join(' ', Fields.Select(i => (bool)i.GetValue(null) ? i.Name : null));

	public NodeGlobals()
	{
	}

	public NodeGlobals(Xon x)
	{
		if(x != null)
		{
			foreach(var i in Fields)
			{
				i.SetValue(this, x.Has(i.Name));
			}
		}
	}
}
