using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace Uccs.Net
{
	[Flags]
	public enum ResourceFlags : byte
	{
		None		= 0, 
		Sealed		= 0b________1, 
		Deprecated	= 0b_______10, 
		Child		= 0b______100, 
		Data		= 0b_____1000, 

		Unchangables= Child | Data, 
	}

	public enum DataType : short
	{
		None		= 0,
		Redirect	= 1,
		IPAddress	= 2, 
		Uri			= 3,
				
		File		= 100, 
		Directory	= 101, 
		Package		= 102, 

		FirstMime	= 1000, 
	}

	[Flags]
	public enum ResourceChanges : ushort
	{
		None			= 0,
		Flags			= 0b______________1,
		Data			= 0b_____________10,
		NonEmtpyData	= 0b____________100,
		Parent			= 0b___________1000,
		AddPublisher	= 0b__________10000,
		RemovePublisher	= 0b_________100000,
		Recursive		= 0b100000000000000,
	}

	public class ResourceData : IBinarySerializable, IEquatable<ResourceData>
	{
		public DataType		Type;
		byte[]				_Value;
		object				_Interpretation;

 		public byte[] Value
 		{
 			get
 			{
 				if(_Value == null)
 				{
 					var s = new MemoryStream();
 					var w = new BinaryWriter(s);
 			
 					WriteInterpetation(w);
 
 					_Value = s.ToArray();
 				}
 
 				return _Value;
 			}
 		}
		
 		public string Hex
 		{
 			get
 			{
 				var s = new MemoryStream();
 				var w = new BinaryWriter(s);
 			
				w.Write7BitEncodedInt((int)Type);
 				WriteInterpetation(w);
 
 				return s.ToArray().ToHex();
 			}
 		}

		public object Interpretation
		{
			get
			{
				if(_Interpretation == null && _Value != null)
				{
					ReadInterpetation(new BinaryReader(new MemoryStream(_Value)));
				}

				return _Interpretation;
			}
		}

		public ResourceData()
		{
		}

		//public ResourceData(byte[] data)
		//{
		//	Type = (DataType)new BinaryReader(new MemoryStream(data)).Read7BitEncodedInt();
		//	_Value = data;
		//}

		public ResourceData(BinaryReader reader)
		{
			Read(reader);
		}

		public ResourceData(DataType type, object interpretation)
		{
			Type = type;
			_Interpretation = interpretation;
		}

		public ResourceData(DataType type, byte[] value)
		{
			Type = type;
			_Value = value;
		}

// 		public static BinaryReader SkipHeader(byte[] data)
// 		{
// 			var r = new BinaryReader(new MemoryStream(data));
// 			r.Read7BitEncodedInt();
// 			return r;
// 		}

		public override string ToString()
		{
			return $"{Type}, {Value.Length}, {Interpretation}";
		}

		public void Read(BinaryReader reader)
		{
			Type = (DataType)reader.Read7BitEncodedInt();
			ReadInterpetation(reader);
		}

		public void ReadInterpetation(BinaryReader reader)
		{
			switch(Type)
			{
				case DataType.None:
					_Interpretation = reader.ReadBytes();
					break;

				case DataType.Redirect:
				case DataType.Uri:
					_Interpretation = reader.ReadUtf8();
					break;

				case DataType.IPAddress:
					_Interpretation = new IPAddress(reader.ReadBytes());
					break;

				case DataType.File:
				case DataType.Directory:
				case DataType.Package: 
					_Interpretation = ReleaseAddress.FromRaw(reader);
					break;

				default:
					throw new ResourceException(ResourceError.UnknownDataType);
			}
		}

		public void Write(BinaryWriter writer)
		{
			writer.Write7BitEncodedInt((int)Type);
				
			if(_Value != null)
			{
				writer.Write(_Value);
			} 
			else
			{
				WriteInterpetation(writer);
			}
		}

		void WriteInterpetation(BinaryWriter writer)
		{
			switch(Type)
			{
				case DataType.None:
					writer.WriteBytes(Interpretation as byte[]);
					break;
	
				case DataType.Redirect:
				case DataType.Uri:
					writer.WriteUtf8(Interpretation as string);
					break;
				
				case DataType.IPAddress:
					writer.WriteBytes((Interpretation as IPAddress).GetAddressBytes());
					break;
				
				case DataType.File:
				case DataType.Directory:
				case DataType.Package: 
 					writer.Write((byte)(Interpretation as ReleaseAddress).TypeCode);
					(Interpretation as ReleaseAddress).Write(writer);
					break;

				default:
					throw new ResourceException(ResourceError.UnknownDataType);
			}
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Value);
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as ResourceData);
		}

		public bool Equals(ResourceData other)
		{
			return other is not null && Type == other.Type && Value.SequenceEqual(other.Value);
		}

		public static bool operator ==(ResourceData left, ResourceData right)
		{
			return  left is null && right is null || 
					left is not null && left.Equals(right);
		}

		public static bool operator !=(ResourceData left, ResourceData right)
		{
			return !(left == right);
		}
	}

	public class Resource : IBinarySerializable
	{
		public const short		DataLengthMax = 8192;

		public ResourceId		Id { get; set; }
		public ResourceAddress	Address { get; set; }
		public ResourceFlags	Flags { get; set; }
		public ResourceData		Data { get; set; }
		public int[]			Resources { get; set; } = {};

		public override string ToString()
		{
			return $"{Id}, {Address}, [{Flags}], Data={{{Data}}}, Resources={{{Resources.Length}}}";
		}

		public Resource Clone()
		{
			return new() {	Id = Id,
							Address	= Address, 
							Flags = Flags,
							Data = Data,
							Resources = Resources};
		}

		public void Write(BinaryWriter writer)
		{
			writer.Write((byte)Flags);
			
			if(Flags.HasFlag(ResourceFlags.Data))
			{
				writer.Write(Data);
			}

			writer.Write(Resources, i => writer.Write7BitEncodedInt(i));
		}

		public void Read(BinaryReader reader)
		{
			Flags	= (ResourceFlags)reader.ReadByte();
			
			if(Flags.HasFlag(ResourceFlags.Data))
			{
				Data = reader.Read<ResourceData>();
			}

			Resources = reader.ReadArray(() => reader.Read7BitEncodedInt());
		}
	}
}
