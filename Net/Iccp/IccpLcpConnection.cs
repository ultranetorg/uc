using System.IO.Pipes;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;

namespace Uccs.Net;

public enum IccpLcpConnectionType : byte
{
	Unknown = 0,
	Node,
	Client
}

public class IccpLcpConnection : LcpConnection
{
	public string													Net;
	public string													Api { get; set; }
	Dictionary<Type, Func<string, IccpArgumentation, IccpResult>>	Calls = [];
	
	public static string GetName(IPAddress ip) => "NnpIpp-" + ip.ToString();

	public IccpLcpConnection(IProgram program, NamedPipeServerStream pipe, LcpServer server, Flow flow) : base(program, pipe, server, flow)
	{
	}

	public IccpLcpConnection(IProgram program, string name, Flow flow) : base(program, name, flow)
	{

		Handler = (from, to, a) =>	{
										if(Calls.TryGetValue(a.GetType(), out var e))
										{
											return e(to, a);
										}

										var m = GetType().GetMethods().First(i =>	i.GetParameters().Length == 2 && 
																					i.GetParameters()[0].ParameterType == typeof(string) && 
																					i.GetParameters()[1].ParameterType == a.GetType() && 
																					i.ReturnType == typeof(Result))
												??
												throw new IccpException(IccpError.NotFound);

										var ma = CreateAdapter<Func<string, IccpArgumentation, IccpResult>>(m);
										
										Calls[a.GetType()] = ma;

										return ma(to, a);
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

	void Request(string from, string to, LcpRequest request)
	{
		lock(Writer)
		{
			Writer.Write(PacketType.Request);
			Writer.WriteASCII(from);
			Writer.WriteASCII(to);
			Writer.Write(request.Id);
			Writer.WriteVirtual(request.Argumentation as IccpArgumentation);
		}
	}

	void Respond(string from, string to, LcpRequest request)
	{
		try
		{
			var r = Handler(from, to, request.Argumentation as IccpArgumentation);
	
			lock(Writer)
			{
				Writer.Write(PacketType.Response);
				Writer.Write(request.Id);
				Writer.WriteVirtual(r);
			}
		}
		catch(Exception ex)
		{
			lock(Writer)
			{
				Writer.Write(PacketType.Failure);
				Writer.Write(request.Id);
				
				if(ex is CodeException cex)
				{
					Writer.WriteVirtual(cex);
				} 
				else
				{
					Writer.WriteVirtual(new IccpException(IccpError.ExcutionFailed));
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
				var pk = Reader.Read<PacketType>();

				switch(pk)
				{
 					case PacketType.Request:
 					{
						var rq = new LcpRequest();
						var from = Reader.ReadASCII();
						var to = Reader.ReadASCII();
						rq.Id = Reader.ReadInt32();
						rq.Argumentation = Reader.ReadVirtual<IccpArgumentation>();
						
						Respond(from, to,  rq);

 						break;
 					}

					case PacketType.Response:
 					{
						var id = Reader.ReadInt32();
						var r = Reader.ReadVirtual<IccpResult>();

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
						var ex = Reader.ReadVirtual<CodeException>();

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

	public IccpResult Call(string from, string to, IccpArgumentation argumentation, Flow flow)
	{
		if(!Pipe.IsConnected)
			throw new LcpException(LcpError.ConnectionLost);

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

		Request(from, to, rq);

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

				return rq.Return as IccpResult;
			}
			else
			{
				throw rq.Exception;
			}
		}
		else if(i == 1 || i == 2)
			throw new OperationCanceledException();
		else
			throw new LcpException(LcpError.ConnectionLost);
	}
}

public class IccpLcpClientConnection : IccpLcpConnection
{
	public IccpLcpClientConnection(IProgram program, string name, Flow flow) : base(program, name, flow)
	{
	}

	public override void Established()
	{
		Writer.Write(IccpLcpConnectionType.Client);
	}
	
	public virtual Result Request(string from, string to, PeerRequest request, Flow flow)
	{
		var s = new MemoryStream();
		var w = new Writer(s, Iccp.Constructor);

		BinarySerializator.Serialize(w, request);

		var rp = Call(	from,
						to,
						new RequestIcca()
						{
							Format = PacketFormat.Binary,
							Request = s.ToArray(),
						},
						flow) as RequestIccr;

		
		var r = new Reader(new MemoryStream(rp.Response), Iccp.Constructor);
		
		return BinarySerializator.Deserialize<Result>(r);
	}
}
