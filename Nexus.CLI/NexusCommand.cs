using Uccs.Net;
using Uccs.Rdn;

namespace Uccs.Nexus.CLI;

public abstract class NexusCommand : NetCommand
{
	public NexusCli	Cli;

	public NexusCommand(NexusCli uos, List<Xon> args, Flow flow) : base(args, flow)
	{
		Cli = uos;

		Flow.Log?.TypesForExpanding.AddRange(  [typeof(IEnumerable<Dependency>), 
												typeof(IEnumerable<AnalyzerReport>), 
												typeof(Resource), 
												typeof(PackageManifest)]);
	}

	public NexusCommand()
	{
	}

	public void Api(Apc call)
	{
		if(Has("apitimeout"))
			call.Timeout = GetInt("apitimeout") * 1000;
			
		if(call is INexusApc u)
		{
			if(Cli.Nexus != null)
				u.Execute(Cli.Nexus, null, null, Flow);
			else
				Cli.NexusApi.Send(call, Flow);

			return;
		}

		throw new Exception();
	}

	public R Api<R>(Apc call)
	{
		if(Has("apitimeout"))
			call.Timeout = GetInt("apitimeout") * 1000;

		if(call is INexusApc u)	
			if(Cli.Nexus != null)
				return (R)u.Execute(Cli.Nexus, null, null, Flow);
			else
				return Cli.NexusApi.Call<R>(call, Flow);

		throw new Exception();
	}

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
