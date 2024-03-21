using System.IO;

namespace Uccs
{
	public interface IBinarySerializable
	{
		void	Write(BinaryWriter writer);
		void	Read(BinaryReader reader);

 		public byte[]	Raw {
								get
								{
									var s = new MemoryStream();
									var w = new BinaryWriter(s);
									
									Write(w);
									
									return s.ToArray();
								}
							}
	}

	public interface ITypeCode
	{
		byte TypeCode { get; }
	}
}
