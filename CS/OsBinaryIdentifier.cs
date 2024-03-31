using System.IO;

namespace Uccs.Net
{
	public class Osbi : IBinarySerializable
	{
		public byte Familty;
		public byte Name;
		public byte Version;
		public byte Architecture;

		public void Read(BinaryReader r)
		{
			Familty			= r.ReadByte();
			Name			= r.ReadByte();
			Version			= r.ReadByte();
			Architecture	= r.ReadByte();
		}

		public void Write(BinaryWriter w)
		{
			w.Write(Familty);
			w.Write(Name);
			w.Write(Version);
			w.Write(Architecture);
		}

		public static Osbi Parse(string text)
		{
			var o = new Osbi();

			var f = text.IndexOf(".");
			o.Familty = byte.Parse(text.Substring(0, f));

			var m = text.IndexOf(".", f + 1);
			o.Name = byte.Parse(text.Substring(f + 1, m - f - 1));

			var v = text.IndexOf(".", m + 1);
			o.Version = byte.Parse(text.Substring(m + 1, v - m - 1));

			o.Architecture = byte.Parse(text.Substring(v + 1));

			return o;
		}
	}
}