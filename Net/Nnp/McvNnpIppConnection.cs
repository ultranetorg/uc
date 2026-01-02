using System.Numerics;

namespace Uccs.Net;

public class McvNnpIppConnection<N, T> : NnpIppNodeConnection where N : McvNode where T : unmanaged, Enum
{
	protected N			Node => Program as N;
	protected string[]	Classes; 
	protected Asset[]	Assets = [new () {Name = nameof(User.Spacetime),	Units = "Byte-days (BD)"},
								  new () {Name = nameof(User.Energy),		Units = "Execution Cycles (EC)"},
								  new () {Name = nameof(User.EnergyNext),	Units = "Execution Cycles (EC)"}];

	public McvNnpIppConnection(N node, string [] classes, Flow flow) : base(node, GetName(node.NexusSettings.Host), flow)
	{
		Classes = classes;
	}

	public override void Established()
	{
		lock(Writer)
		{
			Writer.Write(NnpIppConnectionType.Node);
			Writer.WriteUtf8(Node.Net.Address);
			Writer.WriteUtf8(Node.Settings.Api.LocalAddress(Node.Net));
		}
	}

	public virtual Result Peers(IppConnection connection, PeersNna call)
	{
		if(Node.Mcv != null)
		{
			lock(Node.Mcv)
				return new PeersNnr {Peers = Node.Mcv.LastConfirmedRound.Members.Select(i => i.GraphPpcIPs[0]).ToArray()};
		}
		else
		{
			return new PeersNnr {Peers = Node.Peering.Call(new MembersPpc {}, Flow).Members.Select(i => i.GraphPpcIPs[0]).ToArray()};
		}
	}

//	public virtual Result Transact(IppConnection connection, TransactNna call)
//	{
//		var f = Flow.CreateNested(call.Timeout);
//				
//		Transaction.Import(Node.Net, call.Transaction, Node.Net.Constructor, out var o, out var u, out var a);
//
//		var t = Node.Peering.Transact(o, u, null, ActionOnResult.RetryUntilConfirmed, f);
//		
//		while(f.Active && t.Status != TransactionStatus.Confirmed)
//		{
//			Thread.Sleep(10);
//		}
//		
//		return new TransactNnr {Result = t.Tag};
//	}
	
	public virtual Result Request(IppConnection connection, RequestNna call)
	{
		var f = Flow.CreateNested(call.Timeout);
		
		var r = new BinaryReader(new MemoryStream(call.Request));
		var rq = BinarySerializator.Deserialize<PeerRequest>(r, Node.Peering.Constructor.Construct);
		
		var w = new BinaryWriter(new MemoryStream());
		BinarySerializator.Serialize(w, Node.Peering.Call(rq, f), Node.Peering.Constructor.TypeToCode);

		return new RequestNnr {Response = (w.BaseStream as MemoryStream).ToArray()};
	}

	public virtual Result HolderClasses(IppConnection connection, HolderClassesNna call)
	{
		return new HolderClassesNnr {Classes = Classes};
	}

	public virtual Result AssetBalance(IppConnection connection, AssetBalanceNna call)
	{
		if(!EntityAddress.TryParse<McvTable>(call.Entity, out var ea) || !Classes.Any(i => i == ((McvTable)ea.Table).ToString())) 
			throw new EntityException(EntityError.UnknownEntity);

		if(!Assets.Any(i => i.Name == call.Name))
			throw new EntityException(EntityError.UnknownAsset);

		lock(Node.Mcv.Lock)
		{	
			var a = Node.Mcv.Users.Latest(ea.Id);
			
			if(a != null)
				return	new AssetBalanceNnr
						{
							Balance = new BigInteger(call.Name switch
															   {
																	nameof(User.Spacetime) => a.Spacetime,
																	nameof(User.Energy) => a.Energy,
																	nameof(User.EnergyNext) => a.EnergyNext,
															   })
						};
			else
				throw new EntityException(EntityError.NotFound);
		}
	}

	public virtual Result HolderAssets(IppConnection connection, HolderAssetsNna call)
	{
		if(!EntityAddress.TryParse<McvTable>(call.Entity, out var ea) || !Classes.Any(i => i == ((McvTable)ea.Table).ToString())) 
			throw new EntityException(EntityError.UnknownEntity);

		lock(Node.Mcv.Lock)
		{	
			var a = Node.Mcv.Users.Latest(ea.Id);
			
			if(a != null)
				return new HolderAssetsNnr{Assets = Assets};
			else
				throw new EntityException(EntityError.NotFound);
		}
	}

	public virtual Result HoldersByAccount(IppConnection connection, HoldersByAccountNna call)
	{
		lock(Node.Mcv.Lock)
		{	
			var a = Node.Mcv.Users.Latest(User.BytesToName(call.Address));

			if(a != null)
				return new HoldersByAccountNnr {Holders = [EntityAddress.ToString(McvTable.User, a.Id)]};
			else
				return new HoldersByAccountNnr {Holders = []};
		}
	}

	public virtual Result AssetTransfer(IppConnection connection, AssetTransferNna call)
	{
		if(call.ToNet != Node.Net.Name)
			throw new NnpException(NnpError.Unavailable);

		var t = new TransactApc
				{
					///User = call.Signer, 
					Tag = Guid.CreateVersion7().ToByteArray(),
					Operations = [new UtilityTransfer
								 {
									From		= EntityAddress.Parse<T>(call.FromEntity),
									To			= EntityAddress.Parse<T>(call.ToEntity),
									Energy		= call.Name == nameof(User.Energy) ? long.Parse(call.Amount) : 0, 
									EnergyNext	= call.Name == nameof(User.EnergyNext) ? long.Parse(call.Amount) : 0,
									Spacetime	= call.Name == nameof(User.Spacetime) ? long.Parse(call.Amount) : 0,
								 }] 
				};

		t.Execute(Node, null, null, Flow);

		var otc = new OutgoingTransactionApc {Tag = t.Tag};

		while((otc.Execute(Node, null, null, Flow) as TransactionApe).Status != TransactionStatus.Confirmed)
		{
			Thread.Sleep(1000);
		}

		return new AssetTransferNnr {TransactionId = t.Tag};
	}
}
