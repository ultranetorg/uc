using System.Text;

namespace Uccs.Fair;

public enum Token : uint
{
	None, 
	Metadata,
	CPU,
	Date,
	Distributive,
	Download,
	DescriptionMinimal,
	DescriptionMaximal,
	GPU,
	Hardware,
	License,
	Logo,
	NPU,
	Platform,
	RAM,
	Release,
	Requirements,
	Slogan,
	Software,
	HDD,
	Title,
	Type,
	Version,
	OS,
	Architecture,
	Hash,
	HashType,
	HashValue,
	URI,
	Screenshot,
	Art,
	Id,
	Video,
	VideoType,
	Language,
	Youtube,
	Tags,
	Price,
	Description,
	Deployment,
	Value
}

public enum FieldType : int
{
	None, 
	Integer,
	Float,

	TextUtf8, /// Multi-line
	StringUtf8, /// Single-line
	StringAnsi, /// Single-line
	URI,
	Language,
	License,
	Video,
	Deployment,
	Money,
	Date,
	Platform,
	OS,
	CPUArchitecture,
	Hash,


	FileId, /// Must go before any file-like types
	//Image,
	//ImagePng,
	//ImageJpg,
}

[Flags]
public enum FieldFlag
{
	None, 
	Optional = 0b000_0001, 
	ThisOrAnother = 0b000_0010 /// Single selection
}

public class Field
{
	public Token			Name { get; protected set; }
	public FieldType		Type { get; protected set; }
	public FieldFlag		Flags { get; protected set; }
	public Field[]			Fields { get; protected set; }
	public long?			Length { get; protected set; }

	public Field(Token name, Field[] fields = null, FieldFlag flags = FieldFlag.None, long? length = null)
		: this(name, FieldType.None, fields, flags, length)
	{
	}

	public Field(Token name, FieldType type, FieldFlag flags = FieldFlag.None, long? length = null)
		: this(name, type, null, flags, length)
	{
	}

	public Field(Token name, FieldType type, Field[] fields, FieldFlag flags, long? length = null)
	{
		Name = name;
		Type = type;
		Fields = fields;
		Flags = flags;
		Length = length;
	}
}

public class FieldValue : IBinarySerializable
{
	public Token				Name { get; set; }
	public byte[]				Value { get; set; }
	public FieldValue[]			Fields { get; set; }
	
	public const int			ValueLengthMaximum = 1024*1024;
	public int					Size => (Value?.Length ?? 0) + Fields.Sum(i => i.Size);
	public string				AsUtf8 => Encoding.UTF8.GetString(Value);


	//public static bool			IsFile(Token type) => Types[type] >= FieldType.File;

	public AutoId AsAutoId
	{
		get
		{
			var a = new AutoId();
			a.Read(new BinaryReader(new MemoryStream(Value)));
			return a;
		}
	}

	public FieldValue()
	{
	}

	public FieldValue(Token name)
	{
		Name = name;
	}

	public FieldValue(Token name, byte[] value) : this(name)
	{
		Name	= name;
		Value	= value;
		Fields	= [];
	}
	
	public bool IsValid(McvNet net)
	{
		if(Value != null && Value.Length > FieldValue.ValueLengthMaximum)
			return false;

		return Fields.All(i => i.IsValid(net));
	}

	public void Read(BinaryReader reader)
	{
		Name	= reader.Read<Token>();
		Value	= reader.ReadBytes();
		Fields	= reader.ReadArray<FieldValue>();
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(Name);
		writer.WriteBytes(Value);
		writer.Write(Fields);
	}

	public static byte[] Parse(FieldType type, string value)
	{ 
		switch(type)
		{
			case FieldType.Float : 
			case FieldType.Integer : 
				return BitConverter.GetBytes(long.Parse(value));

			case FieldType.TextUtf8 : 
			case FieldType.StringUtf8 :
				return Encoding.UTF8.GetBytes(value);
			
			case FieldType.FileId : 
				return AutoId.Parse(value).Raw;
		}

		throw new ArgumentException("Unknown ProductFieldType");
	}
}

public class ProductVersion  : IBinarySerializable
{
	public FieldValue[]		Fields { get; set; }
	public int				Id { get; set; }
	public int				Refs { get; set; }

	public int				Size => Fields.Sum(i => i.Size);

	public override string ToString()
	{
		return $"Fields={Fields.Length}, Version={Id}, Refs={Refs}";
	}

	public void Read(BinaryReader reader)
	{
		Fields	= reader.ReadArray<FieldValue>();
		Id		= reader.Read7BitEncodedInt();
		Refs	= reader.Read7BitEncodedInt();
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(Fields);
		writer.Write7BitEncodedInt(Id);
		writer.Write7BitEncodedInt(Refs);
	}

	public FieldValue Find(Func<FieldValue, bool> action)
	{
		FieldValue go(FieldValue[] fields)
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

	public FieldValue Find(Field[] definition, Func<Field, FieldValue, bool> action)
	{
		FieldValue go(Field[] defs, FieldValue[] fields)
		{
			foreach(var i in fields)
			{
				var d = defs.FirstOrDefault(j => j.Name == i.Name);

				if(d != null)
				{
					if(action(d, i))
						return i;
	
					go(d.Fields, i.Fields);
				}
			}

			return null;
		}

		return go(definition, Fields);
	}

	public void ForEach(Action<FieldValue> action)
	{
		void go(FieldValue[] fields)
		{
			foreach(var i in fields)
			{
				action(i);

				go(i.Fields);
			}
		}

		go(Fields);
	}

	public void ForEach(Field[] definition, Action<Field, FieldValue> action)
	{
		void go(Field[] defs, FieldValue[] fields)
		{
			foreach(var i in fields)
			{
				var d = defs.FirstOrDefault(j => j.Name == i.Name);

				if(d != null)
				{
					action(d, i);
	
					go(d.Fields, i.Fields);
				}
			}
		}

		go(definition, Fields);
	}
}

public class Product : IBinarySerializable, ITableEntry
{
	public AutoId				Id { get; set; }
	public AutoId				Author { get; set; }
	public ProductType			Type { get; set; }
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
					Type = Type,
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
		writer.Write(Type);
		writer.Write(Updated);
		writer.Write(Versions);
		writer.Write(Publications);
	}

	public void Read(BinaryReader reader)
	{
		Id				= reader.Read<AutoId>();
		Author			= reader.Read<AutoId>();
		Type			= reader.Read<ProductType>();
		Updated			= reader.Read<Time>();
		Versions		= reader.ReadArray<ProductVersion>();
		Publications	= reader.ReadArray<AutoId>();
	}

	public static FieldValue[] ParseDefinition(Field[] definition, string text)
	{
		var x = new Xon(text);

		FieldValue[] parse(Field[] definition, List<Xon> nodes)
		{
			var a = new List<FieldValue>();

			foreach(var i in nodes)
			{
				var t = Enum.Parse<Token>(i.Name);
				var d = definition.First(j => j.Name.ToString() == i.Name);

				a.Add(	new FieldValue(t, FieldValue.Parse(d.Type, i.Get<string>()))
						{
							Fields = parse(d.Fields, i.Nodes) 
						});
			}

			return a.ToArray();
		}

		return parse(definition, x.Nodes);
	}


	public static Field[] FindDeclaration(ProductType type) =>	type switch
																{
																	ProductType.Software => Software, 
																	_ => throw new IntegrityException()
																};

	public static readonly Field[] Software =	[
													new (Token.Metadata, [
																			new (Token.Version, FieldType.StringUtf8, length: 16)
																		 ]),
													new (Token.Title,	FieldType.StringUtf8, length: 128),
													new (Token.Slogan,	FieldType.StringUtf8, FieldFlag.Optional, length: 256),
													new (Token.URI,		FieldType.URI, length: 1024),
													new (Token.Tags,	FieldType.StringUtf8, FieldFlag.Optional, length: 128),
													new (Token.DescriptionMinimal,	[
																						new (Token.Language,FieldType.Language, length: 8),
																						new (Token.Value, FieldType.TextUtf8, length: 1024),
																					]),
													new (Token.DescriptionMaximal,	[
																						new (Token.Language,FieldType.Language, length: 8),
																						new (Token.Value,	FieldType.TextUtf8, length: int.MaxValue),
																					]),
													new (Token.Logo,	FieldType.FileId),
													new (Token.License, FieldType.StringUtf8, length: 256),
													new (Token.Price,	FieldType.Money, FieldFlag.Optional),
													new (Token.Art, [
																		new (Token.Screenshot,	[
																									new (Token.Id, FieldType.FileId),
																									new (Token.Description,	[
																																new (Token.Language,	FieldType.Language),
																																new (Token.Description,	FieldType.TextUtf8, FieldFlag.Optional),
																															]),
																								]),
																		new (Token.Video,	[
																								new (Token.Type,	FieldType.Video),
																								new (Token.Id,		FieldType.FileId,	FieldFlag.ThisOrAnother),
																								new (Token.Youtube,	FieldType.URI,		FieldFlag.ThisOrAnother),
																								new (Token.Description,	[
																															new (Token.Language,		FieldType.Language),
																															new (Token.Description,	FieldType.TextUtf8, FieldFlag.Optional),
																														]),
																							]),
																	]),
													new (Token.Release,	[
																			new (Token.Version, FieldType.StringUtf8, length: 16),
																			new (Token.Distributive, [
																										new (Token.Platform,	FieldType.StringUtf8, length: 16),
																										new (Token.Version,		FieldType.StringUtf8, length: 16),
																										new (Token.Date,		FieldType.Date),
																										new (Token.Deployment,	FieldType.Deployment),
																										new (Token.Download,[
																																new (Token.URI,	FieldType.URI),
																																new (Token.Hash,[
																																					new (Token.Type,	FieldType.Hash),
																																					new (Token.Value,	FieldType.StringAnsi)
																																				], FieldFlag.Optional),
																															])
																									 ]),
																			new (Token.Requirements,[
																										new (Token.Hardware,[
																																new (Token.CPU,				FieldType.StringAnsi,		FieldFlag.Optional),
																																new (Token.GPU,				FieldType.StringAnsi,		FieldFlag.Optional),
																																new (Token.NPU,				FieldType.StringAnsi,		FieldFlag.Optional),
																																new (Token.RAM,				FieldType.StringAnsi,		FieldFlag.Optional),
																																new (Token.HDD,				FieldType.StringAnsi,		FieldFlag.Optional),
																															]),
																										new (Token.Software,[
																																new (Token.OS,				FieldType.OS),
																																new (Token.Architecture,	FieldType.CPUArchitecture,	FieldFlag.Optional),
																																new (Token.Version,			FieldType.StringAnsi,		FieldFlag.Optional),
																															])
																									]),
																		])
												];

}
