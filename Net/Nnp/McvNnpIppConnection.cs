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

	public virtual Result Peers(IppConnection connection, PeersNna args)
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

//	public virtual Result Transact(IppConnection connection, TransactNna args)
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
//		return new TransactNnr {Result = t.Tag};
//	}
	
	public virtual Result Request(IppConnection connection, RequestNna args)
	{
		var f = Flow.CreateNested(args.Timeout);
		
		var r = new BinaryReader(new MemoryStream(args.Request));
		var rq = BinarySerializator.Deserialize<PeerRequest>(r, Node.Peering.Constructor.Construct);
		
		var w = new BinaryWriter(new MemoryStream());
		BinarySerializator.Serialize(w, Node.Peering.Call(rq, f), Node.Peering.Constructor.TypeToCode);

		return new RequestNnr {Response = (w.BaseStream as MemoryStream).ToArray()};
	}

	public virtual Result HolderClasses(IppConnection connection, HolderClassesNna args)
	{
		return new HolderClassesNnr {Classes = Classes};
	}

	protected virtual void GetHolder(byte c, string n, out ISpacetimeHolder sh, out IEnergyHolder eh)
	{
		sh = null;
		eh = null;

		if(c == (byte)McvTable.User)
		{
			var a = Node.Mcv.Users.Latest(n);
	
			if(a == null)
				throw new EntityException(EntityError.NotFound);
	
			sh = a;
			eh = a;
		}
	}

	public virtual Result AssetBalance(IppConnection connection, AssetBalanceNna args)
	{
		Parse(args.Entity, out var c, out var n); 

		if(!Assets.Any(i => i.Name == args.Name))
			throw new EntityException(EntityError.UnknownAsset);

		ISpacetimeHolder sh;
		IEnergyHolder eh;

		lock(Node.Mcv.Lock)
		{	
			GetHolder(c, n, out sh, out eh);

			if(string.Compare(args.Name, nameof(User.Spacetime),  true) == 0 && sh == null)
				throw new EntityException(EntityError.NotHolder);

			if(string.Compare(args.Name, nameof(User.Energy),  true) == 0 && eh == null)
				throw new EntityException(EntityError.NotHolder);

			if(string.Compare(args.Name, nameof(User.EnergyNext), true) == 0 && eh == null)
				throw new EntityException(EntityError.NotHolder);
		}
			
		return	new AssetBalanceNnr
				{
					Balance = new BigInteger(args.Name switch
														{
															nameof(User.Spacetime) => sh.Spacetime,
															nameof(User.Energy) => eh.Energy,
															nameof(User.EnergyNext) => eh.EnergyNext,
														})
				};
	}

	public virtual Result HolderAssets(IppConnection connection, HolderAssetsNna args)
	{
		Parse(args.Entity, out var c, out var n); 

		lock(Node.Mcv.Lock)
		{	
			return new HolderAssetsNnr{Assets = Assets};
		}
	}

//	public virtual Result HoldersByAccount(IppConnection connection, HoldersByAccountNna args)
//	{
//		lock(Node.Mcv.Lock)
//		{	
//			var a = Node.Mcv.Users.Latest(User.BytesToName(args.Address));
//
//			if(a != null)
//				return new HoldersByAccountNnr {Holders = [EntityAddress.ToString(McvTable.User, a.Id)]};
//			else
//				return new HoldersByAccountNnr {Holders = []};
//		}
//	}

	public void Parse(string text, out byte table, out string name)
	{
		var i = text.IndexOf('/');

		table = 0;
		name = null;

		if(!Enum.TryParse<T>(text.AsSpan(0, i), true, out var t) && !Classes.Any(i => string.Compare(i, t.ToString(), true) == 0))
			throw new EntityException(EntityError.UnknownEntity);

		table = (byte)(object)t;
		name = text.Substring(i + 1);

		if(name.Length == 0)
			throw new EntityException(EntityError.UnknownEntity);
	}

	public virtual Result AssetTransfer(IppConnection connection, AssetTransferNna args)
	{
		if(args.ToNet == Node.Net.Name)
		{
			Parse(args.FromEntity, out var ft, out var fn);
			Parse(args.ToEntity, out var tt, out var tn);

			if(ft == (byte)McvTable.User && tt == (byte)McvTable.User)
			{
				var fu = Node.Peering.Call(new UserPpc {Name = fn}, Flow).User;
				var tu = Node.Peering.Call(new UserPpc {Name = tn}, Flow).User;
		
				var t = new TransactApc
						{
							User = fu.Name,
							Tag = Guid.CreateVersion7().ToByteArray(),
							Operations = [new UtilityTransfer
										 {
											From		= new EntityAddress(ft, fu.Id),
											To			= new EntityAddress(tt, tu.Id),
											Energy		= args.Name == nameof(User.Energy) ? long.Parse(args.Amount) : 0, 
											EnergyNext	= args.Name == nameof(User.EnergyNext) ? long.Parse(args.Amount) : 0,
											Spacetime	= args.Name == nameof(User.Spacetime) ? long.Parse(args.Amount) : 0,
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

		throw new NnpException(NnpError.Unavailable);
	}
}
