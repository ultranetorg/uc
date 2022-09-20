using NativeImport;
using System.IO;

namespace UC.Net
{
	public class OsBinaryIdentifier : IBinarySerializable
	{
		public byte Familty;
		public byte Modification;
		public byte Version;
		public byte Architecture;

		public void Read(BinaryReader r)
		{
			Familty = r.ReadByte();
			Modification = r.ReadByte();
			Version = r.ReadByte();
			Architecture = r.ReadByte();
		}

		public void Write(BinaryWriter w)
		{
			w.Write(Familty);
			w.Write(Modification);
			w.Write(Version);
			w.Write(Architecture);
		}

		public static OsBinaryIdentifier Parse(string text)
		{
			var o = new OsBinaryIdentifier();

			var f = text.IndexOf(".");
			o.Familty = byte.Parse(text.Substring(0, f));

			var m = text.IndexOf(".", f + 1);
			o.Modification = byte.Parse(text.Substring(f + 1, m - f - 1));

			var v = text.IndexOf(".", m + 1);
			o.Version = byte.Parse(text.Substring(m + 1, v - m - 1));

			o.Architecture = byte.Parse(text.Substring(v + 1));

			return o;
		}
	}
}