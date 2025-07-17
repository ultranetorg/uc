using System.Text;

namespace Uccs.Fair;

public enum ProductFieldName : uint
{
	None, 
	Title,
	Slogan,
	Description,
	Logo
}

public enum ProductFieldType : int
{
	None, 
	Integer,
	Float,
	TextUtf8,
	StringUtf8,

	File, /// Must go before any file-like types
	ImagePng,
	ImageJpg,
}

public class ProductFieldVersionReference  : IBinarySerializable
{
	public ProductFieldName	Field { get; set; }
	public int				Version { get; set; }

	public void Read(BinaryReader reader)
	{
		Field = reader.Read<ProductFieldName>();
		Version = reader.Read7BitEncodedInt();
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(Field);
		writer.Write7BitEncodedInt(Version);
	}

	public override string ToString()
	{
		return $"{Field}={Version}";
	}
}

public class ProductFieldVersion : IBinarySerializable
{
	public int			Version { get; set; }
	public byte[]		Value { get; set; }
	public int			Refs { get; set; }

	public string		AsUtf8 => Encoding.UTF8.GetString(Value);
	
	public AutoId AsAutoId
	{
		get
		{
			var a = new AutoId();
			a.Read(new BinaryReader(new MemoryStream(Value)));
			return a;
		}
	}

	public ProductFieldVersion()
	{
	}

	public ProductFieldVersion(int version, byte[] value, int refs)
	{
		Version = version;
		Value = value;
		Refs = refs;
	}

	public void Read(BinaryReader reader)
	{
		Version = reader.Read7BitEncodedInt();
		Value = reader.ReadBytes();
		Refs = reader.Read7BitEncodedInt();
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write7BitEncodedInt(Version);
		writer.WriteBytes(Value);
		writer.Write7BitEncodedInt(Refs);
	}
}

public class ProductField : IBinarySerializable
{
	public ProductFieldName			Name { get; set; }
	public ProductFieldVersion[]	Versions  { get; set; }

	public int						Size =>  Versions.Sum(i => i.Value.Length);

	public const int				ValueLengthMaximum = 1024*1024;
	public const int				ValueNameMaximum = 256;
	
	public ProductField()
	{
	}

	public void Read(BinaryReader reader)
	{
		Name		= reader.Read<ProductFieldName>();
		Versions	= reader.ReadArray<ProductFieldVersion>();
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(Name);
		writer.Write(Versions);
	}
}

public class Product : IBinarySerializable, ITableEntry
{
	public AutoId				Id { get; set; }
	public AutoId				Author { get; set; }
	public ProductField[]		Fields	{ get; set; }
	public Time					Updated { get; set; }
	public AutoId[]				Publications { get; set; }

	public int					Length => Fields.Sum(i => i.Size); /// Data.Type.Length + Data.ContentType.Length  - not fully precise

	public static Dictionary<ProductFieldName, ProductFieldType> FieldTypes =	new ()
																				{
																					{ProductFieldName.Title,		ProductFieldType.StringUtf8},
																					{ProductFieldName.Slogan,		ProductFieldType.StringUtf8},
																					{ProductFieldName.Description,	ProductFieldType.TextUtf8},
																					{ProductFieldName.Logo,			ProductFieldType.ImagePng},
																				};
	public static bool			IsFile(ProductFieldName type) => FieldTypes[type] >= ProductFieldType.File;

	public override string ToString()
	{
		return $"{Id}, Fields={Fields}";
	}

	public EntityId			Key => Id;
	public bool				Deleted { get; set; }
	FairMcv					Mcv;

	public Product()
	{
	}

	public Product(FairMcv mcv)
	{
		Mcv = mcv;
	}

	public ProductFieldVersion Get(ProductFieldVersionReference reference)
	{
		return Fields.First(i => i.Name == reference.Field).Versions.First(i => i.Version == reference.Version);
	}

	public object Clone()
	{
		return new Product(Mcv){Id = Id,
								Author = Author,
								Fields = Fields,
								Updated = Updated,
								Publications = Publications};
	}

	public void ReadMain(BinaryReader reader)
	{
		Read(reader);
	}

	public void WriteMain(BinaryWriter writer)
	{
		Write(writer);
	}

	public void Cleanup(Round lastInCommit)
	{
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(Id);
		writer.Write(Author);
		writer.Write(Updated);
		writer.Write(Fields);
		writer.Write(Publications);
	}

	public void Read(BinaryReader reader)
	{
		Id				= reader.Read<AutoId>();
		Author			= reader.Read<AutoId>();
		Updated			= reader.Read<Time>();
		Fields			= reader.ReadArray<ProductField>();
		Publications	= reader.ReadArray<AutoId>();
	}
}
