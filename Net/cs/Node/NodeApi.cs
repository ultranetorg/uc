using System.Net;
using System.Text.Json;

namespace Uccs.Net;

public abstract class NodeApc : Apc
{
	public abstract object Execute(Node sun, HttpListenerRequest request, HttpListenerResponse response, Flow workflow);
}

public class NodeApiServer : JsonServer
{
	Node Node;

	public NodeApiServer(Node node, ApiSettings settings, Flow workflow, JsonSerializerOptions options = null) : base(settings, options ?? ApiClient.CreateOptions(), workflow)
	{
		Node = node;
	}
 	
 	protected override Type Create(string call)
 	{
 		return Type.GetType(typeof(NodeApiServer).Namespace + '.' + call);
 	}

	protected override object Execute(object call, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		return (call as NodeApc).Execute(Node, request, response, flow);
	}
}

public class PropertyApc : NodeApc
{
	public string Path { get; set; }

	public override object Execute(Node sun, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
	{
		object o = sun;

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

public class ExceptionApc : NodeApc
{
	public string Reason { get; set; }

	public override object Execute(Node sun, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
	{
		throw new Exception("TEST");
	}
}

public class ExitApc : NodeApc
{
	public string Reason { get; set; }

	public override object Execute(Node sun, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
	{
		sun.Stop();
		return null;
	}
}

public class StartApc : NodeApc
{
	public string Entity { get; set; }

	public override object Execute(Node sun, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
	{
		return null;
	}
}

//public class SettingsApc : NodeApc
//{
//	public override object Execute(Node sun, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
//	{
//		lock(sun.Lock)
//			return new Response{ProfilePath	= sun.Settings.Profile, 
//								Settings	= sun.Settings}; /// TODO: serialize
//	}
//
//	public class Response
//	{
//		public string		ProfilePath {get; set;}
//		public NodeSettings	Settings {get; set;}
//	}
//}

public class LogReportApc : NodeApc
{
	public int		Limit  { get; set; }

	public override object Execute(Node node, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
	{
		lock(node.Flow.Log.Messages)
			return new Response {Log = node.Flow.Log.Messages.TakeLast(Limit).Select(i => i.ToString()).ToArray() }; 
	}

	public class Response
	{
		public IEnumerable<string> Log { get; set; }
	}
}
