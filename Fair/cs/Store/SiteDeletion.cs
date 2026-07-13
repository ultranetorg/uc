namespace Uccs.Fair;

//public class StoreDeletion : FairOperation
//{
//	public AutoId				Store { get; set; }
//
//	public override bool		IsValid(McvNet net) => true;
//	public override string		Explanation => $"{Id}";
//
//	public StoreDeletion()
//	{
//	}
//
//	public override void Read(Reader reader)
//	{
//		Store = reader.Read<AutoId>();
//	}
//
//	public override void Write(Writer writer)
//	{
//		writer.Write(Store);
//	}
//
//	public override void Execute(FairExecution execution)
//	{
//		if(RequireStoreModeratorAccess(execution, Store, out var s) == false)
//			return;
//
//		s = execution.Stores.Affect(Store);
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
//			a.Stores = a.Stores.Remove(Store);
//		}
//		
//		///Free(execution, Signer, s,  Mcv.n)
//	}
//}