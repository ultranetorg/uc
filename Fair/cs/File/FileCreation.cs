using System.Text;

namespace Uccs.Fair;

public class FileCreation : FairOperation
{
	public EntityAddress		Owner { get; set; }
	public byte[]				Data { get; set; }

	public override string		Explanation => $"Owner={Owner}, Data={Data.Length}";
	public override bool		IsValid(McvNet net) => Data.Length <= (net as Fair).FileLengthMaximum;

	public FileCreation()
	{
	}

	public FileCreation(EntityAddress owner, byte[] data)
	{
		Owner = owner;
		Data = data;
	}

	public FileCreation(FairTable table, AutoId id, byte[] data)
	{
		Owner = new EntityAddress(table, id);
		Data = data;
	}

	public override void Read(BinaryReader reader)
	{
		Owner	= reader.Read<EntityAddress>();
		Data	= reader.ReadBytes();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Owner);
		writer.WriteBytes(Data);
	}

	public override void Execute(FairExecution execution)
	{
		var f = execution.Files.Create(Owner.Id);

		f.Owner		= Owner;
		f.Refs		= 0;
		f.Data		= Data;

		switch(Owner.Table)
		{
			//case FairTable.Account:
			//{
			//	if(!CanAccessAccount(execution, Owner.Id, out var a, out Error))
			//		return;
			//	
			//	a = execution.AffectAccount(a.Id);
			//	execution.Allocate(a, a, Data.Length);
			//	break;
			//}

			case FairTable.Author:
			{
				if(!CanAccessAuthor(execution, Owner.Id, out var a, out Error))
					return;

				a = execution.Authors.Affect(a.Id);
				a.Files = [..a.Files, f.Id];
				execution.Allocate(a, a, Data.Length);
				break;
			}

			case FairTable.Site:
			{
				if(!IsModerator(execution, Owner.Id, out var a, out Error))
					return;

				a = execution.Sites.Affect(a.Id);
				a.Files = [..a.Files, f.Id];
				execution.Allocate(a, a, Data.Length);
				break;
			}

			default:
			{
				Error = InvalidOwnerAddress;
				return;
			}
		}
	}
}