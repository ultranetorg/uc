using System.Net;
using System.Reflection;

namespace Uccs.Net;

public class PeeringSettings : Settings
{
	public Endpoint			EP { get; set; }
	public int				PermanentMin { get; set; } = 6;
	//public int				PermanentGraphsMin { get; set; } = 6;
	public int				PermanentInboundMax { get; set; } = 128;
	public int				InboundMax { get; set; } = 16 * 1024;
	public bool				InitialRandomization { get; set; } = true;

	public PeeringSettings() : base(NetXonTextValueSerializator.Default)
	{
	}
}

//	public class RdnNodeSettings : SavableSettings
//	{
//		public ApiSettings			Api { get; set; }
//		public bool					Log { get; set; }
//		public int					RdcQueryTimeout { get; set; } = 5000;
//		public int					RdcTransactingTimeout { get; set; } = 5*60*1000;
//		public PeeringSettings		Peering { get; set; } = new();
//
//		public NodeSettings() : base(NetXonTextValueSerializator.Default)
//		{
//		}
//
//		public NodeSettings(string profile) : base(profile, NetXonTextValueSerializator.Default)
//		{
//			if(Debugger.IsAttached)
//			{
//				RdcQueryTimeout = int.MaxValue;
//				RdcTransactingTimeout = int.MaxValue;
//			}
//		}
//	}

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
		
		Password			= d.Get<string>("Password");
	}
}

public class NodeGlobals
{
	public static bool				InfiniteTimeouts;
	public static int				TimeoutOnError = 1000;
	public static bool				ThrowOnCorrupted;
	public static bool				SkipMigrationVerification;

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
