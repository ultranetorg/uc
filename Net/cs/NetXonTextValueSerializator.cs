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
				val is Time)
				return val.ToString();
			if(val is Unit c)		return c.ToString();

			return base.Set(node, val);
		}

		public override object Get(Xon node, object value, Type type)
		{
			var v = value as string;

			if(type == typeof(AccountAddress))		return AccountAddress.Parse(v);
			if(type == typeof(Time))				return Time.Parse(v);
			if(type == typeof(Unit))				return Unit.Parse(v);

			return base.Get(node, value, type);
		}
	}
}
