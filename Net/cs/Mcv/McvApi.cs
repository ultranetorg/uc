using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Uccs.Net
{
	public abstract class McvApc : NodeApc
	{
		public abstract object Execute(McvNode sun, HttpListenerRequest request, HttpListenerResponse response, Flow workflow);

		public override object Execute(Node mcv, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
		{
			return Execute(mcv as McvNode, request, response, workflow);
		}
	}

	public class OperationJsonConverter : JsonConverter<Operation>
	{
		public Func<Operation>	Create;

		public override Operation Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var s = reader.GetString().Split(':');
			var o = ITypeCode.Contruct<Operation>(byte.Parse(s[0]));
 			
			o.Read(new BinaryReader(new MemoryStream(s[1].FromHex())));

			return o;
		}

		public override void Write(Utf8JsonWriter writer, Operation value, JsonSerializerOptions options)
		{
			var s = new MemoryStream();
			var w = new BinaryWriter(s);
			
			value.Write(w);
			
			writer.WriteStringValue(ITypeCode.Codes[value.GetType()] + ":" + s.ToArray().ToHex());
		}
	}

	public abstract class McvApiServer : NodeApiServer
	{
		McvNode Node;
	
		public McvApiServer(McvNode node, Flow workflow, JsonSerializerOptions options = null) : base(node, workflow, options ?? McvApiClient.DefaultOptions)
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
		new public static readonly JsonSerializerOptions DefaultOptions;

		static McvApiClient()
		{
			DefaultOptions = new JsonSerializerOptions{};
			DefaultOptions.IgnoreReadOnlyProperties = true;
			DefaultOptions.TypeInfoResolver = new ApiTypeResolver();

			foreach(var i in ApiClient.DefaultOptions.Converters)
			{
				DefaultOptions.Converters.Add(i);
			}

			DefaultOptions.Converters.Add(new OperationJsonConverter());
		}

		public McvApiClient(HttpClient http, string address, string accesskey) : base(http, address, accesskey)
		{
			Options = DefaultOptions;
		}

		public McvApiClient(string address, string accesskey, int timeout = 30) : base(address, accesskey, timeout)
		{
			Options = DefaultOptions;
		}
	}

	public class RunPeerApc : McvApc
	{
		public override object Execute(McvNode node, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
		{
			lock(node.Lock)
				node.RunPeer();
			
			return null;
		}
	}

	public class McvPropertyApc : McvApc
	{
		public string Path { get; set; }

		public override object Execute(McvNode sun, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
		{
			object o = sun;

			foreach(var i in Path.Split('.'))
			{
				o = o.GetType().GetProperty(i)?.GetValue(o) ?? o.GetType().GetField(i)?.GetValue(o);

				if(o == null)
					throw new NodeException(NodeError.NotFound);
			}

			switch(o)
			{
				case byte[] b:
					return b.ToHex();

				default:
					return o?.ToString();
			}
		}
	}
	public class McvSummaryApc : McvApc
	{
		public int		Limit  { get; set; }

		public override object Execute(McvNode mcv, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
		{
			lock(mcv.Lock)
			{ 
				List<KeyValuePair<string, string>> f = [new ("Incoming Transactions",	$"{mcv.IncomingTransactions.Count}"),
														new ("Outgoing Transactions",	$"{mcv.OutgoingTransactions.Count}"),
														new ("    Pending Delegation",	$"{mcv.OutgoingTransactions.Count(i => i.Status == TransactionStatus.Pending)}"),
														new ("    Accepted",			$"{mcv.OutgoingTransactions.Count(i => i.Status == TransactionStatus.Accepted)}"),
														new ("    Placed",				$"{mcv.OutgoingTransactions.Count(i => i.Status == TransactionStatus.Placed)}"),
														new ("    Confirmed",			$"{mcv.OutgoingTransactions.Count(i => i.Status == TransactionStatus.Confirmed)}")];
				
				if(mcv.Mcv != null)
				{
					f.Add(new ("Synchronization",		$"{mcv.Synchronization}"));
					f.Add(new ("Size",					$"{mcv.Mcv.Size}"));
					f.Add(new ("Members",				$"{mcv.Mcv.LastConfirmedRound?.Members.Count}"));
					f.Add(new ("Base Hash",				mcv.Mcv.BaseHash.ToHex()));
					//f.Add(new ("Emission",				$"{mcv.Mcv.LastConfirmedRound?.Emission.ToDecimalString()}"));
					f.Add(new ("Last Confirmed Round",	$"{mcv.Mcv.LastConfirmedRound?.Id}"));
					f.Add(new ("Last Non-Empty Round",	$"{mcv.Mcv.LastNonEmptyRound?.Id}"));
					f.Add(new ("Last Payload Round",	$"{mcv.Mcv.LastPayloadRound?.Id}"));
					f.Add(new ("ExeunitMinFee",			$"{mcv.Mcv.LastConfirmedRound?.ConsensusExeunitFee.ToString()}"));
					f.Add(new ("Loaded Rounds",			$"{mcv.Mcv.LoadedRounds.Count}"));
					f.Add(new ("SyncCache Blocks",		$"{mcv.SyncTail.Sum(i => i.Value.Count)}"));

					if(mcv.Synchronization == Synchronization.Synchronized)
					{
						foreach(var i in mcv.Vault.Wallets)
						{
							var a = i.Key.ToString();
							f.Add(new ($"{a.Substring(0, 8)}...{a.Substring(a.Length - 8, 8)} {(mcv.Vault.IsUnlocked(i.Key) ? "Unlocked" : "Locked")}", null));
							f.Add(new ("   ST", $"{mcv.Mcv.Accounts.Find(i.Key, mcv.Mcv.LastConfirmedRound.Id)?.BYBalance.ToString()}"));
							f.Add(new ("   EU", $"{mcv.Mcv.Accounts.Find(i.Key, mcv.Mcv.LastConfirmedRound.Id)?.ECBalance.ToString()}"));
							f.Add(new ("   MR", $"{mcv.Mcv.Accounts.Find(i.Key, mcv.Mcv.LastConfirmedRound.Id)?.MRBalance.ToString()}"));
						}
					}
				}
				else
				{
					//f.Add(new ("Members (retrieved)", $"{Members.Count}"));

					foreach(var i in mcv.Vault.Wallets)
					{
						f.Add(new ($"Account", $"{i}"));
					}
				}

				mcv.Statistics.Reset();
		
				return new SummaryApc.Return{Summary = f.Take(Limit).Select(i => new [] {i.Key, i.Value}).ToArray() }; 
			}
		}
	}

	public class ChainReportApc : McvApc
	{
		public int		Limit  { get; set; }

		public override object Execute(McvNode mcv, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
		{
			lock(mcv.Lock)
				return new Return {Rounds = mcv.Mcv.Tail.Take(Limit)
														.Reverse()
														.Select(i => new Return.Round
																	{
																		Id = i.Id, 
																		Members = i.Members == null ? 0 : i.Members.Count,
																		Confirmed = i.Confirmed,
																		Time = i.ConsensusTime,
																		Hash = i.Hash,
																		Votes = i.Votes.Select(b => new Return.Vote {	Generator = b.Generator, 
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

		public override object Execute(McvNode mcv, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
		{
			lock(mcv.Lock)
				return new VotesReportResponse{Votes = mcv.Mcv	.FindRound(RoundId)?.Votes
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
		public AccountAddress			By { get; set; }
		public TransactionStatus		Await { get; set; } = TransactionStatus.Confirmed;

		public override object Execute(McvNode mcv, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
		{
			return mcv.Transact(Operations, By, Await, workflow).Select(i => i.Flow.Log.Messages.Select(i => i.ToString()));
		}
	}

	public class EstimateOperationApc : McvApc
	{
		public IEnumerable<Operation>	Operations { get; set; }
		public AccountAddress			By  { get; set; }

		public override object Execute(McvNode mcv, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
		{
			var t = new Transaction {Zone = mcv.Mcv.Zone, Operations = Operations.ToArray()};
			t.Sign(mcv.Vault.GetKey(By), []);

			return mcv.Call(() => new AllocateTransactionRequest {Transaction = t}, workflow);
		}
	}

	public class ApcTransaction
	{
		public int						Nid { get; set; }
		public TransactionId			Id { get; set; }
		public bool						Successful { get; set; }
			
		public EntityId					Member { get; set; }
		public int						Expiration { get; set; }
		public byte[]					PoW { get; set; }
		public byte[]					Tag { get; set; }
		public Unit					EUFee { get; set; }
		public byte[]					Signature { get; set; }
			 
		public AccountAddress			Signer { get; set; }
		public TransactionStatus		Status { get; set; }
		public IPAddress				MemberNexus { get; set; }
		public TransactionStatus		__ExpectedStatus { get; set; }

		public IEnumerable<Operation>	Operations  { get; set; }

		public ApcTransaction()
		{
		}

		public ApcTransaction(Transaction transaction)
		{
			Nid					= transaction.Nid;
			Id					= transaction.Id;
			Operations			= transaction.Operations.ToArray();
			Successful			= transaction.Successful;
			   
			Member				= transaction.Generator;
			Expiration			= transaction.Expiration;
			PoW					= transaction.PoW;
			Tag					= transaction.Tag;
			EUFee				= transaction.EUFee;
			Signature			= transaction.Signature;
			   
			MemberNexus			= (transaction.Rdi as Peer)?.IP ?? (transaction.Rdi as Node)?.IP;
			Signer				= transaction.Signer;
			Status				= transaction.Status;
			__ExpectedStatus	= transaction.__ExpectedStatus;
		}
	}

	public class IncomingTransactionsApc : McvApc
	{
		public override object Execute(McvNode mcv, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
		{
			lock(mcv.Lock)
				return mcv.IncomingTransactions.Select(i => new ApcTransaction(i)).ToArray();
		}
	}

	public class OutgoingTransactionsApc : McvApc
	{
		public override object Execute(McvNode mcv, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
		{
			lock(mcv.Lock)
				return mcv.OutgoingTransactions.Select(i => new ApcTransaction(i)).ToArray();
		}
	}

	public class PeerRequestApc : McvApc
	{
		public PeerRequest Request { get; set; }

		public override object Execute(McvNode mcv, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
		{
			try
			{
				return mcv.Call(() => Request, workflow);
			}
			catch(NetException ex)
			{
				var rp = ITypeCode.Contructors[typeof(PeerResponse)][ITypeCode.Codes[Request.GetType()]].Invoke(null) as PeerResponse;
				rp.Error = ex;
				
				return rp;
			}
		}
	}

	public class SetGeneratorApc : McvApc
	{
		public IEnumerable<AccountAddress>	 Generators {get; set;}

		public override object Execute(McvNode mcv, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
		{
			lock(mcv.Lock)
				mcv.Mcv.Settings.Generators = Generators.ToArray();

			return null;
		}
	}
}
