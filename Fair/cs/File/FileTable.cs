namespace Uccs.Fair;

public class FileTable : Table<AutoId, File>
{
	public override string			Name => FairTable.File.ToString();
	public IEnumerable<FairRound>	Tail => Mcv.Tail.Cast<FairRound>();
	public new FairMcv				Mcv => base.Mcv as FairMcv;

	public FileTable(FairMcv rds) : base(rds)
	{
	}
	
	public override File Create()
	{
		return new File(Mcv);
	}
 }

public class FileExecution : TableExecution<AutoId, File>
{
	public FileExecution(FairExecution execution) : base(execution.Mcv.Files, execution)
	{
	}

	public File Create(AutoId owner)
	{
		int e = Execution.GetNextEid(Table, owner.B);

		var a = Table.Create();
		a.Id = LastCreatedId = new AutoId(owner.B, e);

		return Affected[a.Id] = a;
	}
}
