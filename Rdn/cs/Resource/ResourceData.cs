using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Uccs.Rdn;

public class DataType : IEquatable<DataType>, IBinarySerializable
{
	public ushort		Control { get; set; }
	public ContentType	Content { get; set; }

	public const ushort	Data							= 000;
	public const ushort	File							= 001;
	public const ushort	Directory						= 002;
	public const ushort	Redirect						= 010;
	public const ushort		Redirect_Uri				= 010_00;

	public DataType()
	{
	}

	public DataType(ushort control, ContentType content)
	{
		Control = control;
		Content = content;
	}

	public static string From(byte[] x)
	{
		return x.ToHex();
	}

	public static ushort Parse(string t)
	{
		return (ushort)typeof(DataType).GetField(t).GetValue(null);
	}

	public override string ToString()
	{
		return $"{Control}, {Content}";
	}

	///public bool IsValueDerived(short[] value)
	///{
	///	return value.Length <= Control.Length && Control.Take(value.Length).SequenceEqual(value);
	///}
	///
	///public bool IsContentDerived(short[] content)
	///{
	///	BitOperations.
	///
	///	return content.Length <= Content.Length && Content.Take(content.Length).SequenceEqual(content);
	///}

	public override bool Equals(object obj)
	{
		return Equals(obj as DataType);
	}

	public bool Equals(DataType o)
	{
		return o is not null && Control == o.Control && Content == o.Content;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Control, Content);
	}

	public static bool operator == (DataType left, DataType right)
	{
		return  left is null && right is null || left is not null && left.Equals(right);
	}

	public static bool operator != (DataType left, DataType right)
	{
		return !(left == right);
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(Control);
		writer.Write7BitEncodedInt((int)Content);
	}

	public void Read(BinaryReader reader)
	{
		Control	= reader.ReadUInt16();
		Content = (ContentType)reader.Read7BitEncodedInt();
	}
}

public enum ContentType
{
	Unknown 					= 0,
	Raw 						= 0 ,
	Text						= 010,
	Image						= 020,
	Audio						= 030,
	Video						= 040,
	Font						= 050,
	Applied						= 100,
		Rdn						= 100_000,
			Rdn_ProductManifest	= 100_000_000,
			Rdn_PackageManifest	= 100_000_001,
			Rdn_Consil			= 100_000_002,
			Rdn_Analysis		= 100_000_003,
}
// 
// 	public interface IResourceValue
// 	{
// 		byte[]		Transform();
//  	}

public class ResourceData : IBinarySerializable, IEquatable<ResourceData>
{
	public const short	LengthMax = 8192;

	public DataType		Type;
	public byte[]		Value;
	
 	public string Hex
 	{
 		get
 		{
 			var s = new MemoryStream();
 			var w = new BinaryWriter(s);
 			
			Write(w);
 		
 			return s.ToArray().ToHex();
 		}
 	}
// 
// 		public object Interpretation
// 		{
// 			get
// 			{
// 				if(_Interpretation == null && _Value != null)
// 				{
// 					ReadInterpetation(new BinaryReader(new MemoryStream(_Value)));
// 				}
// 
// 				return _Interpretation;
// 			}
// 		}

	public ResourceData()
	{
	}

	public ResourceData(BinaryReader reader)
	{
		Read(reader);
	}

	public ResourceData(DataType type, object value)
	{
		Type = type;
		Value = Serialize(value);
	}

	public static byte[] Serialize(object o)
	{
		switch(o)
		{
			case byte[] s:		return s;
			case string s:		return Encoding.UTF8.GetBytes(s);
			case Urr s:			return Encoding.UTF8.GetBytes(s.ToString());
			case Ura s:			return Encoding.UTF8.GetBytes(s.ToString());
			case AprvAddress s:	return Encoding.UTF8.GetBytes(s.ToString());
			case Consil a:		return (a as IBinarySerializable).Raw;
			case Analysis a:	return (a as IBinarySerializable).Raw;

			default :
				throw new ResourceException(ResourceError.UnknownDataType);
		}
	}

	//public static ResourceData FromValue(DataType type, byte[] value)
	//{
	//	Type = type;
	//	_Value = value;
	//}

	public override string ToString()
	{
		return $"{Type}, {Value.Length}";
	}

	public T Read<T>() where T : IBinarySerializable, new()
	{
		return new BinaryReader(new MemoryStream(Value)).Read<T>();
	}

	public T Parse<T>()
	{
		return (T) typeof(T).GetMethod("Parse", [typeof(string)]).Invoke(null, [Encoding.UTF8.GetString(Value)]);
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(Type);
		writer.WriteBytes(Value);
	}

	public void Read(BinaryReader reader)
	{
		Type	= reader.Read<DataType>();
		Value	= reader.ReadBytes();
	}
// 
// 		public void ReadInterpetation(BinaryReader reader)
// 		{
// 			switch(BaseType)
// 			{
// 				case DataBaseType.Bytes:	_Interpretation = reader.ReadBytes();	break;
// 				case DataBaseType.UTF8:		_Interpretation = reader.ReadUtf8();	break;
// 				case DataBaseType.Ura:		_Interpretation = reader.Read<Ura>();	break;
// 				case DataBaseType.Urr:		_Interpretation = reader.ReadVirtual<Urr>(); break;
// 
// 				//case DataType.Consil:	_Interpretation = reader.Read<Consil>(); break;
// 				//case DataType.Analysis:	_Interpretation = reader.Read<Analysis>(); break;
// 
// 
// 				default:
// 					throw new ResourceException(ResourceError.UnknownDataType);
// 			}
// 		}
// 
// 		void WriteInterpetation(BinaryWriter writer)
// 		{
// 			switch(BaseType)
// 			{
// 				case DataBaseType.Bytes:
// 					writer.WriteBytes(Interpretation as byte[]);
// 					break;
// 	
// 				case DataBaseType.UTF8:
// 					writer.WriteUtf8(Interpretation as string);
// 					break;
// 				
// 				case DataBaseType.Ura:
// 				case DataBaseType.Urr:
// 					writer.Write(Interpretation as IBinarySerializable);
// 					break;
// 
// 				default:
// 					throw new ResourceException(ResourceError.UnknownDataType);
// 			}
// 		}

	public override int GetHashCode()
	{
		return HashCode.Combine(Type, Value);
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
		return new ResourceData {Type = Type, Value = Value};
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
