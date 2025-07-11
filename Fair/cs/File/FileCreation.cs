using System.Text;

namespace Uccs.Fair;

//public class FileCreation : FairOperation
//{
//	public EntityAddress		Owner { get; set; }
//	public byte[]				Data { get; set; }
//
//	public override string		Explanation => $"Owner={Owner}, Data={Data.Length}";
//
//	public override bool		IsValid(McvNet net) => Data.Length <= (net as Fair).FileLengthMaximum;
//
//	public override void Read(BinaryReader reader)
//	{
//		Owner	= reader.Read<EntityAddress>();
//		Data		= reader.ReadBytes();
//	}
//
//	public override void Write(BinaryWriter writer)
//	{
//		writer.Write(Owner);
//		writer.Write(Data);
//	}
//
//	public override void Execute(FairExecution execution)
//	{
//		switch(Owner.Table)
//		{
//			case FairTable.Account:
//			{
//				if(!RequireAccount(execution, Owner.Id, out var a))
//					return;
//				
//				a = execution.AffectAccount(a.Id);
//				Allocate(execution, a, a, Data.Length);
//				break;
//			}
//
//			case FairTable.Author:
//			{
//				if(!RequireAuthor(execution, Owner.Id, out var a))
//					return;
//
//				a = execution.Authors.Affect(a.Id);
//				Allocate(execution, a, a, Data.Length);
//				break;
//			}
//
//			case FairTable.Site:
//			{
//				if(!SiteExists(execution, Owner.Id, out var a))
//					return;
//
//				a = execution.Sites.Affect(a.Id);
//				Allocate(execution, a, a, Data.Length);
//				break;
//			}
//
//			default:
//			{
//				Error = InvalidOwnerAddress;
//				return;
//			}
//		}
//
//		var c = execution.Files.Create(Owner.Id);
//
//		c.Refs		= 1;
//		c.Data		= Data;
//
//
//		switch(Owner.Table)
//		{
//			case FairTable.Account:
//			{	
//				var o = execution.AffectAccount(Owner.Id);
//				o.Files = [..o.Files, c.Id];
//				break;
//			}
//		
//			case FairTable.Author:
//			{
//				var o = execution.Authors.Affect(Owner.Id);
//				o.Files = [..o.Files, c.Id];
//				break;
//			}
//			
//			case FairTable.Site:
//			{
//				var o = execution.Sites.Affect(Owner.Id);
//				o.Files = [..o.Files, c.Id];
//				break;
//			}
//		}
//	}
//}