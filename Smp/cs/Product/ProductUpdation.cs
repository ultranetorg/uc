using System.Text;

namespace Uccs.Smp;

public class ProductUpdation : SmpOperation
{
	public EntityId				ProductId { get; set; }
	public ProductField[]		Changes	{ get; set; }
	public override string		Description => $"{ProductId}, {string.Join(',', Changes.Select(i => i.Name))}";

	public override bool		IsValid(Mcv mcv) => Changes.All(i => i.Size <= Product.DescriptionLengthMaximum);

	public ProductUpdation()
	{
	}

	public ProductUpdation(EntityId id)
	{
		ProductId = id;
	}

	public void Change(string change, string data)
	{
		Changes ??= [];

		var f = Changes.FirstOrDefault(i => i.Name == change);

		if(f == null)
		{
			Changes = [..Changes, new (change, data)];
		}
		else
			f.Value  = data;
	}

	public override void ReadConfirmed(BinaryReader reader)
	{
		ProductId	= reader.Read<EntityId>();
		Changes		= reader.ReadArray<ProductField>();
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.Write(ProductId);
		writer.Write(Changes);
	}

	public override void Execute(SmpMcv mcv, SmpRound round)
	{
		if(RequireProductAccess(round, ProductId, out var a, out var p) == false)
			return;

		a = round.AffectAuthor(a.Id);
		p = round.AffectProduct(ProductId);

		p.Fields ??= [];

		foreach(var i in Changes)
		{	
			var f = p.Fields.FirstOrDefault(j => j.Name == i.Name);

			if(f == null)
			{	
				f = i;
				p.Fields = [..p.Fields, i];
			}
			else
			{
				Free(a, f.Size);
				f.Value = i.Value;
			}

			Allocate(round, a, f.Size);
		}

		p.Updated = round.ConsensusTime;
	}
}