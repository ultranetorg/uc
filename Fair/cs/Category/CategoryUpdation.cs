namespace Uccs.Fair;

public class CategoryMovement : FairOperation
{
	public EntityId		Category { get; set; }
	public EntityId		Parent { get; set; }

	public override bool		IsValid(Mcv mcv) => Category != null; // !Changes.HasFlag(CardChanges.Description) || (Data.Length <= Card.DescriptionLengthMax);
	public override string		Description => $"{GetType().Name}, {Parent}";

	public override void ReadConfirmed(BinaryReader reader)
	{
		Category	= reader.Read<EntityId>();
		Parent		= reader.ReadNullable<EntityId>();
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.Write(Category);
		writer.WriteNullable(Parent);
	}

	public override void Execute(FairMcv mcv, FairRound round)
	{
		if(!RequireCategoryAccess(round, Category, out var c))
			return;

		c = round.AffectCategory(Category);

		if(c.Parent != null)
		{
			var p = round.AffectCategory(c.Parent);

			p.Categories = p.Categories.Where(i => i != c.Id).ToArray();
		}

		if(Parent == null)
		{
			var s = round.AffectSite(c.Site);
			s.Categories = [..s.Categories, c.Id];
		} 
		else
		{
			if(!RequireCategory(round, Parent, out var p))
				return;

			if(p.Site != c.Site)
			{
				Error = NotFound;
				return;
			}

			c.Parent = p.Id;

			p = round.AffectCategory(p.Id);
			p.Categories = [..p.Categories, c.Id];
		}

// 			case CategoryChange.RemoveCategory:
// 			{	
// 				if(!RequireCategoryAccess(round, EntityId, Signer, out var c))
// 					return;
// 
// 				if(c.Parent != Category)
// 				{
// 					Error = NotFound;
// 					return;
// 				}
// 
// 				c = round.AffectCategory(EntityId);
// 
// 				c.Parent = null;
// 				c.Categories = c.Categories.Where(i => i != EntityId).ToArray();
// 				
// 				break;
// 			}
			
			//case CategoryChange.Security:
			//{
			//	p.Fields |= CategoryField.Security;
			//	
			//	p.AffectSecurity();
			//
			//	foreach(var i in Security.Permissions)
			//	{
			//		if(i.Value[0] == CategoryActor.None)
			//			p.Security.Permissions.Remove(i.Key);
			//		else
			//			p.Security.Permissions[i.Key] = i.Value;
			//	}
			//	break;
			//}
		
	}
}