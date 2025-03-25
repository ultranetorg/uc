using Uccs.Net;

namespace Uccs.Uos;

public class UosCommand : NetCommand
{
	public Uos					Uos;
	protected override Type[]	TypesForExpanding => [];

	public UosCommand(Uos uos, List<Xon> args, Flow flow) : base(args, flow)
	{
		Uos = uos;
	}

	public void Api(Apc call)
	{
		if(Has("apitimeout"))
			call.Timeout = GetInt("apitimeout") * 1000;
			
		if(call is IUosApc u)
		{
			u.Execute(Uos, null, null, Flow);
			return;
		}

		throw new Exception();
	}

	public Rp Api<Rp>(Apc call)
	{
		if(Has("apitimeout"))
			call.Timeout = GetInt("apitimeout") * 1000;

		if(call is IUosApc u)	return (Rp)u.Execute(Uos, null, null, Flow);

		throw new Exception();
	}

	public Rp RdnRequest<Rp>(Apc call)
	{
		if(Has("apitimeout"))
			call.Timeout = GetInt("apitimeout") * 1000;

		return Uos.RdnApi.Request<Rp>(call, Flow);
	}

	public void RdnSend(Apc call)
	{
		if(Has("apitimeout"))
			call.Timeout = GetInt("apitimeout") * 1000;

		Uos.RdnApi.Send(call, Flow);
	}
}
