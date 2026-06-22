using System.Text;

namespace Uccs.Fair;

public class PublicationPrioritization : FairOperation
{
	public AutoId				Publication { get; set; }

	public override string		Explanation => $"Publication={Publication}";


	public PublicationPrioritization()
	{
	}

	public PublicationPrioritization(AutoId id)
	{
		Publication = id;
	}
	
	public override bool IsValid(McvNet net)
	{
		return true;
	}

	public override void Read(Reader reader)
	{
		Publication	= reader.Read<AutoId>();
	}

	public override void Write(Writer writer)
	{
		writer.Write(Publication);
	}

	public override void Execute(FairExecution execution)
	{
		if(!CanAccessPublication(execution, Publication, out var p, out var a, out var r,  out Error))
			return;

		r = execution.Products.Affect(r.Id);

		r.Publications = [p.Id, ..r.Publications.Remove(p.Id)];

		a = execution.Authors.Affect(a.Id);
		execution.PayOperationEnergy(a);
	}
}