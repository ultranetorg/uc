using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Uccs.Net;

public abstract class McvApc : NodeApc
{
	public abstract object Execute(McvNode sun, HttpListenerRequest request, HttpListenerResponse response, Flow workflow);

	public override object Execute(Node mcv, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
	{
		return Execute(mcv as McvNode, request, response, workflow);
	}
}

//public class OperationJsonConverter : JsonConverter<Operation>
//{
//	Net Net;
//
//	public OperationJsonConverter(Net net)
//	{
//		Net = net;
//	}
//
//	public override Operation Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
//	{
//		var s = reader.GetString().Split(':');
//		var o = Net.Contruct<Operation>(uint.Parse(s[0]));
// 			
//		o.Read(new BinaryReader(new MemoryStream(s[1].FromHex())));
//
//		return o;
//	}
//
//	public override void Write(Utf8JsonWriter writer, Operation value, JsonSerializerOptions options)
//	{
//		var s = new MemoryStream();
//		var w = new BinaryWriter(s);
//		
//		value.Write(w);
//		
//		writer.WriteStringValue(Net.Codes[value.GetType()] + ":" + s.ToArray().ToHex());
//	}
//}

public abstract class McvApiServer : NodeApiServer
{
	McvNode Node;

	public McvApiServer(McvNode node, ApiSettings settings, Flow workflow, JsonSerializerOptions options = null) : base(node, settings, workflow, options ?? McvApiClient.CreateOptions())
	{
		Node = node;
	}

	protected override Type Create(string call)
	{
		return Type.GetType(typeof(McvApiServer).Namespace + '.' + call) ?? base.Create(call);
	}

	protected override object Execute(object call, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		if(call is McvApc m)
			return m.Execute(Node, request, response, flow);
		else
			return (call as NodeApc).Execute(Node, request, response, flow);
	}
}

public class McvApiClient : ApiClient
{
	new public static JsonSerializerOptions CreateOptions()
	{
		var o = ApiClient.CreateOptions();

		//o.Converters.Add(new OperationJsonConverter(net));
		
		return o;
	}

	public McvApiClient(HttpClient http, string address, string accesskey) : base(http, address, accesskey)
	{
		Options = CreateOptions();
	}

	public McvApiClient(string address, string accesskey, int timeout = 30) : base(address, accesskey, timeout)
	{
		Options = CreateOptions();
	}
}

public class McvPropertyApc : McvApc
{
	public string Path { get; set; }

	public override object Execute(McvNode node, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
	{
		object o = node;

		foreach(var i in Path.Split('.'))
		{
			o = o.GetType().GetProperty(i)?.GetValue(o) ?? o.GetType().GetField(i)?.GetValue(o);

			if(o == null)
				throw new NodeException(NodeError.NotFound);
		}

		return o;
	}
}

public class PeersReportApc : McvApc
{
	public int		Limit { get; set; }

	public override object Execute(McvNode node, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
	{
		lock(node.Peering.Lock)
			return new Return{Peers = node.Peering.Peers.Where(i => i.Status == ConnectionStatus.OK).TakeLast(Limit).Select(i => new Return.Peer   {IP			= i.IP,			
																																					Status		= i.StatusDescription,
																																					PeerRank	= i.PeerRank,
																																					Roles		= i.Roles,
																																					LastSeen	= i.LastSeen,
																																					LastTry		= i.LastTry,
																																					Retries		= i.Retries}).ToArray()}; 
	}

	public class Return
	{
		public class Peer
		{
			public IPAddress	IP { get; set; }
			public string		Status  { get; set; }
			public int			PeerRank { get; set; }
			public DateTime		LastSeen { get; set; }
			public DateTime		LastTry { get; set; }
			public int			Retries { get; set; }
			public long			Roles { get; set; }
		}

		public IEnumerable<Peer> Peers {get; set;}
	}
}

public class McvSummaryApc : McvApc
{
	public int		Limit  { get; set; }

 	public class Return
 	{
 		public IEnumerable<string[]> Summary {get; set;}
 	}

	public override object Execute(McvNode node, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
	{
		List<KeyValuePair<string, string>> f;

		lock(node.Peering.Lock)
		{
			f = [	new ("Peers all/in/out",		$"{node.Peering.Peers.Count}/{node.Peering.Connections.Count(i => i.Inbound )}/{node.Peering.Connections.Count(i => !i.Inbound)} {(node.Peering.MinimalPeersReached ? " MinimalPeersReached" : null)}"),
					new ("IP(Reported):Port",		$"{node.Peering.Settings.IP} ({node.Peering.IP}) : {node.Peering.Settings.Port}"),
					new ("Votes Acceped/Rejected",	$"{node.Peering.Statistics.AcceptedVotes}/{node.Peering.Statistics.RejectedVotes}"),

					new ("Incoming Transactions",	$"{node.Peering.IncomingTransactions.Count}"),
					new ("Outgoing Transactions",	$"{node.Peering.OutgoingTransactions.Count}"),
					new ("    Pending",				$"{node.Peering.OutgoingTransactions.Count(i => i.Status == TransactionStatus.Pending)}"),
					new ("    Accepted",			$"{node.Peering.OutgoingTransactions.Count(i => i.Status == TransactionStatus.Accepted)}"),
					new ("    Placed",				$"{node.Peering.OutgoingTransactions.Count(i => i.Status == TransactionStatus.Placed)}"),
					//new ("    Confirmed",			$"{node.Peering.OutgoingTransactions.Count(i => i.Status == TransactionStatus.Confirmed)}")
					];
		}

		if(node.Mcv != null)
		{
			lock(node.Mcv.Lock)
			{ 
				f.Add(new ("Generators",			$"{string.Join(", ", (object[])node.Mcv.Settings.Generators)}"));
				f.Add(new ("Synchronization",		$"{node.Peering.Synchronization}"));
				f.Add(new ("Size",					$"{node.Mcv.Size}"));
				f.Add(new ("Members",				$"{node.Mcv.LastConfirmedRound?.Members.Count}"));
				f.Add(new ("Base Hash",				node.Mcv.GraphHash.ToHex()));
				//f.Add(new ("Emission",			$"{mcv.Mcv.LastConfirmedRound?.Emission.ToDecimalString()}"));
				f.Add(new ("Last Confirmed Round",	$"{node.Mcv.LastConfirmedRound?.Id}"));
				f.Add(new ("Last Non-Empty Round",	$"{node.Mcv.LastNonEmptyRound?.Id}"));
				f.Add(new ("Last Payload Round",	$"{node.Mcv.LastPayloadRound?.Id}"));
				f.Add(new ("ExeunitMinFee",			$"{node.Mcv.LastConfirmedRound?.ConsensusECEnergyCost.ToString()}"));
				f.Add(new ("Loaded Rounds",			$"{node.Mcv.LoadedRounds.Count}"));
				f.Add(new ("SyncCache Blocks",		$"{node.Peering.SyncTail.Sum(i => i.Value.Count)}"));

/// 				foreach(var i in node.UosApi.Request())
/// 				{
/// 					var a = i.Address.ToString();
/// 
/// 					f.Add(new ($"{a.Substring(0, 8)}...{a.Substring(a.Length - 8, 8)} {(node.Vault.IsUnlocked(i.Address) ? "Unlocked" : "Locked")}", null));
/// 
/// 					if(node.Peering.Synchronization == Synchronization.Synchronized)
/// 					{
/// 						f.Add(new ("   BD",			$"{node.Mcv.Accounts.Find(i.Key, node.Mcv.LastConfirmedRound.Id)?.Spacetime:N}"));
/// 						f.Add(new ("   EC",			$"{node.Mcv.Accounts.Find(i.Key, node.Mcv.LastConfirmedRound.Id)?.Energy:N}"));
/// 						f.Add(new ("   EC (next)",	$"{node.Mcv.Accounts.Find(i.Key, node.Mcv.LastConfirmedRound.Id)?.EnergyNext:N}"));
/// 					}
/// 				}
			}
		}

		node.Peering.Statistics.Reset();
	
		return new Return {Summary = f.Take(Limit).Select(i => new [] {i.Key, i.Value}).ToArray() }; 
	}
}

public class ChainReportApc : McvApc
{
	public int		Limit  { get; set; }

	public override object Execute(McvNode node, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
	{
		lock(node.Mcv.Lock)
			return new Return {Rounds = node.Mcv.Tail.Take(Limit)
													.Reverse()
													.Select(i => new Return.Round
																{
																	Id = i.Id, 
																	Members = i.Members == null ? 0 : i.Members.Count,
																	Confirmed = i.Confirmed,
																	Time = i.ConsensusTime,
																	Hash = i.Hash,
																	Votes = i.Votes.Select(b => new Return.Vote{Generator = b.Generator, 
																															IsPayload = b.Transactions.Any(), 
																															/*Confirmed = i.Confirmed && i.Transactions.Any() && i.ConfirmedPayloads.Contains(b)*/ }),
																	JoinRequests = i.Transactions.SelectMany(i => i.Operations).OfType<CandidacyDeclaration>().Select(i => i.Transaction.Signer),
																})
													.ToArray()}; 
	}

	public class Return
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
			public bool							Confirmed {get; set;}
			public Time							Time {get; set;}
			public byte[]						Hash {get; set;}
			public byte[]						Summary {get; set;}
			public IEnumerable<Vote>			Votes {get; set;}
			public IEnumerable<AccountAddress>	JoinRequests {get; set;}
			//public IEnumerable<AccountAddress>	HubJoinRequests {get; set;}
			//public IEnumerable<AccountAddress>	AnalyzerJoinRequests {get; set;}
		}

		public IEnumerable<Round> Rounds {get; set;}
	}
}

public class VotesReportApc : McvApc
{
	public int		RoundId  { get; set; }
	public int		Limit  { get; set; }

	public override object Execute(McvNode node, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
	{
		lock(node.Mcv.Lock)
			return new VotesReportResponse{Votes = node.Mcv	.FindRound(RoundId)?.Votes
															.OrderBy(i => i.Generator)
															.Take(Limit)
															.Select(i => new VotesReportResponse.Vote
															{
																Try = i.Try,
																ParentSummary = i.ParentHash,
																Signature = i.Signature,
																Generator = i.Generator
															})
															.ToArray()}; 
	}
}

public class VotesReportResponse
{
	public class Vote
	{
		public int				Try { get; set; }
		public byte[]			ParentSummary { get; set; }
		public byte[]			Signature { get; set; }
		public AccountAddress	Generator { get; set; }
	}

	public IEnumerable<Vote> Votes {get; set;}
}

public class TransactApc : McvApc
{
	public IEnumerable<Operation>	Operations { get; set; }
	public AccountAddress			Signer { get; set; }
	public byte[]					Tag { get; set; }
	public bool						Sponsored { get; set; }
	public ActionOnResult			ActionOnResult { get; set; } = ActionOnResult.RetryUntilConfirmed;

	public override object Execute(McvNode node, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		if(!Operations.Any())
			throw new ApiCallException("No operations");

		foreach(var i in Operations)
			i.PreTransact(node, Sponsored, flow);

		var t = node.Peering.Transact(Operations, Signer, Tag, Sponsored, ActionOnResult, flow);
	
		return new TransactionApe(t);
	}
}

public class OutgoingTransactionApc : McvApc
{
	public byte[]	Tag { get; set; }

	public override object Execute(McvNode mcv, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
	{
		lock(mcv.Peering.Lock)
		{
			var t = mcv.Peering.OutgoingTransactions.Find(i => i.Tag != null && i.Tag.SequenceEqual(Tag));

			if(t != null)
			{
				t.Inquired = DateTime.UtcNow;
				return new TransactionApe(t);
			} 
			else
				return null;
		}
	}
}

public class EstimateOperationApc : McvApc
{
	public IEnumerable<Operation>	Operations { get; set; }
	public AccountAddress			By  { get; set; }

	public override object Execute(McvNode node, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
	{
		var t = new Transaction {Net = node.Mcv.Net, Operations = Operations.ToArray()};

		t.Signer = By;
		t.Signature	 = node.UosApi.Request<byte[]>(new AuthorizeApc{Net		= node.Mcv.Net.Name,
																	Account	= By,
																	Session = node.Settings.Sessions.First(i => i.Account == By).Session,
																	Hash	= t.Hashify(),
																	Trust	= Trust.None}, t.Flow);


		///t.Sign(node.Vault.Find(By).Key, []);

		return node.Peering.Call(() => new AllocateTransactionRequest {Transaction = t}, workflow);
	}
}

public class TransactionApe
{
	public TransactionId			Id { get; set; }
	public int						Nid { get; set; }
		
	public AutoId					Member { get; set; }
	public int						Expiration { get; set; }
	public byte[]					Tag { get; set; }
	public long						Bonus { get; set; }
	public byte[]					Signature { get; set; }
		 
	public AccountAddress			Signer { get; set; }
	public TransactionStatus		Status { get; set; }
	public IPAddress				MemberEndpoint { get; set; }
	public ActionOnResult			__ExpectedStatus { get; set; }

	public IEnumerable<Operation>	Operations  { get; set; }
	public LogMessage[]				Log { get; set; }

	public TransactionApe()
	{
	}

	public TransactionApe(Transaction transaction)
	{
		Nid					= transaction.Nid;
		Id					= transaction.Id;
		Operations			= transaction.Operations.ToArray();
		   
		Member				= transaction.Member;
		Expiration			= transaction.Expiration;
		Tag					= transaction.Tag;
		Bonus				= transaction.Bonus;
		Signature			= transaction.Signature;
		   
		MemberEndpoint		= (transaction.Rdi as Peer)?.IP ?? (transaction.Rdi as HomoTcpPeering)?.IP;
		Signer				= transaction.Signer;
		Status				= transaction.Status;
		__ExpectedStatus	= transaction.ActionOnResult;

		Log					= transaction.Flow.Log.Messages.ToArray();
	}
}

public class IncomingTransactionsApc : McvApc
{
	public override object Execute(McvNode node, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
	{
		lock(node.Peering.Lock)
			return node.Peering.IncomingTransactions.Select(i => new TransactionApe(i)).ToArray();
	}
}

public class OutgoingTransactionsApc : McvApc
{
	public override object Execute(McvNode node, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
	{
		lock(node.Peering.Lock)
			return node.Peering.OutgoingTransactions.Select(i => new TransactionApe(i)).ToArray();
	}
}

public class PpcApc : McvApc
{
	public FuncPeerRequest Request { get; set; }

	public override object Execute(McvNode node, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
	{
		try
		{
			return node.Peering.Call(() => Request, workflow);
		}
		catch(NetException ex)
		{
			var rp = node.Peering.Constract(typeof(PeerResponse), node.Peering.TypeToCode(Request.GetType())) as PeerResponse;
			rp.Error = ex;
			
			return rp;
		}
	}
}

public class SetGeneratorApc : McvApc
{
	public IEnumerable<AccountAddress>	 Generators {get; set;}

	public override object Execute(McvNode node, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
	{
		lock(node.Mcv.Lock)
			node.Mcv.Settings.Generators = Generators.ToArray();

		return null;
	}
}

public class EnforceSessionsApc : McvApc
{
	public AccountAddress	 Account {get; set;}

	public override object Execute(McvNode node, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
	{
		lock(node.Peering.Lock)
			node.Peering.GetSession(Account);

		return null;
	}
}
