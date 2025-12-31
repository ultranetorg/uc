using System.IO.Pipes;
using System.Linq.Expressions;
using System.Net;
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
