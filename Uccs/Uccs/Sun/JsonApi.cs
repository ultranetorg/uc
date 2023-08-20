using System;
using System.Linq;
using System.Collections.Generic;
using System.Dynamic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net.Http;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Net;
using System.Collections;
using static System.Collections.Specialized.BitVector32;

namespace Uccs.Net
{
	public abstract class ApiCall
	{
		public string			ProtocolVersion { get; set; }
		public string			AccessKey { get; set; }

		public static string NameOf<C>() => NameOf(typeof(C));
		public static string NameOf(Type type) => type.Name.Remove(type.Name.IndexOf("Call"));
	}

	public class BatchCall : ApiCall
	{
		public class Item
		{
			public string Name { get; set; }
			public dynamic Call { get; set; }
		}

		public IEnumerable<Item> Calls { get; set; }

		public void Add(ApiCall call)
		{
			if(Calls == null)
				Calls = new List<Item>();

			(Calls as List<Item>).Add(new Item {Name = call.GetType().Name.Remove(call.GetType().Name.IndexOf("Call")), Call = call});
		}
	}

	public class ExitCall : ApiCall
	{
		public string Reason { get; set; }
	}

	public class SettingsCall : ApiCall
	{
	}

	public class SettingsResponse
	{
		public string		ProfilePath {get; set;}
		public Settings		Settings {get; set;}
	}

	public class LogReportCall : ApiCall
	{
		public int		Limit  { get; set; }
	}

	public class LogResponse
	{
		public IEnumerable<string> Log {get; set;}
	}

	public class PeersReportCall : ApiCall
	{
		public int		Limit  { get; set; }
	}

	public class PeersResponse
	{
		public IEnumerable<string> Peers {get; set;}
	}

	public class SummaryReportCall : ApiCall
	{
		public int		Limit  { get; set; }
	}

	public class SummaryResponse
	{
		public IEnumerable<string[]> Summary {get; set;}
	}

	public class ChainReportCall : ApiCall
	{
		public int		Limit  { get; set; }
	}

	public class ChainReportResponse
	{
		public class Vote
		{
			public AccountAddress	Generator {get; set;}
			public bool				IsPayload {get; set;}
			//public bool				Confirmed {get; set;}
		}

		public class Round
		{
			public int							Id {get; set;}
			public int							Members {get; set;}
			public int							Analyzers {get; set;}
			public bool							Voted {get; set;}
			public bool							Confirmed {get; set;}
			public ChainTime					Time {get; set;}
			public byte[]						Hash {get; set;}
			public byte[]						Summary {get; set;}
			public IEnumerable<Vote>			Votes {get; set;}
			public IEnumerable<AccountAddress>	JoinRequests {get; set;}
			//public IEnumerable<AccountAddress>	HubJoinRequests {get; set;}
			//public IEnumerable<AccountAddress>	AnalyzerJoinRequests {get; set;}
		}

		public IEnumerable<Round> Rounds {get; set;}
	}

	public class VotesReportCall : ApiCall
	{
		public int		RoundId  { get; set; }
		public int		Limit  { get; set; }
	}

	public class VotesReportResponse
	{
		public class Piece
		{
			public int				Try { get; set; }
			public string			Signature { get; set; }
			public AccountAddress	Generator { get; set; }
		}

		public IEnumerable<Piece> Pieces {get; set;}
	}

	public class RunNodeCall : ApiCall
	{
	}

	public class AddWalletCall : ApiCall
	{
		public AccountAddress	Account { get; set; }
		public byte[]			Wallet { get; set; }
	}

	public class UnlockWalletCall : ApiCall
	{
		public AccountAddress	Account { get; set; }
		public string	Password { get; set; }
	}

	public class SetGeneratorCall : ApiCall
	{
		public IEnumerable<AccountAddress>	 Generators {get; set;}
	}

	public class UntTransferCall : ApiCall
	{
		public AccountAddress	From { get; set; }
		public AccountAddress	To { get; set; }
		public Coin				Amount { get; set; }
	}

	public class QueryResourceCall : ApiCall
	{
		public string		Query { get; set; }
	}

	public class AddReleaseCall : ApiCall
	{
		public PackageAddress	Release { get; set; }
		public byte[]			Complete { get; set; }
		public byte[]			Incremental { get; set; }
		public byte[]			Manifest { get; set; }
	}

	//public class DownloadReleaseCall : ApiCall
	//{
	//	public ReleaseAddress	Release { get; set; }
	//}

	public class PackageStatusCall : ApiCall
	{
		public PackageAddress	Release { get; set; }
		public int			Limit  { get; set; }
	}

	public class InstallPackageCall : ApiCall
	{
		public PackageAddress	Release { get; set; }
	}

	public class GenerateAnalysisReportCall : ApiCall
	{
		public IDictionary<ResourceAddress, AnalysisResult>	Results { get; set; }
	}
}
