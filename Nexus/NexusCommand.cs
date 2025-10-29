using Uccs.Net;

namespace Uccs.Nexus;

public class NexusCommand : NetCommand
{
	public Nexus	Nexus;

	public NexusCommand(Nexus uos, List<Xon> args, Flow flow) : base(args, flow)
	{
		Nexus = uos;
	}

	public void Api(Apc call)
	{
		if(Has("apitimeout"))
			call.Timeout = GetInt("apitimeout") * 1000;
			
		if(call is INexusApc u)
		{
			u.Execute(Nexus, null, null, Flow);
			return;
		}

		throw new Exception();
	}

	public Rp Api<Rp>(Apc call)
	{
		if(Has("apitimeout"))
			call.Timeout = GetInt("apitimeout") * 1000;

		if(call is INexusApc u)	return (Rp)u.Execute(Nexus, null, null, Flow);

		throw new Exception();
	}

	public Rp RdnRequest<Rp>(Apc call)
	{
		if(Has("apitimeout"))
			call.Timeout = GetInt("apitimeout") * 1000;

		return Nexus.RdnApi.Request<Rp>(call, Flow);
	}

	public void RdnSend(Apc call)
	{
		if(Has("apitimeout"))
			call.Timeout = GetInt("apitimeout") * 1000;

		Nexus.RdnApi.Send(call, Flow);
	}
}
