namespace Uccs.Fair
{
	public class FairXonTextValueSerializator : NetXonTextValueSerializator
	{
		public new static readonly XonTextValueSerializator Default = new FairXonTextValueSerializator();

		public override object Set(Xon node, object val)
		{
			if(val == null)
			{
				return null;
			}

			return base.Set(node, val);
		}

		public override object Get(Xon node, object value, Type type)
		{
			var v = value as string;

			return base.Get(node, value, type);
		}
	}
}
