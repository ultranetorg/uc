using System.IO;

namespace UC.Net
{
	public class RealizationEntry : IBinarySerializable
	{
		public string					Name;
		public OsBinaryIdentifier[]		OSes;

		public void Read(BinaryReader r)
		{
			Name	= r.ReadUtf8();
			OSes	= r.ReadArray<OsBinaryIdentifier>();
		}

		public void Write(BinaryWriter w)
		{
			w.WriteUtf8(Name);
			w.Write(OSes);
		}

		public RealizationEntry Clone()
		{
			return new RealizationEntry{Name = Name, 
										OSes = OSes.Clone() as OsBinaryIdentifier[]};
		}
	}
}
