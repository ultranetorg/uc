﻿namespace Uccs.Fair;

public class SiteEntry : Site, ITableEntry
{
	public bool				Deleted { get; set; }
	FairMcv					Mcv;

	public SiteEntry()
	{
	}

	public SiteEntry(FairMcv mcv)
	{
		Mcv = mcv;
	}

	public SiteEntry Clone()
	{
		return new(Mcv){Id = Id,
						Title = Title,
						Moderators = Moderators,
						Categories = Categories};
	}

	public void ReadMain(BinaryReader reader)
	{
		Read(reader);
	}

	public void WriteMain(BinaryWriter writer)
	{
		Write(writer);
	}

	public void WriteMore(BinaryWriter w)
	{
	}

	public void ReadMore(BinaryReader r)
	{
	}

	public void Cleanup(Round lastInCommit)
	{
	}

}

