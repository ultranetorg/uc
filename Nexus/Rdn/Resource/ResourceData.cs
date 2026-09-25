using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Uccs.Rdn;

public enum Meaning : ushort
{
	Raw											= 0,
	
	Control										= 01_000,
		Redirect_Uri							= 01_001,
	
	Rex											= 02_000,
		Rex_File 								= 02_001,
		Rex_Directory 							= 02_002,

	Package										= 03_000,
		Package_Software						= 03_000,
			Package_Software_ProductManifest	= 03_001,
			Package_Software_VersionManifest	= 03_002,
	
	Ampp										= 04_000,
		Ampp_Council							= 04_001,
		Ampp_Analysis							= 04_002,
	
	Dns											= 05_000,
		DnsRecord								= 05_001,

}

public enum ContentType : uint
{
	Undefined 	= 0,
	//Raw 		= 0,
	Text		= 0001,
	Image		= 0002,
	Audio		= 0003,
	Video		= 0004,
	Font		= 0005,
}

//public class DataType : IEquatable<DataType>, IBinarySerializable
//{
//
//	public DataType()
//	{
//	}
//
//	public DataType(Meaning meaning, ContentType content)
//	{
//	}
//
//	public DataType(Meaning meaning)
//	{
//		Meaning = meaning;
//	}
//
//	public static string From(byte[] x)
//	{
//		return x.ToHex();
//	}
//
//	public static DataType Parse(string t)
//	{
//		var i = t.IndexOf('/');
//
//		if(i == -1)
//			return new DataType(Enum.Parse<Meaning>(t, true), ContentType.Undefined);
//		else
//			return new DataType(Enum.Parse<Meaning>(t.Substring(0, i), true), Enum.Parse<ContentType>(t.AsSpan(i + 1, t.Length - i - 1), true));
//	}
//
//	public override string ToString()
//	{
//		return $"{Meaning}, {Content}";
//	}
//
//	public override bool Equals(object obj)
//	{
//		return Equals(obj as DataType);
//	}
//
//	public bool Equals(DataType o)
//	{
//		return o is not null && Meaning == o.Meaning && Content == o.Content;
//	}
//
//	public override int GetHashCode()
//	{
//		return HashCode.Combine(Meaning, Content);
//	}
//
//	public static bool operator == (DataType left, DataType right)
//	{
//		return  left is null && right is null || left is not null && left.Equals(right);
//	}
//
//	public static bool operator != (DataType left, DataType right)
//	{
//		return !(left == right);
//	}
//
//	public void Write(Writer writer)
//	{
//		writer.Write(Meaning);
//		writer.Write(Content);
//	}
//
//	public void Read(Reader reader)
//	{
//		Meaning	= reader.Read<Meaning>();
//		Content = reader.Read<ContentType>();
//	}
//}

public class ResourceData : IBinarySerializable, IEquatable<ResourceData>
{
	public const short	LengthMax = 8192;

	public Meaning		Meaning { get; set; }
	public ContentType	Content { get; set; }
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

	public ResourceData(Meaning meaning, ContentType content, object value)
	{
		Meaning = meaning;
		Content = content;
		Value = Serialize(value);
	}

	public ResourceData(Meaning meaning, object value)
	{
		Meaning = meaning;
		Content = ContentType.Undefined;
		Value = Serialize(value);
	}

	public override int GetHashCode()
	{
		return (int)Meaning ^ (int)Content ^ (Value?.Length > 0 ? (int)Value[0] : 0);
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as ResourceData);
	}

	public bool Equals(ResourceData o)
	{
		return o is not null && Meaning == o.Meaning && Content == o.Content && Bytes.Equal(Value, o.Value);
	}

	public static bool operator == (ResourceData left, ResourceData right)
	{
		return  left is null && right is null || 
				left is not null && left.Equals(right);
	}

	public static bool operator != (ResourceData left, ResourceData right)
	{
		return !(left == right);
	}

	public static byte[] Serialize(object o)
	{
		switch(o)
		{
			case byte[] s:	return s;
			case string s:	return Encoding.UTF8.GetBytes(s);

			default :
				if(o is IBinarySerializable b)
					return b.ToRaw(Rdn.Any.Constructor);
				else
					throw new ResourceException(ResourceError.UnknownDataType);
		}
	}

	public override string ToString()
	{
		return $"{Meaning}, {Content}, {Value?.Length}";
	}

	public T Read<T>() where T : IBinarySerializable, new()
	{
		using var r = new Reader(Value, Rdn.Any.Constructor);

		return r.Read<T>();
	}

	public T ReadVirtual<T>(Constructor constructor = null) where T : class, IBinarySerializable, ITypeCode
	{
		using var r = new Reader(Value, constructor ?? Rdn.Any.Constructor);

		var o = r.Constructor.Construct(typeof(T), r.ReadUInt32()) as T;
		o.Read(r);
		return o;
	}

	//public T Parse<T>()
	//{
	//	return (T) typeof(T).GetMethod("Parse", [typeof(string)]).Invoke(null, [Encoding.UTF8.GetString(Value)]);
	//}

	public void Write(Writer writer)
	{
		writer.Write(Meaning);
		writer.Write(Content);
		writer.WriteBytes(Value);
	}

	public void Read(Reader reader)
	{
		Meaning	= reader.Read<Meaning>();
		Content = reader.Read<ContentType>();
		Value	= reader.ReadBytes();
	}
}

public class ResourceDataJsonConverter : JsonConverter<ResourceData>
{
	public override ResourceData Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		return new Reader(reader.GetString().FromHex(), Rdn.Any.Constructor).Read<ResourceData>();
	}

	public override void Write(Utf8JsonWriter writer, ResourceData value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.Hex);
	}
}
