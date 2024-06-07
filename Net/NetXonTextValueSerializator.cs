using System;
using System.Net;

namespace Uccs.Net
{
	public class NetXonTextValueSerializator : XonTextValueSerializator
	{
		public new static readonly XonTextValueSerializator Default = new NetXonTextValueSerializator();

		public override object Set(Xon node, object val)
		{
			if(val == null)
			{
				return null;
			}

			if(	val is AccountAddress	||
				val is Ura	||
				val is Time)
				return val.ToString();
			if(val is Money c)		return c.ToDecimalString();

			return base.Set(node, val);
		}

		public override object Get(Xon node, object value, Type type)
		{
			var v = value as string;

			if(type == typeof(AccountAddress))		return AccountAddress.Parse(v);
			if(type == typeof(Ura))					return Ura.Parse(v);
			if(type == typeof(Time))				return Time.Parse(v);
			if(type == typeof(Money))				return Money.ParseDecimal(v);

			return base.Get(node, value, type);
		}
	}
}
