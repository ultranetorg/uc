using System.Numerics;
using System.Text;

namespace Uccs.Net;

public class McvIccpLcpConnection: IccpLcpConnection
{
	public McvNode		Node => Program as McvNode;
	public McvPeering	Peering => Node.Peering;
	public Mcv			Mcv => Node.Mcv;

	protected string[]	Classes; 

	public McvIccpLcpConnection(McvNode node, string application, Flow flow) : base(node, GetName(node.NexusSettings.Host), application,  flow)
	{
		Classes = [nameof(User)];

		if(Mcv != null)
		{
			Mcv.FriendTransferFormed += (e, f) =>	{
											 			foreach(var i in node.Settings.Mcv.Generators.Where(i => e.Round.Members.Any(j => j.User == i.Id)))
											 			{
															Task.Run(() =>	{
											 									Call(Net, f.Name, new TransferRequestIcca {Hash = f.LastOutgoingTransfer.Hash
																													/*
																													 * , 
																													 *	Signature = Net.Cryptography.ZeroSignature
																													 *
																													 */}, Flow);
	
																				while(Flow.Active)
																				{
																					var rp = Call(Net, f.Name, new LastIncomingTransferIcca {}, Flow) as LastIncomingTransferIccr;
	
																					if(Bytes.Equal(f.LastOutgoingTransfer.Hash, rp.Result.Hash))
																					{
																						Mcv.FriendTransferResults.Add(rp.Result, f.Name);
																						break;
																					}
	
																					if(rp.Id > f.LastOutgoingTransfer.Id) /// too late, next one is confirmed already
																						break;
	
																					Thread.Sleep(1000);
																				}
																			});
											 			}
													};
		}

	}

	void RequireSynchronized()
	{
		if(Mcv == null)
			throw new IccpException(IccpError.Unavailable);

		if(Peering.Synchronization != Synchronization.Synchronized)
			throw new IccpException(IccpError.NotReady);
	}

	void RequireMembership()
	{
		if(!Peering.IsMember)
			throw new IccpException(IccpError.NotReady);
	}

	public override void Established()
	{
		lock(Writer)
		{
			Writer.Write(IccpLcpConnectionType.Node);
			Writer.WriteUtf8(Node.Net.Address);
			Writer.WriteUtf8(Node.Settings.Api?.LocalNodeAddress(Node.Net));
		}
	}

	public virtual Result Peers(string from, PeersIcca args)
	{
		if(Peering.Synchronization == Synchronization.Synchronized)
		{
			lock(Node.Mcv)
				return new PeersIccr {Peers = [..Node.Mcv.LastConfirmedRound.Members.SelectMany(i => i.GraphPpiEndpoints)]};
		}
		else
			try
			{
				return new PeersIccr {Peers = Node.Peering.Call(new MembersPpc {}, Flow).Members.SelectMany(i => i.GraphPpiEndpoints).ToArray()};
			}
			catch(NodeException)
			{
				throw new IccpException(IccpError.PpcFailure);
			}
	}

	public Result SubnetPeers(string from, SubnetPeersIcca args)
	{
 		if(Peering.Synchronization == Synchronization.Synchronized)
 		{
	 		lock(Mcv.Lock)
			{
				var f = Mcv.Friends.Latest(args.Name)
						??
						throw new IccpException(IccpError.NotFound);
	
				return new SubnetPeersIccr {Peers = f.Peers};
			}
 		} 
 		else
	 		return new SubnetPeersIccr {Peers = Peering.Call(new SubnetPeersPpc {Name = args.Name}, Flow).Endpoints};
	}

 	public Result TransferRequest(string from, TransferRequestIcca args) /// A message from a subnet to vote for
 	{
		RequireSynchronized();

 		lock(Mcv.Lock)
 		{	
			RequireMembership();

 			var t = Mcv.FriendTransferRequests.Find(i => Bytes.Equal(i.Hash, args.Hash));
 
 			if(t != null)
 				return null;
 
 			//if(m.Nonce != args.Nonce - 1)
 			//	throw new EntityException(EntityError.NotSequential);
		}
		
		var rp = Call(Net, from, new LastOutgoingTransferIcca {}, Flow) as LastOutgoingTransferIccr;
			
 		lock(Mcv.Lock)
 		{	
 			///
 			/// TODO : Check signature
 			///

			var f = Mcv.Friends.Latest(from);

			if(Bytes.Equal(f.LastIncomingTransfer.Hash, rp.Transfer.Hash)) /// no new transfer
				return null;
 
			rp.Transfer.From = from;
 			Node.Mcv.FriendTransferRequests.Add(rp.Transfer);
 
 			return null;
 		}
 	}

	public Result LastOutgoingTransfer(string from, LastOutgoingTransferIcca args)
	{
		RequireSynchronized();

		lock(Mcv.Lock)
		{	
			RequireMembership();

			var s = Node.Mcv.Friends.Latest(from)
					??
					throw new IccpException(IccpError.NotFound);

			return new LastOutgoingTransferIccr {Transfer = s.LastOutgoingTransfer};
		}
	}

	public Result LastIncomingTransfer(string from, LastIncomingTransferIcca args) /// Confirmation on our message to a subnet
	{
		RequireSynchronized();

		lock(Mcv.Lock)
		{
			RequireMembership();

			var s = Node.Mcv.Friends.Latest(from)
					??
					throw new IccpException(IccpError.NotFound);

			return new LastIncomingTransferIccr {Result = s.LastIncomingTransfer};
		}
	}

//	public virtual Result Transact(TransactIcca args)
//	{
//		var f = Flow.CreateNested(args.Timeout);
//				
//		Transaction.Import(Node.Net, args.Transaction, Node.Net.Constructor, out var o, out var u, out var a);
//
//		var t = Node.Peering.Transact(o, u, null, ActionOnResult.RetryUntilConfirmed, f);
//		
//		while(f.Active && t.Status != TransactionStatus.Confirmed)
//		{
//			Thread.Sleep(10);
//		}
//		
//		return new TransactIccr {Result = t.Tag};
//	}
	
	public virtual Result Request(string from, RequestIcca args)
	{
		var f = Flow.CreateNested(args.Timeout);
		
		var r = new Reader(new MemoryStream(args.Request), Node.Peering.Constructor);
		var rq = BinarySerializator.Deserialize<PeerRequest>(r);
		
		var w = new Writer(new MemoryStream(), Node.Peering.Constructor);
		BinarySerializator.Serialize(w, Node.Peering.Call(rq, f, null));

		return new RequestIccr {Response = (w.BaseStream as MemoryStream).ToArray()};
	}
	
//	public virtual Result JsonApi(JsonApiIcca args)
//	{
//		
//		ConstructorInfo constuctor;
//		
//		var rp = new JsonApiIccr();
//
//		lock(Calls)
//			if(!Calls.TryGetValue(args.Call, out constuctor))
//			{
//				var t = Type.GetType($"{typeof(JsonServer).Namespace}.{args.Call}{Apc.Postfix}") ?? Create(args.Call + Apc.Postfix);
//
//				if(t == null)
//				{
//					rp.Status = (int)HttpStatusCode.NotFound;
//					goto end;
//				}
//
//				Calls[args.Call] = constuctor = t.GetConstructor(new System.Type[]{});
//			}
//
//		var c = (args.Request.Length > 0 ? JsonSerializer.Deserialize(args.Request, constuctor.DeclaringType, McvApi.CreateOptions()) : constuctor.Invoke(null)) as Apc;
//
//		rp.Status = (int)HttpStatusCode.OK;
//
//		var f = Flow.CreateNested(args.Timeout);
//
//		Execute(call, rq, rp, f);
//
//		rp.Response = JsonSerializer.Serialize(execute(c));
//		
//		end:
//
//		return rp;
//	}

	protected virtual void GetHolder(byte @class, AutoId entity, out ISpacetimeHolder sh, out IEnergyHolder eh)
	{
		RequireSynchronized();

		if(@class == (byte)McvTable.User)
		{
			var a = Node.Mcv.Users.Latest(entity)
					??
					throw new EntityException(EntityError.NotFound);
	
			sh = a;
			eh = a;
		}
		else
			throw new EntityException(EntityError.UnknownClass);

	}

	public virtual Result HolderClasses(string from, HolderClassesIcca args)
	{
		return new HolderClassesIccr {Classes = Classes};
	}

	public virtual Result AssetBalance(string from, AssetBalanceIcca args)
	{
		RequireSynchronized();

		Read(args.Entity, out var c, out var n); 

		if(!args.Asset.SequenceEqual(Asset.Energy(0, Node.Mcv.LastConfirmedRound.ConsensusTime.Years).Id) && 
		   !args.Asset.SequenceEqual(Asset.Energy(1, (byte)(Node.Mcv.LastConfirmedRound.ConsensusTime.Years + 1)).Id) && 
		   !args.Asset.SequenceEqual(Asset.Spacetime.Id))
			throw new EntityException(EntityError.UnknownAsset);

		ISpacetimeHolder sh;
		IEnergyHolder eh;

		Asset e, en;

		lock(Node.Mcv.Lock)
		{	
			GetHolder(c, n, out sh, out eh);

			e = Asset.Energy(0, Node.Mcv.LastConfirmedRound.ConsensusTime.Years);
			en = Asset.Energy(1, (byte)(Node.Mcv.LastConfirmedRound.ConsensusTime.Years + 1));

			if(args.Asset.SequenceEqual(Asset.Spacetime.Id) && sh == null)
				throw new EntityException(EntityError.NotHolder);

			if(args.Asset.SequenceEqual(e.Id) && eh == null)
				throw new EntityException(EntityError.NotHolder);

			if(args.Asset.SequenceEqual(en.Id) && eh == null)
				throw new EntityException(EntityError.NotHolder);
		}

		BigInteger b = 0;
		if(args.Asset.SequenceEqual(Asset.Spacetime.Id))	b = sh.Spacetime; else
		if(args.Asset.SequenceEqual(e.Id))					b = eh.Energy; else
		if(args.Asset.SequenceEqual(en.Id))					b = eh.EnergyNext;
			
		return	new AssetBalanceIccr
				{
					Balance = b
				};
	}

	public virtual Result HolderAssets(string from, HolderAssetsIcca args)
	{
		RequireSynchronized();

		lock(Node.Mcv.Lock)
		{	
			return	new HolderAssetsIccr 
					{
						Assets = [	
									Asset.Spacetime,
									Asset.Energy(0, Node.Mcv.LastConfirmedRound.ConsensusTime.Years),
									Asset.Energy(1, (byte)(Node.Mcv.LastConfirmedRound.ConsensusTime.Years + 1))
								 ]
					};
		}
	}

//	public virtual Result HoldersByAccount(HoldersByAccountIcca args)
//	{
//		lock(Node.Mcv.Lock)
//		{	
//			var a = Node.Mcv.Users.Latest(User.BytesToName(args.Address));
//
//			if(a != null)
//				return new HoldersByAccountIccr {Holders = [EntityAddress.ToString(McvTable.User, a.Id)]};
//			else
//				return new HoldersByAccountIccr {Holders = []};
//		}
//	}

	public virtual Result AddressTextToUniversal(string from, AddressTextToUniversalIcca args)
	{
		RequireSynchronized();

		var i = args.Text.IndexOf('/');

		if(!Enum.TryParse<McvTable>(args.Text.AsSpan(0, i), true, out var t) && !Classes.Any(i => string.Compare(i, t.ToString(), true) == 0))
			throw new EntityException(EntityError.UnknownEntity);

		lock(Node.Mcv.Lock)
		{ 
			var u = Node.Mcv.Users.Latest(args.Text.Substring(i + 1))
					??
					throw new EntityException(EntityError.NotFound);

			return new AddressTextToUniversalIccr {Universal = [(byte)t, ..u.Id.Raw]};
		}
	}

	public void Read(byte[] holder, out byte table, out AutoId name)
	{
		table = holder[0];
		name = new Reader(holder[1..]).Read<AutoId>();
	}

	public virtual Result Transact(string from, TransactIcca args)
	{
 		//foreach(var t in args.Transactions())
 		//{
	 	//	if(t.ToNet == Node.Net.Name)
	 	//	{
	 	//		Parse(args.FromEntity, out var ft, out var fn);
	 	//		Parse(args.ToEntity, out var tt, out var tn);
	 	//
	 	//		if(ft == (byte)McvTable.User && tt == (byte)McvTable.User)
	 	//		{
	 	//			var fu = Node.Peering.Call(new UserPpc {Name = fn}, Flow).User;
	 	//			var tu = Node.Peering.Call(new UserPpc {Name = tn}, Flow).User;
	 	//	
	 	//			var t = new TransactApc
	 	//					{
	 	//						Application = Node.Name,
	 	//						User = fu.Name,
	 	//						Tag = Guid.CreateVersion7().ToByteArray(),
	 	//						Operations = [new UtilityTransfer
	 	//									 {
	 	//										From		= new EntityAddress(ft, fu.Id),
	 	//										To			= new EntityAddress(tt, tu.Id),
	 	//										Energy		= args.Name == nameof(User.Energy) ? long.Parse(args.Amount) : 0, 
	 	//										EnergyNext	= args.Name == nameof(User.EnergyNext) ? long.Parse(args.Amount) : 0,
	 	//										Spacetime	= args.Name == nameof(User.Spacetime) ? long.Parse(args.Amount) : 0,
	 	//									 }] 
	 	//					};
	 	//	
	 	//			t.Execute(Node, null, null, Flow);
	 	//	
	 	//			var otc = new OutgoingTransactionApc {Tag = t.Tag};
	 	//	
	 	//			while((otc.Execute(Node, null, null, Flow) as TransactionApe).Status != TransactionStatus.Confirmed)
	 	//			{
	 	//				Thread.Sleep(1000);
	 	//			}
	 	//	
	 	//			return new AssetTransferIccr {TransactionId = t.Tag};
	 	//		}
	 	//	}
 		//}
	
		throw new IccpException(IccpError.Unavailable);
	}

	public virtual Result ShowGui(string from, ShowGuiIcca args)
	{
		Node.ShowGui?.Invoke();

		return null;
	}

	public virtual Result Do(string from, DoIcca args)
	{
		return new DoIccr {Response = Node.Do(args.Query)};
	}
}
