namespace Uccs.Fair;

//public class SiteDeletion : FairOperation
//{
//	public AutoId				Site { get; set; }
//
//	public override bool		IsValid(McvNet net) => true;
//	public override string		Explanation => $"{Id}";
//
//	public SiteDeletion()
//	{
//	}
//
//	public override void Read(BinaryReader reader)
//	{
//		Site = reader.Read<AutoId>();
//	}
//
//	public override void Write(BinaryWriter writer)
//	{
//		writer.Write(Site);
//	}
//
//	public override void Execute(FairExecution execution)
//	{
//		if(RequireSiteModeratorAccess(execution, Site, out var s) == false)
//			return;
//
//		s = execution.Sites.Affect(Site);
//		s.Deleted = true;
//		
//		foreach(var i in s.Categories)
//		{
//			var c = execution.Categories.Affect(i);
//			c.Deleted = true;
//
//			foreach(var j in c.Publications)
//			{
//				var p = execution.Publications.Affect(j);
//				p.Deleted = true;
//			}
//		}
//
//		foreach(var i in s.Moderators)
//		{
//			var a = execution.AffectAccount(i);
//			a.Sites = a.Sites.Remove(Site);
//		}
//		
//		///Free(execution, Signer, s,  Mcv.n)
//	}
//}