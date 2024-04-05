using System;

namespace Uccs.Net
{
	public class ProxyRdi : RdcInterface
	{
		public AccountAddress	Generator { get; protected set; }
		public RdcInterface		Proxy { get; protected set; }

		public ProxyRdi(AccountAddress generator, RdcInterface proxy)
		{
			Generator = generator;
			Proxy = proxy;
		}

		public override RdcResponse Request(RdcRequest rq)
		{
			return Proxy.Request(new ProxyRequest { Destination = Generator, Guid = Guid.NewGuid().ToByteArray(), Request = rq}).Response;
		}

		public override void Send(RdcRequest rq)
		{
			Proxy.Send(new ProxyRequest {Destination = Generator, Request = rq});
		}

		public override string ToString()
		{
			return @$"{Proxy} proxy";
		}
	}
}
