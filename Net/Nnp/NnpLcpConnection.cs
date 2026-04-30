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

public class NnpLcpConnection : LcpConnection
{
	public static string GetName(IPAddress ip) => "NnpIpp-" + ip.ToString();
	public string Net;

	public NnpLcpConnection(IProgram program, NamedPipeServerStream pipe, LcpServer server, Flow flow, Constructor constructor) : base(program, pipe, server, flow, constructor)
	{
	}

	public NnpLcpConnection(IProgram program, string name, Flow flow) : base(program, name, flow)
	{
		//RegisterHandler(typeof(NnpClass), this);

		Dictionary<Type, Func<string, NnpArgumentation, Result>> ms = [];

		Handler = (from, to, a) =>	{
										if(ms.TryGetValue(a.GetType(), out var e))
										{
											return e(to, a);
										}

										var m = CreateAdapter<Func<string, NnpArgumentation, Result>>(GetType().GetMethods().First(i => i.GetParameters().Length == 2 && 
																																		i.GetParameters()[0].ParameterType == typeof(string) && 
																																		i.GetParameters()[1].ParameterType == a.GetType() && 
																																		i.ReturnType == typeof(Result)));

										ms[a.GetType()] = m;

										return m(to, a);
									};
	}

	TFunc CreateAdapter<TFunc>(MethodInfo mi) where TFunc : Delegate
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

	void Request(string to, LcpRequest request)
	{
		lock(Writer)
		{
			Writer.Write((byte)PacketType.Request);
			Writer.WriteASCII(Net);
			Writer.WriteASCII(to);
			Writer.Write(request.Id);
			Writer.Write(Constructor.TypeToCode(request.Argumentation.GetType()));
			(request.Argumentation as IBinarySerializable).Write(Writer);
			///BinarySerializator.Serialize(Writer, request.Argumentation, Constructor.TypeToCode);
		}
	}

	void Respond(string to, LcpRequest request)
	{
		try
		{
			///var r = Methods[Constructor.TypeToCode(request.Argumentation.GetType())].Invoke(Handler, [this, request.Argumentation]) as Result;
			var r = Handler(Net, to, request.Argumentation as NnpArgumentation);
	
			lock(Writer)
			{
				Writer.Write((byte)PacketType.Response);
				Writer.Write(request.Id);
				Writer.Write(Constructor.TypeToCode(r.GetType()));
				(r as IBinarySerializable).Write(Writer); 
				///BinarySerializator.Serialize(Writer, r, Constructor.TypeToCode);
			}
		}
		catch(Exception ex)
		{
			lock(Writer)
			{
				Writer.Write((byte)PacketType.Failure);
				Writer.Write(request.Id);
				
				if(ex is CodeException)
				{
					BinarySerializator.Serialize(Writer, ex, Constructor.TypeToCode);
				} 
				else
				{
					BinarySerializator.Serialize(Writer, new NnpException(NnpError.ExcutionFailed), Constructor.TypeToCode);
				}
			}
		}
	}

	public override void Listen()
	{
		try
		{
			while(Pipe.IsConnected && Flow.Active)
			{
				var pk = (PacketType)Reader.ReadByte();

//				if(Pipeing.Flow.Aborted || Status != ConnectionStatus.OK)
//					return;

				switch(pk)
				{
 					case PacketType.Request:
 					{
						var rq = new LcpRequest();
						var from = Reader.ReadASCII();
						var me = Reader.ReadASCII();
						rq.Id = Reader.ReadInt32();
						rq.Argumentation = Constructor.Construct(typeof(Argumentation), Reader.ReadUInt32()) as Argumentation;
						(rq.Argumentation as IBinarySerializable).Read(Reader);
						///rq.Argumentation = BinarySerializator.Deserialize<Argumentation>(Reader, Constructor.Construct);
						
						Respond(from, rq);

 						break;
 					}

					case PacketType.Response:
 					{
						var id = Reader.ReadInt32();
						var r = Constructor.Construct(typeof(Result), Reader.ReadUInt32()) as Result;
						(r as IBinarySerializable).Read(Reader);
						///var r = BinarySerializator.Deserialize<Result>(Reader, Constructor.Construct);

						lock(OutRequests)
						{
							var rq = OutRequests.Find(i => i.Id == id);

							if(rq is LcpRequest f)
							{
								f.Return = r;
								f.Event.Set();
 									
								OutRequests.Remove(rq);
							}
						}

						break;
					}

 					case PacketType.Failure:
 					{
						var id = Reader.ReadInt32();
						var ex = BinarySerializator.Deserialize<CodeException>(Reader, Constructor.Construct);

						lock(OutRequests)
						{
							var rq = OutRequests.Find(i => i.Id == id);

							if(rq is LcpRequest f)
							{
								f.Exception = ex;
								f.Event.Set();
 									
								OutRequests.Remove(rq);
							}
						}


 						break;
 					}

					default:
						throw new NodeException(NodeError.Integrity);
				}
			}
		}
		catch(Exception ex)
		{
		}
		finally
		{
			Disconnect();
		}
	}

///	public void Post(IppProcRequest rq)
///	{
///		if(!Pipe.IsConnected)
///			throw new IpcException(IpcError.ConnectionLost);
///
///		rq.Id = IdCounter++;
///
///		Request(rq);
///	}

	public Result Call(string to, Argumentation argumentation, Flow flow)
	{
		if(!Pipe.IsConnected)
			throw new IpcException(IpcError.ConnectionLost);

		var rq = new LcpRequest
				 {
					Argumentation = argumentation,
					Id = IdCounter++
				 };

		lock(OutRequests)
		{
			rq.Event = new ManualResetEvent(false);
			OutRequests.Add(rq);
		}

		Request(to, rq);

		int i = -1;

		try
		{
			i = WaitHandle.WaitAny([rq.Event, flow.Cancellation.WaitHandle, Flow.Cancellation.WaitHandle], NodeGlobals.InfiniteTimeouts ? Timeout.Infinite : 10 * 1000);
		}
		catch(ObjectDisposedException)
		{
			throw new OperationCanceledException();
		}
		finally
		{
			rq.Event.Close();
		}

		if(i == 0)
		{
			if(rq.Exception == null)
			{
				if(rq.Return == null)
					throw new NodeException(NodeError.Connectivity);

				return rq.Return;
			}
			else
			{
				throw rq.Exception;
			}
		}
		else if(i == 1 || i == 2)
			throw new OperationCanceledException();
		else
			throw new IpcException(IpcError.ConnectionLost);
	}
}

public class NnpLcpClientConnection : NnpLcpConnection
{
	//public R Call<A, R>(Nnc<A, R> call, Flow flow) where A : NnpArgumentation, new() where R : Result => Call(call.Argumentation, flow) as R;

	public NnpLcpClientConnection(IProgram program, string name, Flow flow) : base(program, name, flow)
	{
		Constructor = new ();
		Constructor.Register<Argumentation>	(Assembly.GetExecutingAssembly(), typeof(NnpClass), i => i[..^3]);
		Constructor.Register<Result>		(Assembly.GetExecutingAssembly(), typeof(NnpClass), i => i[..^3]);
		Constructor.Register<CodeException>	(Assembly.GetExecutingAssembly(), typeof(ExceptionClass), i => i.Remove(i.IndexOf("Exception")));
	}

	public override void Established()
	{
		Writer.Write(NnpIppConnectionType.Client);
	}

	//public virtual byte[] Transact(Net net, byte[] transaction, Endpoint node, Flow flow)
	//{
	//	return Call(new Nnc<TransactNna, TransactNnr>(	new()
	//													{
	//														Format = PacketFormat.Binary,
	//														Transaction = transaction,
	//														Net	= net.Address,
	//													}),
	//													flow).Result;
	//}
	
	public virtual Result Request(string to, PeerRequest request, Flow flow)
	{
		var s = new MemoryStream();
		var w = new BinaryWriter(s);

		BinarySerializator.Serialize(w, request, Constructor.TypeToCode);

		var rp = Call(	to,
						new RequestNna()
						{
							Format = PacketFormat.Binary,
							Request = s.ToArray(),
						},
						flow) as RequestNnr;

		
		var r = new BinaryReader(new MemoryStream(rp.Response));
		
		return BinarySerializator.Deserialize<Result>(r, Constructor.Construct);
	}
}

public class NnpLcpNodeConnection : NnpLcpConnection
{
	public NnpLcpNodeConnection(IProgram program, string name, Flow flow) : base(program, name, flow)
	{
		Constructor = new ();
		Constructor.Register<Argumentation>	(Assembly.GetExecutingAssembly(), typeof(NnpClass), i => i[..^3]);
		Constructor.Register<Result>		(Assembly.GetExecutingAssembly(), typeof(NnpClass), i => i[..^3]);
		Constructor.Register<CodeException>	(Assembly.GetExecutingAssembly(), typeof(ExceptionClass), i => i.Remove(i.IndexOf("Exception")));
	}
}
