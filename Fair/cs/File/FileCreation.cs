using System.Text;

namespace Uccs.Fair;

public class FileCreation : FairOperation
{
	public EntityAddress		Owner { get; set; }
	public byte[]				Data { get; set; }
	public FairMime				Mime { get; set; }

	public override string		Explanation => $"Owner={Owner}, Data={Data.Length}";
	public FileCreation()
	{
	}

	public FileCreation(EntityAddress owner, byte[] data, FairMime mime)
	{
		Owner = owner;
		Data = data;
		Mime = mime;
	}

	public FileCreation(FairTable table, AutoId id, byte[] data, FairMime mime)
	{
		Owner = new EntityAddress((byte)table, id);
		Data = data;
		Mime = mime;
	}

	public override bool IsValid(McvNet net)
	{
		if(Data.Length > (net as Fair).FileLengthMaximum && !Enum.IsDefined(Mime))
			return false;

		if(Mime == FairMime.ImageJpg || Mime == FairMime.ImagePng)
		{
			if(File.GetImageFormat(Data) != Mime)
				return false;
		}

		return true;
	}

	public override void Read(Reader reader)
	{
		Owner	= reader.Read<EntityAddress>();
		Data	= reader.ReadBytes();
		Mime	= reader.Read<FairMime>();
	}

	public override void Write(Writer writer)
	{
		writer.Write(Owner);
		writer.WriteBytes(Data);
		writer.Write(Mime);
	}

	public override void Execute(FairExecution execution)
	{
		var f = execution.Files.Create(Owner.Id);

		f.Owner		= Owner;
		f.Refs		= 0;
		f.Data		= Data;
		f.Mime		= Mime;

		switch((FairTable)Owner.Table)
		{
			case FairTable.Author:
			{
				if(!CanAccessAuthor(execution, Owner.Id, out var a, out Error))
					return;

				a = execution.Authors.Affect(a.Id);
				a.Files = [..a.Files, f.Id];
				execution.Allocate(a, a, execution.Net.EntityLength + Data.Length);
				break;
			}

			case FairTable.Store:
			{
				if(!IsModerator(execution, Owner.Id, out var a, out Error))
					return;

				a = execution.Stores.Affect(a.Id);
				a.Files = [..a.Files, f.Id];
				execution.Allocate(a, a, execution.Net.EntityLength + Data.Length);
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