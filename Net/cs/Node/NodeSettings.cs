using System.Diagnostics;
using System.Net;
using System.Reflection;

namespace Uccs.Net
{
	public class PeeringSettings : Settings
	{
		public IPAddress		IP { get; set; }
		public ushort			Port { get; set; }
		public int				PermanentMin { get; set; } = 6;
		public int				PermanentBaseMin { get; set; } = 6;
		public int				PermanentInboundMax { get; set; } = 128;
		public int				InboundMax { get; set; } = 16 * 1024;
		public bool				InitialRandomization { get; set; } = true;

		public PeeringSettings() : base(NetXonTextValueSerializator.Default)
		{
		}
	}

	public abstract class NodeSettings : SavableSettings
	{
		public ApiSettings			Api { get; set; }
		public bool					Log { get; set; }
		public int					RdcQueryTimeout { get; set; } = 5000;
		public int					RdcTransactingTimeout { get; set; } = 5*60*1000;
		public PeeringSettings		Peering { get; set; } = new();

		public NodeSettings() : base(NetXonTextValueSerializator.Default)
		{
		}

		public NodeSettings(string profile) : base(profile, NetXonTextValueSerializator.Default)
		{
			if(Debugger.IsAttached)
			{
				RdcQueryTimeout = int.MaxValue;
				RdcTransactingTimeout = int.MaxValue;
			}
		}
	}

	public class SecretSettings
	{
		public const string		FileName = "Secrets.globals";

		public string			Password;
		public string			EthereumWallet;
		public string			EthereumPassword;

		public string			EthereumProvider;

		string					Path;

		public SecretSettings()
		{
		}

		public SecretSettings(string path)
		{
			Path = path;

			var d = new Xon(File.ReadAllText(path));
			
			Password			= d.Get<string>("Password");

			EthereumProvider	= d.Get<string>("NasProvider");
			EthereumWallet		= d.Get<string>("EmissionWallet");
			EthereumPassword	= d.Get<string>("EmissionPassword");
		}
	}

	public class NodeGlobals
	{
		public static bool				UI;
		public static bool				DisableTimeouts;
		public static bool				ThrowOnCorrupted;
		public static bool				SkipSynchronization;
		public static bool				SkipMigrationVerification;
		public static SecretSettings	Secrets;

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
}
