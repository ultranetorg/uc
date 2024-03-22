using System;
using System.Net;

namespace Uccs.Net
{
	public class SunXonTextValueSerializator : XonTextValueSerializator
	{
		public new static readonly XonTextValueSerializator Default = new SunXonTextValueSerializator();

		public override object Set(Xon node, object val)
		{
			if(val == null)
			{
				return null;
			}

			if(	val is AccountAddress	||
				val is ResourceAddress	||
				val is Time)
				return val.ToString();
			if(val is Money c)		return c.ToHumanString();

			return base.Set(node, val);
		}

		public override O Get<O>(Xon node, object value)
		{
			var v = value as string;

			if(typeof(O) == typeof(AccountAddress))		return (O)(object)AccountAddress.Parse(v);
			if(typeof(O) == typeof(ResourceAddress))	return (O)(object)ResourceAddress.Parse(v);
			if(typeof(O) == typeof(Time))				return (O)(object)Time.Parse(v);
			if(typeof(O) == typeof(Money))				return (O)(object)Money.ParseDecimal(v);

			return base.Get<O>(node, value);
		}
	}
}
