using System.IO.Pipes;
using System.Linq.Expressions;
using System.Net;
using System.Numerics;
using System.Reflection;

namespace Uccs.Net;

public enum NnpIppConnectionType : byte
{
	Unknown = 0,
	Node,
	Client
}

public abstract class NnpIppConnection : IppConnection
{
	public static string GetName(IPAddress ip) => "NnpIpp-" + ip.ToString();

	protected NnpIppConnection(IProgram program, string name, Flow flow) : base(program, name, flow)
	{
		//RegisterHandler(typeof(NnpClass), this);

		Dictionary<Type, Func<IppConnection, NnpArgumentation, Result>> ms = [];

		Handler =	(c, a) =>
					{
						if(ms.TryGetValue(a.GetType(), out var e))
						{
							return e(c, a);
						}

						var m = CreateAdapter<Func<IppConnection, NnpArgumentation, Result>>(GetType().GetMethods().First(i => i.GetParameters().Length == 2 && i.GetParameters()[1].ParameterType == a.GetType() && i.ReturnType == typeof(Result)));

						ms[a.GetType()] = m;

						return m(c, a);
					};
	}

	public TFunc CreateAdapter<TFunc>(MethodInfo mi) where TFunc : Delegate
	{
		var funcType = typeof(TFunc);
		var invoke = funcType.GetMethod("Invoke")!;

		var delegateParamTypes = invoke.GetParameters().Select(p => p.ParameterType).ToArray();
		var delegateReturnType = invoke.ReturnType;

		var lp = delegateParamTypes.Select(Expression.Parameter).ToArray();

		var methodParams = mi.GetParameters();

		var convertedArgs = methodParams.Select((p, i) => Expression.Convert(lp[i], p.ParameterType)).ToArray();

		var call = mi.IsStatic	? Expression.Call(mi, convertedArgs)
								: Expression.Call(Expression.Constant(this, mi.DeclaringType), mi, convertedArgs);

		Expression body = mi.ReturnType == typeof(void)	? Expression.Block(call, Expression.Default(delegateReturnType))
														: Expression.Convert(call, delegateReturnType);

		return Expression.Lambda<TFunc>(body, lp).Compile();
	}
}

public class NnpIppClientConnection : NnpIppConnection
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

	public virtual byte[] Transact(Net net, byte[] transaction, Endpoint node, Flow flow)
	{
		return Call(new Nnc<TransactNna, TransactNnr>(	new()
														{
															Format = PacketFormat.Binary,
															Transaction = transaction,
															Net	= net.Address,
														}),
														flow).Result;
	}
	
	public virtual Result Request(Net net, PeerRequest request, Flow flow)
	{
		var s = new MemoryStream();
		var w = new BinaryWriter(s);

		BinarySerializator.Serialize(w, request, Constructor.TypeToCode);

		var rp = Call(new Nnc<RequestNna, RequestNnr>(	new()
														{
															Format = PacketFormat.Binary,
															Request = s.ToArray(),
															Net	= net.Address,
														}),
														flow);

		
		var r = new BinaryReader(new MemoryStream(rp.Response));
		
		return BinarySerializator.Deserialize<Result>(r, Constructor.Construct);
	}
}

public class NnpIppNodeConnection : NnpIppConnection
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
	protected Asset[]	Assets = [new () {Name = nameof(User.Spacetime), Units = "Byte-days (BD)"},
								  new () {Name = nameof(User.Energy),	Units = "Execution Cycles (EC)"},
								  new () {Name = nameof(User.EnergyNext),Units = "Execution Cycles (EC)"}];

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

	public virtual Result Transact(IppConnection connection, TransactNna call)
	{
		var f = Flow.CreateNested(call.Timeout);
				
		Transaction.Import(call.Transaction, Node.Net.Constructor, out var o, out var a);

		var t = Node.Peering.Transact(o, a, null, ActionOnResult.RetryUntilConfirmed, f);
		
		while(f.Active && t.Status != TransactionStatus.Confirmed)
		{
			Thread.Sleep(10);
		}
		
		return new TransactNnr {Result = t.Tag};
	}
	
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
