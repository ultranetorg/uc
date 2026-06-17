using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Uccs.Rdn;

public class DataType : IEquatable<DataType>, IBinarySerializable
{
	public ushort		Meaning { get; set; }
	public ContentType	Content { get; set; }

	public const ushort	Self							= 000;
	public const ushort	File							= 001;
	public const ushort	Directory						= 002;
	public const ushort	Redirect						= 010;
	public const ushort		Redirect_Uri				= 010_00;

	public DataType()
	{
	}

	public DataType(ushort meaning, ContentType content)
	{
		Meaning = meaning;
		Content = content;
	}

	public static string From(byte[] x)
	{
		return x.ToHex();
	}

	public static DataType Parse(string t)
	{
		var i = t.IndexOf('/');

		if(i == -1)
			return new DataType((ushort)typeof(DataType).GetField(t).GetValue(null), ContentType.Unknown);
		else
			return new DataType((ushort)typeof(DataType).GetField(t.Substring(0, i)).GetValue(null), Enum.Parse<ContentType>(t.AsSpan(i + 1, t.Length - i - 1), true));
	}

	public override string ToString()
	{
		return $"{Meaning}, {Content}";
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as DataType);
	}

	public bool Equals(DataType o)
	{
		return o is not null && Meaning == o.Meaning && Content == o.Content;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Meaning, Content);
	}

	public static bool operator == (DataType left, DataType right)
	{
		return  left is null && right is null || left is not null && left.Equals(right);
	}

	public static bool operator != (DataType left, DataType right)
	{
		return !(left == right);
	}

	public void Write(Writer writer)
	{
		writer.Write(Meaning);
		writer.Write(Content);
	}

	public void Read(Reader reader)
	{
		Meaning	= reader.ReadUInt16();
		Content = reader.Read<ContentType>();
	}
}

public enum ContentType : uint
{
	Unknown 									= 0,
	Raw 										= 0,
	Text										= 0001,
	Image										= 0002,
	Audio										= 0003,
	Video										= 0004,
	Font										= 0005,
	Package										= 1000,
		Package_Software						= 1000_000,
			Package_Software_ProductManifest	= 1000_000_000,
			Package_Software_VersionManifest	= 1000_000_001,
	Ampp										= 1000_001,
		Ampp_Council							= 1000_001_000,
		Ampp_Analysis							= 1000_001_001,
	Dns											= 1000_002,
		DnsRecord								= 1000_002_000,
}

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
 			var w = new Writer(s);
 			
			Write(w);
 		
 			return s.ToArray().ToHex();
 		}
 	}

	public ResourceData()
	{
	}

	public ResourceData(Reader reader)
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
			case Consil a:		return (a as IBinarySerializable).ToRaw();
			case Analysis a:	return (a as IBinarySerializable).ToRaw();
			case DnsRecord a:	return (a as IBinarySerializable).ToRaw();

			default :
				throw new ResourceException(ResourceError.UnknownDataType);
		}
	}

	public override string ToString()
	{
		return $"{Type}, {Value.Length}";
	}

	public T Read<T>() where T : IBinarySerializable, new()
	{
		return new Reader(Value).Read<T>();
	}

	public T Parse<T>()
	{
		return (T) typeof(T).GetMethod("Parse", [typeof(string)]).Invoke(null, [Encoding.UTF8.GetString(Value)]);
	}

	public void Write(Writer writer)
	{
		writer.Write(Type);
		writer.WriteBytes(Value);
	}

	public void Read(Reader reader)
	{
		Type	= reader.Read<DataType>();
		Value	= reader.ReadBytes();
	}

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
}

public class ResourceDataJsonConverter : JsonConverter<ResourceData>
{
	public override ResourceData Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		return new ResourceData(new Reader(reader.GetString().FromHex()));
	}

	public override void Write(Utf8JsonWriter writer, ResourceData value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.Hex);
	}
}
