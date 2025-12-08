using System.Numerics;
using System.Reflection;

namespace Uccs.Net;

public enum NnpIppConnectionType : byte
{
	Unknown = 0,
	Node,
	Client
}

public class NnpIppClientConnection : IppConnection
{
	public R Call<A, R>(Nnc<A, R> call, Flow flow) where A : NnpArgumentation, new() where R : Result => Call(call.Argumentation, flow) as R;

	public NnpIppClientConnection(IProgram program, string name, Flow flow) : base(program, name, flow)
	{
		Constructor = new ();
		Constructor.Register<Argumentation>	(Assembly.GetExecutingAssembly(), typeof(NnpClass), i => i.Remove(i.Length - 3));
		Constructor.Register<Result>		(Assembly.GetExecutingAssembly(), typeof(NnpClass), i => i.Remove(i.Length - 3));
		Constructor.Register<CodeException>	(Assembly.GetExecutingAssembly(), typeof(ExceptionClass), i => i.Remove(i.IndexOf("Exception")));
	}

	public override void Established()
	{
		Writer.Write(NnpIppConnectionType.Client);
	}
}

public class NnpIppNodeConnection : IppConnection
{
	public NnpIppNodeConnection(IProgram program, string name, Flow flow) : base(program, name, flow)
	{
		Constructor = new ();
		Constructor.Register<Argumentation>	(Assembly.GetExecutingAssembly(), typeof(NnpClass), i => i.Remove(i.Length - 3));
		Constructor.Register<Result>		(Assembly.GetExecutingAssembly(), typeof(NnpClass), i => i.Remove(i.Length - 3));
		Constructor.Register<CodeException>	(Assembly.GetExecutingAssembly(), typeof(ExceptionClass), i => i.Remove(i.IndexOf("Exception")));
	}
}

public class McvNnpIppConnection<N, T> : NnpIppNodeConnection where N : McvNode where T : unmanaged, Enum
{
	protected N			Node => Program as N;

	protected string[]	Classes; 
	protected Asset[]	Assets = [new () {Name = nameof(Account.Spacetime), Units = "Byte-days (BD)"},
								  new () {Name = nameof(Account.Energy), Units = "Execution Cycles (EC)"},
								  new () {Name = nameof(Account.EnergyNext), Units = "Execution Cycles (EC)"}];

	public McvNnpIppConnection(N node, string [] classes, Flow flow) : base(node, NnpTcpPeering.GetName(node.NexusSettings.Host), flow)
	{
		RegisterHandler(typeof(NnpClass), this);

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

	public virtual Result Transact(IppConnection connection, PacketNna call)
	{
		var f = Flow.CreateNested(call.Timeout);
		
		var r = new BinaryReader(new MemoryStream(call.Transaction));
		
		var t = Node.Peering.Transact(	r.ReadArray(() =>	{
 														 		var o = Node.Net.Constructor.Construct(typeof(Operation), (byte)r.ReadUInt32()) as Operation;
 														 		o.Read(r); 
 																return o;
															}),
										r.Read<AccountAddress>(),
										r.ReadBytes(),
										r.ReadBoolean(),
										ActionOnResult.RetryUntilConfirmed,
										f);
		
		while(f.Active && t.Status != TransactionStatus.Confirmed)
		{
			Thread.Sleep(10);
		}
		
		return new PacketNnr {Result = t.Tag};
	}
	
	public virtual Result Request(IppConnection connection, PacketNna call)
	{
		var f = Flow.CreateNested(call.Timeout);
		
		var r = new BinaryReader(new MemoryStream(call.Transaction));
		
		var rq = BinarySerializator.Deserialize<PeerRequest>(r, Node.Peering.Constructor.Construct);
		
		var w = new BinaryWriter(new MemoryStream());

		BinarySerializator.Serialize(w, Node.Peering.Call(rq, f), Node.Peering.Constructor.TypeToCode);

		return new PacketNnr {Result = (w.BaseStream as MemoryStream).ToArray()};
	}

	public virtual Result HolderClasses(IppConnection connection, HolderClassesNna call)
	{
		return new HolderClassesNnr {Classes = Classes};
	}

	public virtual Result AssetBalance(IppConnection connection, AssetBalanceNna call)
	{
		if(call.HolderClass != nameof(Account))
			throw new EntityException(EntityError.UnknownClass);

		if(!Assets.Any(i => i.Name == call.Name))
			throw new EntityException(EntityError.UnknownAsset);

		lock(Node.Mcv.Lock)
		{	
			var a = Node.Mcv.Accounts.Latest(AutoId.Parse(call.HolderId));
			
			if(a != null)
				return	new AssetBalanceNnr
						{
							Balance = new BigInteger(call.Name switch
															   {
																	nameof(Account.Spacetime) => a.Spacetime,
																	nameof(Account.Energy) => a.Energy,
																	nameof(Account.EnergyNext) => a.EnergyNext,
															   })
						};
			else
				throw new EntityException(EntityError.NotFound);
		}
	}

	public virtual Result HolderAssets(IppConnection connection, HolderAssetsNna call)
	{
		if(call.HolderClass != nameof(Account))
			throw new NnpException(NnpError.Unknown);

		lock(Node.Mcv.Lock)
		{	
			if(!AutoId.TryParse(call.HolderId, out var id))
				throw new NnpException(NnpError.NotFound);

			var a = Node.Mcv.Accounts.Latest(id);
			
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
			var a = Node.Mcv.Accounts.Latest(new AccountAddress(call.Address));

			if(a != null)
				return new HoldersByAccountNnr {Holders = [new AssetHolder {Class = nameof(Account), Id = a.Id.ToString()}]};
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
					Signer = call.Signer, 
					Tag = Guid.CreateVersion7().ToByteArray(),
					Operations = [new UtilityTransfer
								 {
								 	FromTable	= (byte)Enum.Parse(typeof(T), call.FromClass),
								 	From		= AutoId.Parse(call.FromId),
								 	ToTable		= (byte)Enum.Parse(typeof(T), call.ToClass),
								 	To			= AutoId.Parse(call.ToId),
								 	Energy		= call.Name == nameof(Account.Energy) ? long.Parse(call.Amount) : 0, 
								 	EnergyNext	= call.Name == nameof(Account.EnergyNext) ? long.Parse(call.Amount) : 0,
								 	Spacetime	= call.Name == nameof(Account.Spacetime) ? long.Parse(call.Amount) : 0,
								 }] 
				};

		t.Execute(Node, null, null, Flow);

		var otc = new OutgoingTransactionApc {Tag = t.Tag};

		while((otc.Execute(Node, null, null, Flow) as TransactionApe).Status != TransactionStatus.Confirmed)
		{
			Thread.Sleep(10);
		}

		return new AssetTransferNnr {TransactionId = t.Tag};
	}
}
