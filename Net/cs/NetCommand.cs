using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;

namespace Uccs.Net
{
	public abstract class NetCommand : Command
	{
		protected NetCommand(List<Xon> args, Flow flow) : base(args, flow)
		{
		}

		public AccountAddress GetAccountAddress(string paramenter, bool mandatory = true)
		{
			if(Has(paramenter))
				return AccountAddress.Parse(GetString(paramenter));
			else
				if(mandatory)
					throw new SyntaxException($"Parameter '{paramenter}' not provided");
				else
					return null;
		}

		protected E GetEnum<E>(string paramenter, E def) where E : struct
		{
			var p = One(paramenter);

			if(p != null)
				return Enum.Parse<E>(p.Get<string>());
			else
				return def;
		}

		protected E GetEnum<E>(string paramenter) where E : struct
		{
			var p = One(paramenter);

			if(p != null)
				return Enum.Parse<E>(p.Get<string>());
			else
				throw new SyntaxException($"Parameter '{paramenter}' not provided");
		}
	}
}
