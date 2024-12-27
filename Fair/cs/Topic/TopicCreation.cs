namespace Uccs.Fair;

public class TopicCreation : FairOperation
{
	public EntityId				Catalogue { get; set; }
	public EntityId				Product { get; set; }

	public override bool		IsValid(Mcv mcv) => true; // !Changes.HasFlag(CardChanges.Description) || (Data.Length <= Card.DescriptionLengthMax);
	public override string		Description => $"{GetType().Name}";

	public TopicCreation()
	{
	}

	public override void ReadConfirmed(BinaryReader reader)
	{
		Catalogue	= reader.Read<EntityId>();
		Product		= reader.Read<EntityId>();
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.Write(Catalogue);
		writer.Write(Product);
	}

	public override void Execute(FairMcv mcv, FairRound round)
	{
		if(RequireCatalogueAccess(round, Catalogue, out var cat) == false)
			return;

		if(RequireProduct(round, Product, out var au, out var p) == false)
			return;

		var c = round.AffectTopic(cat);

		c.Catalogue = Catalogue;
		c.Product = Product;

		cat = round.AffectCatalogue(cat.Id);

		cat.Topics = cat.Topics == null ? [c.Id] : [..cat.Topics, c.Id];
	}
}