namespace Uccs.Rdn
{
	public class RdnXonTextValueSerializator : NetXonTextValueSerializator
	{
		public new static readonly XonTextValueSerializator Default = new RdnXonTextValueSerializator();

		public override object Set(Xon node, object val)
		{
			if(val == null)
			{
				return null;
			}

			if(
				val is Ura
				)
				return val.ToString();
			if(val is Money c)		return c.ToString();

			return base.Set(node, val);
		}

		public override object Get(Xon node, object value, Type type)
		{
			var v = value as string;

			if(type == typeof(Ura))					return Ura.Parse(v);

			return base.Get(node, value, type);
		}
	}
}
