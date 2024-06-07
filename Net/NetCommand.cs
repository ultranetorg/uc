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

		protected Urr GetReleaseAddress(string paramenter, bool mandatory = true)
		{
			if(Has(paramenter))
				return Urr.Parse(GetString(paramenter));
			else
				if(mandatory)
					throw new SyntaxException($"Parameter '{paramenter}' not provided");
				else
					return null;
		}

		protected Money GetMoney(string paramenter)
		{
			var p = One(paramenter);

			if(p != null)
				return Money.ParseDecimal(p.Get<string>());
			else
				throw new SyntaxException($"Parameter '{paramenter}' not provided");
		}

		protected Money GetMoney(string paramenter, Money def)
		{
			var p = One(paramenter);

			if(p != null)
				return Money.ParseDecimal(p.Get<string>());
			else
				return def;
		}

		protected ResourceData GetData()
		{
			var d = One("data");

			if(d != null)
			{
				if(d.Nodes.Any())
				{
					var t = GetEnum<DataType>("data");
					
					switch(t)
					{
						case DataType.Raw:
							return new ResourceData(t, d.Get<string>("bytes").FromHex());

						case DataType.File:
						case DataType.Directory:
						case DataType.Package:
							return new ResourceData(t, Urr.Parse(d.Get<string>("address")));
				
						case DataType.Consil:
							return new ResourceData(t, new Consil  {Analyzers = d.Get<string>("analyzers").Split(',').Select(AccountAddress.Parse).ToArray(),  
																	PerByteFee = d.Get<Money>("fee") });
						case DataType.Analysis:
							return new ResourceData(t, new Analysis {Release = Urr.Parse(d.Get<string>("release")), 
																	 Payment = d.Get<Money>("payment"),
																	 Consil  = d.Get<Ura>("consil")});
						default:
							throw new SyntaxException("Unknown type");
					}
				}
				else if(d.Value != null)
					return new ResourceData(new BinaryReader(new MemoryStream(GetBytes("data"))));
			}

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
