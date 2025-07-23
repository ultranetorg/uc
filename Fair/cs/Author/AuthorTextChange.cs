using System.Text;

namespace Uccs.Fair;

public class AuthorTextChange : FairOperation
{
	public AutoId				Author { get; set; }
	public string				Title { get; set; }
	public string				Description { get; set; }

	public override bool		IsValid(McvNet net) =>	(Title == null || Title.Length <= Fair.TitleLengthMaximum) && 
														(Description == null || Description.Length <= Fair.PostLengthMaximum);
	public override string		Explanation => $"Description={Description}, Description={Description}";
	
	public override void Read(BinaryReader reader)
	{
		Author		= reader.Read<AutoId>();
		Title		= reader.ReadUtf8Nullable();
		Description	= reader.ReadUtf8Nullable();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Author);
		writer.WriteUtf8Nullable(Title);
		writer.WriteUtf8Nullable(Description);
	}

	public override void Execute(FairExecution execution)
	{
		if(!CanAccessAuthor(execution, Author, out var a, out Error))
			return;

		a = execution.Authors.Affect(Author);

		if(Title != null)
		{
			a.Title = Title;
		}

		if(Description != null)
		{
			if(a.Description != null)
				execution.Free(a, a, Encoding.UTF8.GetByteCount(a.Description));

			a.Description = Description;
			execution.Allocate(a, a, Encoding.UTF8.GetByteCount(Description));
		}

		execution.PayCycleEnergy(a);
	}
}