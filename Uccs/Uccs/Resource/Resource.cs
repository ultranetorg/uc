using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection.Metadata;

namespace Uccs.Net
{
	[Flags]
	public enum ResourceFlags : byte
	{
		None			= 0, 
		Sealed			= 0b_______1, 
		Child			= 0b______10, 
		Data			= 0b_____100, 
		Analysis		= 0b____1000, 
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
	public enum ResourceChanges : byte
	{
		None			= 0,
		Recursive		= 0b______________1,
		NotNullData			= 0b_____________10,
		//Flags			= 0b______________1,
		//Data			= 0b_____________10,
		//Parent			= 0b___________1000,
		//Analysis		= 0b__________10000,
		//AddPublisher	= 0b_________100000,
		//RemovePublisher	= 0b________1000000,
	}

	public class ResourceData : IBinarySerializable, IEquatable<ResourceData>
	{
		public const short	LengthMax = 8192;

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
		public ResourceId			Id { get; set; }
		public ResourceAddress		Address { get; set; }
		public ResourceFlags		Flags { get; set; }
		public ResourceData			Data { get; set; }
		public int[]				Resources { get; set; } = {};
		public Time					Updated { get; set; }

		public Money				AnalysisPayment { get; set; }
		public byte					AnalysisConsil { get; set; }
		public AnalyzerResult[]		AnalysisResults { get; set; }

		public bool					New;

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
							Resources = Resources,
							Updated = Updated,
							AnalysisPayment = AnalysisPayment,
							AnalysisConsil = AnalysisConsil,
							AnalysisResults = AnalysisResults
							};
		}

		public void Write(BinaryWriter writer)
		{
			writer.Write((byte)Flags);
			writer.Write(Updated);
			
			if(Flags.HasFlag(ResourceFlags.Data))
				writer.Write(Data);

			if(Flags.HasFlag(ResourceFlags.Analysis))
			{
				writer.Write(AnalysisPayment);
				writer.Write(AnalysisConsil);
				writer.Write(AnalysisResults, i => { writer.Write(i.AnalyzerId);
													 writer.Write((byte)i.Result); });
			}
		
			writer.Write(Resources, i => writer.Write7BitEncodedInt(i));
		}

		public void Read(BinaryReader reader)
		{
			Flags = (ResourceFlags)reader.ReadByte();
			Updated	= reader.Read<Time>();

			if(Flags.HasFlag(ResourceFlags.Data))
				Data = reader.Read<ResourceData>();
			
			if(Flags.HasFlag(ResourceFlags.Analysis))
			{
				AnalysisPayment	= reader.Read<Money>();
				AnalysisConsil	= reader.ReadByte();
				AnalysisResults	= reader.ReadArray(() => new AnalyzerResult{AnalyzerId = reader.ReadByte(), 
																			Result = (AnalysisResult)reader.ReadByte()});
			}

			Resources = reader.ReadArray(() => reader.Read7BitEncodedInt());
		}
	}
}
