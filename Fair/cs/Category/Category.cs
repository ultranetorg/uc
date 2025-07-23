using System.Collections.Immutable;

namespace Uccs.Fair;

public enum CategoryType : byte
{
	None, Software, Movie, Music, Book, Game
}

public class Category : IBinarySerializable, ITableEntry
{
	public AutoId			Id { get; set; }
	public AutoId			Site { get; set; }
	public AutoId			Parent { get; set; }
	public CategoryType		Type { get; set; }
	public string			Title { get; set; }
	public AutoId[]			Categories { get; set; }
	public AutoId[]			Publications { get; set; }
	public AutoId			Avatar  { get; set; }

	public EntityId			Key => Id;
	public bool				Deleted { get; set; }
	FairMcv					Mcv;

	public Category()
	{
	}

	public Category(FairMcv mcv)
	{
		Mcv = mcv;
	}

	public object Clone()
	{
		return	new Category(Mcv)   
				{
					Id				= Id,
					Site			= Site,
					Parent			= Parent,
					Type			= Type,
					Title			= Title,
					Categories		= Categories,
					Publications	= Publications,
					Avatar			= Avatar,
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

	public void Read(BinaryReader reader)
	{
		Id				= reader.Read<AutoId>();
		Site			= reader.Read<AutoId>();
		Parent			= reader.ReadNullable<AutoId>();
		Type			= reader.Read<CategoryType>();
		Title			= reader.ReadUtf8();
		Categories		= reader.ReadArray<AutoId>();
		Publications	= reader.ReadArray<AutoId>();
		Avatar			= reader.ReadNullable<AutoId>();
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(Id);
		writer.Write(Site);
		writer.WriteNullable(Parent);
		writer.Write(Type);
		writer.WriteUtf8(Title);
		writer.Write(Categories);
		writer.Write(Publications);
		writer.WriteNullable(Avatar);
	}
}
