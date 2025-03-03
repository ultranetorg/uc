using System.Text;

namespace Uccs.Fair;

public class ProductUpdation : FairOperation
{
	public EntityId				ProductId { get; set; }
	public string				Name	{ get; set; }
	public byte[]				Value	{ get; set; }
	public override string		Description => $"{ProductId}, {Name}, {Value}";

	public override bool		IsValid(Mcv mcv) => Value.Length <= ProductField.ValueLengthMaximum;

	public ProductUpdation()
	{
	}

	public ProductUpdation(EntityId id)
	{
		ProductId = id;
	}

	//public void Change(string change, string data)
	//{
	//	Changes ??= [];
	//
	//	var f = Changes.FirstOrDefault(i => i.Name == change);
	//
	//	if(f == null)
	//	{
	//		Changes = [..Changes, new (change, data)];
	//	}
	//	else
	//		f.Value  = data;
	//}

	public override void ReadConfirmed(BinaryReader reader)
	{
		ProductId	= reader.Read<EntityId>();
		Name		= reader.ReadUtf8();
		Value		= reader.ReadBytes();
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.Write(ProductId);
		writer.WriteUtf8(Name);
		writer.WriteBytes(Value);
	}

	public override void Execute(FairMcv mcv, FairRound round)
	{
		if(RequireProductAccess(round, ProductId, out var a, out var r) == false)
			return;

		a = round.AffectAuthor(a.Id);
		r = round.AffectProduct(ProductId);

		var f = r.Fields.FirstOrDefault(j => j.Name == Name);

		if(f == null)
		{	
			f = new ProductField {Name = Name, Versions = [new ProductFieldVersion {Value = Value, Version = 0}]};
			r.Fields = [..r.Fields, f];
		}
		else
		{
			f = new ProductField {Name = f.Name, Versions = f.Versions};
			r.Fields = [..r.Fields.Where(i => i.Name != f.Name), f];

			if(f.Versions.Last().Refs == 0)
			{
				Free(round, Signer, a, f.Versions.Last().Value.Length);

				f.Versions = [..f.Versions[..^1], new ProductFieldVersion {Value = Value, Version = f.Versions.Last().Version}];
			}
			else
				f.Versions = [..f.Versions, new ProductFieldVersion {Value = Value, Version = f.Versions.Length}];
		}

		Allocate(round, Signer, a, Value.Length);

		foreach(var j in r.Publications)
		{
			var p = round.AffectPublication(j);
				
			var c = p.Changes.FirstOrDefault(c => c.Name == Name);

			if(c == null)
			{
				p.Changes = [..p.Changes, new ProductFieldVersionReference {Name = Name, Version = f.Versions.Last().Version}];
			} 
			else
			{
				p.Changes = [..p.Changes.Where(i => i != c), new ProductFieldVersionReference {Name = Name, Version = f.Versions.Last().Version}];
			}
		}

		r.Updated = round.ConsensusTime;
	}
}