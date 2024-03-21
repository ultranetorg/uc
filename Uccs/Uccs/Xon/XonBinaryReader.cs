using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Uccs
{
	public class XonBinaryReader : IXonReader
    {
		//IXonValueSerializator		Serializator;
		public XonToken				Current { get; set; }
		List<EBonHeader>			Flags = new();
		Stream						Stream;
		BinaryReader				Reader;

		public XonBinaryReader(Stream s)
		{
			Stream = s;
			Reader = new BinaryReader(Stream);
		}
		
		public XonToken Read(IXonValueSerializator serializator)
		{
			//Serializator = serializator;

			if(Stream.Position < Stream.Length)
				Current = XonToken.NodeBegin;
			else
				Current = XonToken.End;

			var sig = Reader.ReadUtf8();

			if(sig == IXonWriter.BonHeader)
			{
				var h = Reader.Read7BitEncodedInt();

				if(h > 0 && serializator is IXonBinaryMetaSerializator s)
				{
					s.DeserializeHeader(Reader.ReadBytes(h));
				}

				return Current;
			}
			else
				Current = XonToken.End;

			return Current;
		}

		public XonToken ReadNext()
		{
			if(Stream.Position < Stream.Length)
			{
				switch(Current)
				{
					case XonToken.NodeBegin:
						var f = (EBonHeader)Reader.ReadByte();
						Flags.Add(f);

						Current = XonToken.NameBegin;
						break;

					case XonToken.NodeEnd:
						if(!Flags.Any()) // if last of roots
							return XonToken.End;

						if(Flags.Last().HasFlag(EBonHeader.Last))
							Current = XonToken.ChildrenEnd;
						else
							Current = XonToken.NodeBegin;

						Flags.Remove(Flags.Last());
						break;

					case XonToken.NameEnd:
						if(Flags.Last().HasFlag(EBonHeader.HasMeta))
							Current = XonToken.MetaBegin;
						else if(Flags.Last().HasFlag(EBonHeader.HasValue))
							Current = XonToken.SimpleValueBegin;
						else if(Flags.Last().HasFlag(EBonHeader.Parent))
							Current = XonToken.ChildrenBegin;
						else
							Current = XonToken.NodeEnd;
						break;

					case XonToken.MetaEnd:
						if(Flags.Last().HasFlag(EBonHeader.HasValue))
							Current = XonToken.SimpleValueBegin;
						else if(Flags.Last().HasFlag(EBonHeader.Parent))
							Current = XonToken.ChildrenBegin;
						else
							Current = XonToken.NodeEnd;
						break;

					case XonToken.ValueEnd:
						if(Flags.Last().HasFlag(EBonHeader.Parent))
							Current = XonToken.ChildrenBegin;
						else
							Current = XonToken.NodeEnd;
						break;

					case XonToken.ChildrenBegin:
						Current = XonToken.NodeBegin;
						break;

					case XonToken.ChildrenEnd:
						Current = XonToken.NodeEnd;
						break;

					case XonToken.ValueBegin:
						Current = XonToken.SimpleValueBegin;
						break;

					case XonToken.SimpleValueEnd:
						Current = XonToken.ValueEnd;
						break;
				}
			}
			else
				Current = XonToken.End;

			return Current;
		}

		public string ParseName()
		{
			Current = XonToken.NameEnd;
		
			return Reader.ReadUtf8();
		}
		
		public object ParseMeta()
		{
			Current = XonToken.MetaEnd;
		
			return Reader.ReadBytes(Reader.Read7BitEncodedInt());
		}

		public object ParseValue()
		{
			int n = 0;

			if(!Flags.Last().HasFlag(EBonHeader.BigValue))
			{
				n = ((byte)Flags.Last() & 0b00000111) + 1;
			}
			else
			{
				n = Reader.Read7BitEncodedInt();
			}

			var v = Reader.ReadBytes(n);
			Current = XonToken.SimpleValueEnd;
		
			return v;
		}
    }
}
