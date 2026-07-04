using System.Text;

namespace Uccs.Fair;

public enum Token : uint
{
	None, 
	Architecture,
	Art,
	CPU,
	Date,
	Description,
	DescriptionMaximal,
	DescriptionMinimal,
	Distribution,
	Distributive,
	Source,
	EULA,
	GPU,
	Hardware,
	Hash,
	HashType,
	HashValue,
	HDD,
	Id,
	ISBN,
	Language,
	LicenseType,
	LicensingDetailsUrl,
	Logo,
	Metadata,
	Minimal,
	Name,
	NPU,
	OS,
	Platform,
	Price,
	RAM,
	Recommended,
	Realization,
	Release,
	Requirements,
	Screenshot,
	Slogan,
	Software,
	Tags,
	Title,
	Type,
	UILanguages,
	URI,
	Value,
	Version,
	Video,
	VideoType
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
	LanguageCode, /// two letter code
	License,
	DistributionType, /// installable, zip, webapp  ...
	Money,
	Date,
	Platform,
	OS,
	CPUArchitecture,
	Hash, /// SHA256, MD5, ...


	FileId,
	URL,
	/// Must go before any file-like types
	//Image,
	//ImagePng,
	//ImageJpg,
}

[Flags]
public enum FieldFlag
{
	None, 
	Optional = 0b000_0001, 
	Multi = 0b000_0010,
	ThisOrAnother = 0b000_0100 /// Single selection
}

public class Field
{
	public Token			Name { get; protected set; }
	public FieldType		Type { get; protected set; }
	public FieldFlag		Flags { get; protected set; }
	public Field[]			Fields { get; protected set; }
	public long?			Length { get; protected set; }

	public Field(Token name, Field[] fields = null) : this(name, FieldType.None, FieldFlag.None, null, fields)
	{
	}

	public Field(Token name, FieldType type = FieldType.None, FieldFlag flags = FieldFlag.None, long? length = null, Field[] fields = null)
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
			
			var reader = new Reader(Value);
			a.Read(reader);
			
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

	public FieldValue(Token name, string value) : this(name)
	{
		Name	= name;
		Value	= Encoding.UTF8.GetBytes(value);;
		Fields	= [];
	}

	public FieldValue(Token name, byte[] value, byte _ = default) : this(name)
	{
		Name	= name;
		Value	= value;;
		Fields	= [];
	}

	public FieldValue(Token name, FieldValue[] fields) : this(name)
	{
		Name = name;
		Fields = fields;
	}

	public FieldValue(Token name, byte[] value, FieldValue[] fields) : this(name)
	{
		Name = name;
		Value = value;
		Fields = fields;
	}

	public FieldValue(Token name, string value, FieldValue[] fields) : this(name)
	{
		Name = name;
		Value	= Encoding.UTF8.GetBytes(value);
		Fields = fields;
	}

	public bool IsValid(McvNet net)
	{
		if(Value != null && Value.Length > FieldValue.ValueLengthMaximum)
			return false;

		return Fields.All(i => i.IsValid(net));
	}

	public void Read(Reader reader)
	{
		Name	= reader.Read<Token>();
		Value	= reader.ReadBytes();
		Fields	= reader.ReadArray<FieldValue>();
	}

	public void Write(Writer writer)
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

	public void Read(Reader reader)
	{
		Fields	= reader.ReadArray<FieldValue>();
		Id		= reader.Read7BitEncodedInt();
		Refs	= reader.Read7BitEncodedInt();
	}

	public void Write(Writer writer)
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

	public EntityId				Key => Id;
	public bool					Deleted { get; set; }
	FairMcv						Mcv;

	public Product()
	{
	}

	public Product(FairMcv mcv)
	{
		Mcv = mcv;
	}

	public override string ToString()
	{
		return $"{Id}, Author={Author}, Versions={Versions.Length}";
	}

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

	public void ReadMain(Reader reader)
	{
		Read(reader);
	}

	public void WriteMain(Writer writer)
	{
		Write(writer);
	}

	public void Cleanup(Round lastInCommit)
	{
	}

	public void Write(Writer writer)
	{
		writer.Write(Id);
		writer.Write(Author);
		writer.Write(Type);
		writer.Write(Updated);
		writer.Write(Versions);
		writer.Write(Publications);
	}

	public void Read(Reader reader)
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

				a.Add(new FieldValue(t, FieldValue.Parse(d.Type, i.Get<string>()), parse(d.Fields, i.Nodes)));
			}

			return a.ToArray();
		}

		return parse(definition, x.Nodes);
	}


	public static Field[] FindDeclaration(ProductType type) =>	type switch
																{
																	ProductType.Book => Book,
																	ProductType.Game => Game,
																	ProductType.Movie => Movie,
																	ProductType.Software => Software,
																	_ => throw new IntegrityException()
																};

	public static readonly Field[] Book =	[
												new (Token.Title, FieldType.StringUtf8, length: 128),
												new (Token.DescriptionMinimal,	[
																					new (Token.Language,FieldType.LanguageCode, length: 8),
																					new (Token.Value, FieldType.TextUtf8, length: 1024),
																				]),
												new (Token.DescriptionMaximal,	[
																					new (Token.Language,FieldType.LanguageCode, length: 8),
																					new (Token.Value,	FieldType.TextUtf8, length: int.MaxValue),
																				]),
												new (Token.ISBN, FieldType.StringAnsi, length: 13),
											];

	public static readonly Field[] Game =	[
												new (Token.Title, FieldType.StringUtf8, length: 128),
												new (Token.DescriptionMinimal,  [
																					new (Token.Language,FieldType.LanguageCode, length: 8),
																					new (Token.Value, FieldType.TextUtf8, length: 1024),
																				]),
												new (Token.DescriptionMaximal,  [
																					new (Token.Language,FieldType.LanguageCode, length: 8),
																					new (Token.Value,   FieldType.TextUtf8, length: int.MaxValue),
																				]),
											];

	public static readonly Field[] Movie =	[
												new (Token.Title, FieldType.StringUtf8, length: 128),
												new (Token.DescriptionMinimal,  [
																					new (Token.Language,FieldType.LanguageCode, length: 8),
																					new (Token.Value, FieldType.TextUtf8, length: 1024),
																				]),
												new (Token.DescriptionMaximal,  [
																					new (Token.Language,FieldType.LanguageCode, length: 8),
																					new (Token.Value,   FieldType.TextUtf8, length: int.MaxValue),
																				]),
											];

	public static readonly Field[] Software =	[
													new (Token.Metadata,
													[
														new (Token.Version, FieldType.StringUtf8, length: 16)
													]),
													new (Token.Title,		FieldType.StringUtf8, length: 128),
													//new (Token.Slogan,	FieldType.StringUtf8, FieldFlag.Optional, length: 256),
													new (Token.URI,			FieldType.URI, length: 1024),
													new (Token.Tags,		FieldType.StringUtf8, FieldFlag.Optional, length: 128),
													new (Token.UILanguages, flags : FieldFlag.Optional, fields: 
													[
														new (Token.Language, FieldType.LanguageCode),
													]),
													new (Token.DescriptionMinimal,
													[
														new (Token.Language,FieldType.LanguageCode, length: 8),
														new (Token.Value,   FieldType.TextUtf8, length: 1024),
													]),
													new (Token.DescriptionMaximal,
													[
														new (Token.Language,	FieldType.LanguageCode, length: 8),
														new (Token.Value,		FieldType.TextUtf8, length: int.MaxValue),
													]),
													new (Token.Logo,				FieldType.FileId),
													new (Token.LicenseType,			FieldType.StringUtf8, length: 256),
													new (Token.LicensingDetailsUrl, FieldType.URL, FieldFlag.Optional, length: 256), /// a link to publisher website where various licensing details are described
													new (Token.EULA,				FieldType.TextUtf8, length: 65536), /// full text of EULA
													new (Token.Price,				FieldType.Money, FieldFlag.Optional), /// if  a price is fixed
													new (Token.Art, 
													[
														new (Token.Screenshot, flags: FieldFlag.Multi, fields:	
														[
															new (Token.Id, FieldType.FileId),
														]),
														new (Token.Video, flags: FieldFlag.Optional|FieldFlag.Multi, fields:
														[
															new (Token.URI,	FieldType.URI),
														]),
													]),
													new (Token.Release, FieldType.StringUtf8, length: 16, flags: FieldFlag.Multi, fields: /// Any name that describes platform and functionality specifics
													[
														new (Token.Version,	FieldType.StringUtf8, length: 16),
														new (Token.Date,	FieldType.Date),
														
														new (Token.Distributive, flags: FieldFlag.Multi, fields:		/// a form of distribution
														[
															new (Token.Type,	FieldType.DistributionType),			/// archive, exe/msi installer, torrent, rdn package etc.
															new (Token.Source,	flags: FieldFlag.Multi, fields:			/// links
															[
																new (Token.URI,	FieldType.URI)
															]), 
														]),
														
														new (Token.Requirements,
														[
															new (Token.Platform,
															[
																new (Token.Minimal,	
																[
																	new (Token.Hardware, flags: FieldFlag.Optional, fields:
																	[
																		new (Token.CPU,				FieldType.StringAnsi,		FieldFlag.Optional),
																		new (Token.GPU,				FieldType.StringAnsi,		FieldFlag.Optional),
																		new (Token.NPU,				FieldType.StringAnsi,		FieldFlag.Optional),
																		new (Token.RAM,				FieldType.StringAnsi,		FieldFlag.Optional),
																		new (Token.HDD,				FieldType.StringAnsi,		FieldFlag.Optional),
																	]),
																	new (Token.Software, flags: FieldFlag.Optional, fields:
																	[
																		new (Token.OS,				FieldType.StringAnsi),
																		new (Token.Architecture,	FieldType.StringAnsi,		FieldFlag.Optional),
																		new (Token.Version,			FieldType.StringAnsi,		FieldFlag.Optional),
																	])
																]),
																new (Token.Recommended, flags: FieldFlag.Optional, fields:
																[
																	new (Token.Hardware, flags: FieldFlag.Optional, fields:
																	[
																		new (Token.CPU,				FieldType.StringAnsi,		FieldFlag.Optional),
																		new (Token.GPU,				FieldType.StringAnsi,		FieldFlag.Optional),
																		new (Token.NPU,				FieldType.StringAnsi,		FieldFlag.Optional),
																		new (Token.RAM,				FieldType.StringAnsi,		FieldFlag.Optional),
																		new (Token.HDD,				FieldType.StringAnsi,		FieldFlag.Optional),
																	]),
																	new (Token.Software, flags: FieldFlag.Optional, fields:
																	[
																		new (Token.OS,				FieldType.StringAnsi),
																		new (Token.Architecture,	FieldType.StringAnsi,		FieldFlag.Optional),
																		new (Token.Version,			FieldType.StringAnsi,		FieldFlag.Optional),
																	])
																])
															]),
														]),
													])
												];
}
