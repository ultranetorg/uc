using Uccs.Net;
using Uccs.Rdn;

namespace Uccs.Nexus;

public class NexusCommand : NetCommand
{
	public NexusCli	Cli;

	public NexusCommand(NexusCli uos, List<Xon> args, Flow flow) : base(args, flow)
	{
		Cli = uos;

		Flow.Log?.TypesForExpanding.AddRange(  [typeof(IEnumerable<Dependency>), 
												typeof(IEnumerable<AnalyzerResult>), 
												typeof(Resource), 
												typeof(VersionManifest)]);
	}

	public void Api(Apc call)
	{
		if(Has("apitimeout"))
			call.Timeout = GetInt("apitimeout") * 1000;
			
		if(call is INexusApc u)
		{
			u.Execute(Cli.Nexus, null, null, Flow);
			return;
		}

		throw new Exception();
	}

	public Rp Api<Rp>(Apc call)
	{
		if(Has("apitimeout"))
			call.Timeout = GetInt("apitimeout") * 1000;

		if(call is INexusApc u)	return (Rp)u.Execute(Cli.Nexus, null, null, Flow);

		throw new Exception();
	}

//	public Rp RdnRequest<Rp>(Apc call)
//	{
//		if(Has("apitimeout"))
//			call.Timeout = GetInt("apitimeout") * 1000;
//
//		return Nexus.RdnApi.Request<Rp>(call, Flow);
//	}
//
//	public void RdnSend(Apc call)
//	{
//		if(Has("apitimeout"))
//			call.Timeout = GetInt("apitimeout") * 1000;
//
//		Nexus.RdnApi.Send(call, Flow);
//	}

	protected Ura GetResourceAddress(string paramenter, bool mandatory = true)
	{
		if(Has(paramenter))
			return Ura.Parse(GetString(paramenter));
		else
			if(mandatory)
				throw new SyntaxException($"Parameter '{paramenter}' not provided");
			else
				return null;
	}

}
