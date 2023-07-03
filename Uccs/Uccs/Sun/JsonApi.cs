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
		public class Block
		{
			public AccountAddress	Generator {get; set;}
			public bool				IsPayload {get; set;}
			public bool				Confirmed {get; set;}
		}

		public class Round
		{
			public int							Id {get; set;}
			public int							Generators {get; set;}
			public int							Hubs {get; set;}
			public int							Analyzers {get; set;}
			public int							Pieces {get; set;}
			public bool							Voted {get; set;}
			public bool							Confirmed {get; set;}
			public ChainTime					Time {get; set;}
			public byte[]						Hash {get; set;}
			public byte[]						Consensus {get; set;}
			public IEnumerable<Block>			Blocks {get; set;}
			public IEnumerable<AccountAddress>	GeneratorJoinRequests {get; set;}
			public IEnumerable<AccountAddress>	HubJoinRequests {get; set;}
			//public IEnumerable<AccountAddress>	AnalyzerJoinRequests {get; set;}
		}

		public IEnumerable<Round> Rounds {get; set;}
	}

	public class PiecesReportCall : ApiCall
	{
		public int		RoundId  { get; set; }
		public int		Limit  { get; set; }
	}

	public class PiecesReportResponse
	{
		public class Piece
		{
			public BlockType		Type { get; set; }
			public int				Try { get; set; }
			public int				Index { get; set; }
			public int				Total { get; set; }
			public string			Signature { get; set; }
			public int				DataLength { get; set; }
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

	public class QueryReleaseCall : ApiCall
	{
		public IEnumerable<ReleaseQuery>	Queries { get; set; }
		public bool							Confirmed { get; set; }
	}

	public class AddReleaseCall : ApiCall
	{
		public ReleaseAddress	Release { get; set; }
		public byte[]			Complete { get; set; }
		public byte[]			Incremental { get; set; }
		public byte[]			Manifest { get; set; }
	}

	//public class DownloadReleaseCall : ApiCall
	//{
	//	public ReleaseAddress	Release { get; set; }
	//}

	public class ReleaseStatusCall : ApiCall
	{
		public ReleaseAddress	Release { get; set; }
		public int				Limit  { get; set; }
	}

	public class GetReleaseCall : ApiCall
	{
		public ReleaseAddress	Release { get; set; }
	}

	public class GenerateAnalysisReportCall : ApiCall
	{
		public IDictionary<ReleaseAddress, AnalysisResult>	Results { get; set; }
	}
}
