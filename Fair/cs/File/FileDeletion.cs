namespace Uccs.Fair;

public class FileDeletion : FairOperation
{
	public AutoId File { get; set; }

	public override bool IsValid(McvNet net) => true;
	public override string Explanation => $"File={File}";

	public FileDeletion()
	{
	}

	public override void Read(BinaryReader reader)
	{
		File = reader.Read<AutoId>();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(File);
	}

	public override void Execute(FairExecution execution)
	{
		if(!FileExists(execution, File, out var f, out Error))
			return;

		if(f.Refs > 0)
		{
			Error = NotEmptyReferencies;
			return;
		}

		switch((FairTable)f.Owner.Table)
		{
			//case FairTable.Account:
			//{
			//	if(!CanAccessAccount(execution, f.Owner.Id, out var o))
			//		return;
			//
			//	execution.AffectAccount(o.Id);
			//	o.Files = o.Files.Remove(f.Id);
			//	break;
			//}

			case FairTable.Author:
			{
				if(!CanAccessAuthor(execution, f.Owner.Id, out var o, out Error))
					return;

				execution.Authors.Affect(o.Id);
				o.Files = o.Files.Remove(f.Id);
				break;
			}
			case FairTable.Site:
			{
				if(!IsModerator(execution, f.Owner.Id, out var o, out Error))
					return;

				execution.Sites.Affect(o.Id);
				o.Files = o.Files.Remove(f.Id);
				break;
			}
			default:
			{
				Error = InvalidOwnerAddress;
				return;
			}
		}

		f = execution.Files.Affect(f.Id);
		f.Deleted = true;

		//Free(execution, Signer, a, execution.Net.EntityLength + p.Length);
	}
}