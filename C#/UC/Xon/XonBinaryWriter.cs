using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UC.Net;

namespace UC
{
	enum EBonHeader : byte
	{
		Null		= 0b00000000,
		Parent		= 0b10000000,
		Last		= 0b01000000,
		HasMeta		= 0b00100000,
		HasValue	= 0b00010000,
		BigValue	= 0b00001000
	};

	public class XonBinaryWriter : IXonWriter 
	{
		Stream Stream; 
		BinaryWriter Writer;

		public XonBinaryWriter(Stream s)
		{
			Stream = s;
		}

		public void Start()
		{
			Writer = new BinaryWriter(Stream);
			Writer.WriteUtf8(IXonWriter.BonHeader);
		}

		public void Finish()
		{
			Stream.Flush();
		}

		public void Write(Xon root)
		{
			if(root.Serializator is IXonBinaryMetaSerializator s)
			{
				var h = s.SerializeHeader();
				Writer.Write7BitEncodedInt(h.Length);
				Writer.Write(h);
			}
			else
				Writer.Write7BitEncodedInt(0);

			foreach(var i in root.Nodes)
			{
				Write(i, root.Nodes.Last());
			}
		}

		void Write(Xon node, Xon last)
		{
			var f = EBonHeader.Null;
				
			if(node.Nodes.Any())
			{
				f |= EBonHeader.Parent;
			}
		
			if(node == last)
			{
				f |= EBonHeader.Last;
			}

			var v = node.Value as byte[];

			if(v != null)
			{
				f |= EBonHeader.HasValue;

				if(v.Length <= 8)
				{
					f |= (EBonHeader)(v.Length - 1);
				} 
				else
				{
					f |= EBonHeader.BigValue;
				}
			}

			if(node.Meta != null)
			{
				f |= EBonHeader.HasMeta;
			}
				
			Writer.Write((byte)f);
			Writer.WriteUtf8(node.Name);

			if(node.Meta != null && node.Serializator is IXonBinaryMetaSerializator s)
			{
				var m = s.SerializeMeta(node.Meta);
				Writer.Write7BitEncodedInt(m.Length);
				Writer.Write(m);
			}

			if(v != null)
			{
				if(f.HasFlag(EBonHeader.BigValue))
				{
					Writer.Write7BitEncodedInt(v.Length);
				}

				Writer.Write(v);
			}

			foreach(var i in node.Nodes)
			{
				Write(i, node.Nodes.Last());
			}
		}
	}
}
