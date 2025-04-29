namespace Uccs.Fair;

public class CategoryMovement : FairOperation
{
	public AutoId		Category { get; set; }
	public AutoId		Parent { get; set; }

	public override bool		IsValid(McvNet net) => Category != null; // !Changes.HasFlag(CardChanges.Description) || (Data.Length <= Card.DescriptionLengthMax);
	public override string		Explanation => $"{GetType().Name}, {Parent}";

	public override void Read(BinaryReader reader)
	{
		Category	= reader.Read<AutoId>();
		Parent		= reader.ReadNullable<AutoId>();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Category);
		writer.WriteNullable(Parent);
	}

	public override void Execute(FairExecution execution, bool dispute)
	{
		if(!RequireCategoryAccess(execution, Category, out var c))
			return;

		c = execution.AffectCategory(Category);

		if(c.Parent != null)
		{
			var p = execution.AffectCategory(c.Parent);

			p.Categories = p.Categories.Where(i => i != c.Id).ToArray();
		}

		if(Parent == null)
		{
			var s = execution.AffectSite(c.Site);
			s.Categories = [..s.Categories, c.Id];
		} 
		else
		{
			if(!RequireCategory(execution, Parent, out var p))
				return;

			if(p.Site != c.Site)
			{
				Error = NotFound;
				return;
			}

			c.Parent = p.Id;

			p = execution.AffectCategory(p.Id);
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