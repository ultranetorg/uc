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

public class ProductField : IBinarySerializable
{
	public ProductFieldName			Name { get; set; }
	public byte[]					Value { get; set; }
	public ProductField[]			Fields { get; set; }
	
	public const int				ValueLengthMaximum = 1024*1024;
	public int						Size => Value.Length + Fields.Sum(i => i.Size);
	public string					AsUtf8 => Encoding.UTF8.GetString(Value);

	public static readonly Dictionary<ProductFieldName, ProductFieldType> Types =	new ()
																					{
																						{ProductFieldName.Title,		ProductFieldType.StringUtf8},
																						{ProductFieldName.Slogan,		ProductFieldType.StringUtf8},
																						{ProductFieldName.Description,	ProductFieldType.TextUtf8},
																						{ProductFieldName.Logo,			ProductFieldType.ImagePng},
																					};
	public static bool			IsFile(ProductFieldName type) => Types[type] >= ProductFieldType.File;

	public AutoId AsAutoId
	{
		get
		{
			var a = new AutoId();
			a.Read(new BinaryReader(new MemoryStream(Value)));
			return a;
		}
	}

	public ProductField()
	{
	}

	public ProductField(ProductFieldName name, byte[] value)
	{
		Name	= name;
		Value	= value;
		Fields	= [];
	}
	
	public bool IsValid(McvNet net)
	{
		if(Value.Length > ProductField.ValueLengthMaximum)
			return false;

		return Fields.All(i => i.IsValid(net));
	}

	public void Read(BinaryReader reader)
	{
		Name	= reader.Read<ProductFieldName>();
		Value	= reader.ReadBytes();
		Fields	= reader.ReadArray<ProductField>();
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(Name);
		writer.WriteBytes(Value);
		writer.Write(Fields);
	}

	public static byte[] Parse(ProductFieldName name, string value)
	{ 
		switch(Types[name])
		{
			case ProductFieldType.Float : 
			case ProductFieldType.Integer : 
				return BitConverter.GetBytes(long.Parse(value));

			case ProductFieldType.TextUtf8 : 
			case ProductFieldType.StringUtf8 :
				return Encoding.UTF8.GetBytes(value);
			
			case ProductFieldType.ImagePng : 
			case ProductFieldType.ImageJpg : 
				return AutoId.Parse(value).Raw;
		}

		throw new ArgumentException("Unknown ProductFieldType");
	}
}

public class ProductVersion  : IBinarySerializable
{
	public ProductField[]	Fields { get; set; }
	public int				Id { get; set; }
	public int				Refs { get; set; }

	public int				Size => Fields.Sum(i => i.Size);

	public override string ToString()
	{
		return $"Fields={Fields.Length}, Version={Id}, Refs={Refs}";
	}

	public void Read(BinaryReader reader)
	{
		Fields	= reader.ReadArray<ProductField>();
		Id = reader.Read7BitEncodedInt();
		Refs	= reader.Read7BitEncodedInt();
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(Fields);
		writer.Write7BitEncodedInt(Id);
		writer.Write7BitEncodedInt(Refs);
	}

	public ProductField Find(Func<ProductField, bool> action)
	{
		ProductField go(ProductField[] fields)
		{
			foreach(var i in fields)
			{
				if(action(i))
					return i;

				go(i.Fields);
			}

			return null;
		}

		return go(Fields);
	}

	public void ForEach(Action<ProductField> action)
	{
		void go(ProductField[] fields)
		{
			foreach(var i in fields)
			{
				action(i);

				go(i.Fields);
			}
		}

		go(Fields);
	}
}

public class Product : IBinarySerializable, ITableEntry
{
	public AutoId				Id { get; set; }
	public AutoId				Author { get; set; }
	public ProductVersion[]		Versions { get; set; }
	public Time					Updated { get; set; }
	public AutoId[]				Publications { get; set; }

	public int					Length => Versions.Sum(i => i.Size); /// Data.Type.Length + Data.ContentType.Length  - not fully precise

	public override string ToString()
	{
		return $"{Id}, Author={Author}, Versions={Versions.Length}";
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

	//public byte[] Get(int version, ProductFieldName name)
	//{
	//	return Fields.First(i => i.Name == name).Value;
	//}

	public object Clone()
	{
		return	new Product(Mcv)
				{
					Id = Id,
					Author = Author,
					Versions = Versions,
					Updated = Updated,
					Publications = Publications
				};
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
		writer.Write(Versions);
		writer.Write(Publications);
	}

	public void Read(BinaryReader reader)
	{
		Id				= reader.Read<AutoId>();
		Author			= reader.Read<AutoId>();
		Updated			= reader.Read<Time>();
		Versions		= reader.ReadArray<ProductVersion>();
		Publications	= reader.ReadArray<AutoId>();
	}

	public static ProductField[] ParseDefinition(string text)
	{
		var x = new Xon(text);

		ProductField[] parse(List<Xon> nodes)
		{
			var a = new List<ProductField>();

			foreach(var i in nodes)
			{
				var t = Enum.Parse<ProductFieldName>(i.Name);

				a.Add(	new ProductField(t, ProductField.Parse(t, i.Get<string>()))
						{
							Fields = parse(i.Nodes) 
						});
			}

			return a.ToArray();
		}

		return parse(x.Nodes);
	}
}
