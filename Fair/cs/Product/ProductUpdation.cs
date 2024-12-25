using System.Text;

namespace Uccs.Fair;

public class ProductUpdation : FairOperation
{
	public ProductId			ProductId { get; set; }
	public ProductField[]		Changes	{ get; set; }
	public override string		Description => $"{ProductId}, {string.Join(',', Changes.Select(i => i.Type))}";

	public override bool IsValid(Mcv mcv) => Changes.All(i => i.Type switch
																	 {
																	 	ProductProperty.Description => (i.Value as string).Length <= Product.DescriptionLengthMaximum,
																	 	_ => throw new RequirementException()
																	 });

	public ProductUpdation()
	{
	}

	public ProductUpdation(ProductId id)
	{
		ProductId = id;
	}

	public void Change(ProductProperty change, string data)
	{
		Changes ??= [];

		var f = Changes.FirstOrDefault(i => i.Type == change);

		if(f == null)
		{
			Changes = [..Changes, new (change, data)];
		}
		else
			f.Value  = data;
	}

	public override void ReadConfirmed(BinaryReader reader)
	{
		ProductId	= reader.Read<ProductId>();
		Changes		= reader.ReadArray<ProductField>();
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.Write(ProductId);
		writer.Write(Changes);
	}

	public override void Execute(FairMcv mcv, FairRound round)
	{
		if(RequireSignerProduct(round, ProductId, out var a, out var p) == false)
			return;

		a = round.AffectAuthor(a.Id);
		p = round.AffectProduct(ProductId);

		p.Fields ??= [];

		foreach(var i in Changes)
		{	
			var f = p.Fields.FirstOrDefault(j => j.Type == i.Type);

			if(f == null)
			{	
				f = i;
				p.Fields = [..p.Fields, i];
			}
			else
			{
				Free(a, ProductField.GetSize(i.Type, f.Value));
				f.Value = i.Value;
			}

			Allocate(round, a, ProductField.GetSize(f.Type, f.Value));
		}

		p.Updated = round.ConsensusTime;
	}
}