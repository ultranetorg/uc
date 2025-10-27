using System.Net;
using System.Reflection;
using Uccs.Net;

namespace Uccs.Nexus;

internal class NexusApiServer : JsonServer
{
	Nexus Uos;

	public NexusApiServer(Nexus uos, Flow flow) : base(	uos.Settings.Api.ToApiSettings(uos.Settings.Zone, KnownSystem.UosApi), ApiClient.CreateOptions(), flow)
	{
		Uos = uos;
	}

	protected override Type Create(string call)
	{
		return Type.GetType(typeof(NexusApiServer).Namespace + '.' + call) ?? Assembly.GetAssembly(typeof(NodeApc)).GetType(typeof(McvApc).Namespace + '.' + call);
	}

	protected override object Execute(object call, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		if(call is IUosApc u) 
			return u.Execute(Uos, request, response, flow);

		throw new ApiCallException("Unknown call");
	}
}

internal interface IUosApc
{
	public abstract object Execute(Nexus uos, HttpListenerRequest request, HttpListenerResponse response, Flow flow);
}

public class NexusPropertyApc : Net.UosPropertyApc, IUosApc
{
	public object Execute(Nexus uos, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		object o = uos;

		foreach(var i in Path.Split('.'))
		{
			o = o.GetType().GetProperty(i)?.GetValue(o) ?? o.GetType().GetField(i)?.GetValue(o);

			if(o == null)
				throw new NodeException(NodeError.NotFound);
		}

		switch(o)
		{
			case byte[] b:
				return b.ToHex();

			default:
				return o?.ToString();
		}
	}
}

internal class NodeInfoApc : Net.NodeInfoApc, IUosApc
{
	public object Execute(Nexus uos, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		lock(uos)
			return uos.Nodes.Find(i => i.Net == Net);
	}
}
