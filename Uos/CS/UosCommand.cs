using Uccs.Net;

namespace Uccs.Uos
{
	public class UosCommand : NetCommand
	{
		public Uos					Uos;
		protected override Type[]	TypesForExpanding => [];

		public Guid Mcvid
		{
			get
			{
				if(Has("mcvid"))
					return Guid.Parse(GetString("mcvid"));
				else
					return Uos.Settings.CliDefaultMcv;
			}
		}

		public UosCommand(Uos uos, List<Xon> args, Flow flow) : base(args, flow)
		{
			Uos = uos;
		}

		public void Api(Apc call)
		{
			if(Has("apitimeout"))
				call.Timeout = GetInt("apitimeout") * 1000;
				
			if(call is UosApc u)
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

			if(call is UosApc u)	return (Rp)u.Execute(Uos, null, null, Flow);

			throw new Exception();
		}
	}
}
