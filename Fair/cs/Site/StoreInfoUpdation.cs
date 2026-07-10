using System.Text;

namespace Uccs.Fair;

public class StoreInfoUpdation : VotableOperation
{
	public string				Title { get; set; }
	public string				Slogan { get; set; }
	public string				Description { get; set; }

	public override bool		IsValid(McvNet net) =>	(Title == null || Title.Length <= Fair.TitleLengthMaximum) && 
														(Slogan == null || Slogan.Length <= Fair.SloganLengthMaximum) && 
														(Description == null || Description.Length <= Fair.PostLengthMaximum);
	public override string		Explanation => $"Title={Title}, Slogan={Slogan}, Description={Description}";
	
	public override void Read(Reader reader)
	{
		Title		= reader.ReadUtf8();
		Slogan		= reader.ReadUtf8();
		Description	= reader.ReadUtf8();
	}

	public override void Write(Writer writer)
	{
		writer.WriteUtf8(Title);
		writer.WriteUtf8(Slogan);
		writer.WriteUtf8(Description);
	}

	public override bool Overlaps(VotableOperation other)
	{
		return false;
	}

 	public override bool ValidateProposal(FairExecution execution, out string error)
 	{
		error = null;
		return true;
 	}

	public override void Execute(FairExecution execution)
	{
		if(Title != null)
		{
			execution.StoreTitles.Deindex(Store.Id, Store.Title);
			execution.StoreTitles.Index(Store.Id, Title);

			Store.Title = Title;
		}

		if(Slogan != null)
		{
			Store.Slogan = Slogan;
		}

		if(Description != null)
		{
			if(Store.Description != null)
				execution.Free(Store, Store, Encoding.UTF8.GetByteCount(Store.Description));

			Store.Description = Description;
			execution.Allocate(Store, Store, Encoding.UTF8.GetByteCount(Description));
		}
	}
}