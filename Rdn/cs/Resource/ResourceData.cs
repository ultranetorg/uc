using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Uccs.Rdn
{

	public class DataType : IEquatable<DataType>, IBinarySerializable
	{
		public string Control;
		public string Content;

		public static readonly string	Self							= null;
		public static readonly string	File							= From([1]);
		public static readonly string	Directory						= From([2]);
		public static readonly string	Redirect						= From([100]);
		public static readonly string		Redirect_Uri				= From([100, 0]);
		public static readonly string		Redirect_IPAddress			= From([100, 1]);
		public static readonly string			Redirect_IP4			= From([100, 1, 0]);
		public static readonly string			Redirect_IP6			= From([100, 1, 1]);
		public static readonly string		Redirect_ProductRealization	= From([100, 100]);

		public DataType()
		{
		}

		public DataType(string control, string content)
		{
			Control = control;
			Content = content;
		}

		public DataType(byte[] control, byte[] content)
		{
			Control = From(control);
			Content = From(content);
		}

		public static string From(byte[] x)
		{
			return x.ToHex();
		}

		public static string Parse(string t)
		{
			return typeof(DataType).GetField(t).GetValue(null) as string;
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
			writer.WriteBytes(Control?.FromHex());
			writer.WriteBytes(Content?.FromHex());
		}

		public void Read(BinaryReader reader)
		{
			Control	= reader.ReadBytes()?.ToHex();
			Content = reader.ReadBytes()?.ToHex();
		}
	}

	public class ContentType
	{
		public static readonly string	Unknown 					= null;
		public static readonly string	Raw 						= null;
		public static readonly string	Text						= DataType.From([10]);
		public static readonly string	Image						= DataType.From([20]);
		public static readonly string	Audio						= DataType.From([30]);
		public static readonly string	Video						= DataType.From([40]);
		public static readonly string	Font						= DataType.From([50]);
		public static readonly string	Applied						= DataType.From([100]);
		public static readonly string		Rdn						= DataType.From([100, 0]);
		public static readonly string			Rdn_ProductManifest	= DataType.From([100, 0, 0]);
		public static readonly string			Rdn_PackageManifest	= DataType.From([100, 0, 1]);
 		public static readonly string			Rdn_Consil			= DataType.From([100, 0, 2]);
 		public static readonly string			Rdn_Analysis		= DataType.From([100, 0, 3]);

		public static string Parse(string t)
		{
			return typeof(ContentType).GetField(t).GetValue(null) as string;
		}
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
				case byte[] s:	 return s;
				case string s:	 return Encoding.UTF8.GetBytes(s);
				case Urr s:		 return Encoding.UTF8.GetBytes(s.ToString());
				case Ura s:		 return Encoding.UTF8.GetBytes(s.ToString());
				case Consil a:	 return (a as IBinarySerializable).Raw;
				case Analysis a: return (a as IBinarySerializable).Raw;

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

}
