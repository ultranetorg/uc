using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Uccs.Rdn;

public enum Meaning : ushort
{
	Raw										= 0,
	
	Control									= 01_000,
		Redirect_Uri						= 01_001,
	
	ExternalContent							= 02_000,

	Package									= 03_000,
		Package_Software_ProductManifest	= 03_001,
		Package_Software_VersionManifest	= 03_002,
	
	Ampp									= 04_000,
		Ampp_Council						= 04_001,
		Ampp_Analysis						= 04_002,
	
	Dns										= 05_000,
		Dns_Record							= 05_001,
}

public enum ContentType : uint
{
	Undefined					= 0,
	File						= 01_00_000_000,
		File_Text				= 01_01_000_000,
		File_Image				= 01_02_000_000,
		File_Audio				= 01_03_000_000,
		File_Video				= 01_04_000_000,
		File_Font				= 01_05_000_000,
		File_End				= 01_99_999_999,

	Directory					= 02_00_000_000,
}

public class ResourceData : IBinarySerializable, IEquatable<ResourceData>
{
	public const short	LengthMax = 8192;

	public Meaning		Meaning { get; set; }
	public ContentType	Content { get; set; }
	public byte[]		Value;
	object				Object;
	
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

	public static bool	IsFile(ContentType c) => ContentType.File <= c && c < ContentType.File_End;
	public static bool	IsDirectory(ContentType c) => c == ContentType.Directory;

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
	
	public bool IsCexInvolved(out Urn urn)
	{ 
		if(	Meaning != Meaning.ExternalContent &&
			Meaning != Meaning.Package_Software_VersionManifest)
		{	
			urn = null;
			return false;
		}

		urn = new Reader(Value, Rdn.Any.Constructor).ReadVirtual<Urn>();
		
		if(urn.Nid != UrnNid.Blake3)
			return false;

		return true;
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

	byte[] Serialize(object o)
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

	public T Get<T>() where T : IBinarySerializable, new()
	{
		if(Object == null)
		{
			using var r = new Reader(Value, Rdn.Any.Constructor);
	
			Object = r.Read<T>();
		}

		return (T)Object;
	}

	public T GetVirtual<T>(Constructor constructor = null) where T : class, IBinarySerializable, ITypeCode
	{
		if(Object == null)
		{
			using var r = new Reader(Value, constructor ?? Rdn.Any.Constructor);

			var o = r.Constructor.Construct(typeof(T), r.ReadUInt32()) as T;
			o.Read(r);
			Object = o;
		}

		return (T)Object;
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
