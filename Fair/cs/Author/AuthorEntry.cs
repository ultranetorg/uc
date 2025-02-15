﻿namespace Uccs.Fair;

public class AuthorEntry : Author, ITableEntry
{
	Mcv						Mcv;
	public bool				Deleted { get; set; }
	public EntityId[]		Products { get; set; }

	public AuthorEntry()
	{
	}

	public AuthorEntry(Mcv chain)
	{
		Mcv = chain;
	}

	public override string ToString()
	{
		return $"{Id}, {Owner}, {Expiration}";
	}

	public AuthorEntry Clone()
	{
		return new AuthorEntry(Mcv){Id					= Id,
									Title				= Title,
									Owner				= Owner,
									Expiration			= Expiration,
									Space				= Space,
									Spacetime					= Spacetime,
									Energy					= Energy,
									EnergyThisPeriod		= EnergyThisPeriod,
									EnergyNext				= EnergyNext,
									ModerationReward	= ModerationReward,

									Products			= Products};
	}

	public void WriteMain(BinaryWriter writer)
	{
		Write(writer);
		writer.Write(Products);
	}

	public void ReadMain(BinaryReader reader)
	{
		Read(reader);
		Products = reader.ReadArray<EntityId>();
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
