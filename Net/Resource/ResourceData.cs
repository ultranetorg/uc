using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Uccs.Net
{
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

		public ResourceData(BinaryReader reader)
		{
			Read(reader);
		}

		public ResourceData(DataType type, object interpretation)
		{
			Type = type;
			_Interpretation = interpretation;
		}

		//public static ResourceData FromValue(DataType type, byte[] value)
		//{
		//	Type = type;
		//	_Value = value;
		//}

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
				case DataType.Raw:
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
					_Interpretation = reader.Read<Urr>(Urr.FromType);
					break;

				case DataType.Consil:
					_Interpretation = reader.Read<Consil>();
					break;

				case DataType.Analysis:
					_Interpretation = reader.Read<Analysis>();
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
				case DataType.Raw:
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
				case DataType.Consil:
				case DataType.Analysis:
					writer.Write(Interpretation as IBinarySerializable);
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
		
		public ResourceData Clone()
		{
			object i = null;

			if(_Interpretation == null)
			{
				return new ResourceData {Type = Type, _Value = _Value};
			} 
			else
			{
				switch(_Interpretation)
				{
					case byte[] v:
						i = v.Clone();
						break;

					case string v:
						i = v.Clone();
						break;

					case IPAddress v:
						i = new IPAddress(v.GetAddressBytes());
						break;

					case Urr v: 
						i = Urr.FromRaw(v.Raw);
						break;

					case Consil v:
						i = v.Clone();
						break;

					case Analysis v:
						i = v.Clone();
						break;

					default:
						throw new ResourceException(ResourceError.UnknownDataType);
				}
				
				return new ResourceData {Type = Type, _Interpretation = i, _Value = _Value};
			}
		}
	}

	public class ResourceDataJsonConverter : JsonConverter<ResourceData>
	{
		public override ResourceData Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			return new ResourceData(new BinaryReader(new MemoryStream(reader.GetString().FromHex())));
		}

		public override void Write(Utf8JsonWriter writer, ResourceData value, JsonSerializerOptions options)
		{
			writer.WriteStringValue(value.Hex);
		}
	}

}
