﻿using System.Text;

namespace Uccs.Fair;

public class SiteTextChange : VotableOperation
{
	public string				Title { get; set; }
	public string				Slogan { get; set; }
	public string				Description { get; set; }

	public override bool		IsValid(McvNet net) =>	(Title == null || Title.Length <= Fair.TitleLengthMaximum) && 
														(Slogan == null || Slogan.Length <= Fair.SloganLengthMaximum) && 
														(Description == null || Description.Length <= Fair.PostLengthMaximum);
	public override string		Explanation => $"Description={Description}, Slogan={Slogan}, Description={Description}";
	
	public override void Read(BinaryReader reader)
	{
		Title		= reader.ReadUtf8Nullable();
		Slogan		= reader.ReadUtf8Nullable();
		Description	= reader.ReadUtf8Nullable();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.WriteUtf8Nullable(Title);
		writer.WriteUtf8Nullable(Slogan);
		writer.WriteUtf8Nullable(Description);
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
			execution.SiteTitles.Deindex(Site.Id, Site.Title);
			execution.SiteTitles.Index(Site.Id, Title);

			Site.Title = Title;
		}

		if(Slogan != null)
		{
			Site.Slogan = Slogan;
		}

		if(Description != null)
		{
			if(Description != null)
				execution.Free(Site, Site, Encoding.UTF8.GetByteCount(Site.Description));

			Site.Description = Description;
			execution.Allocate(Site, Site, Encoding.UTF8.GetByteCount(Description));
		}
	}
}