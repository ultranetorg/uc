namespace Uccs.Fair;

public class PerpetualVoting : FairOperation
{
	public AutoId				Site { get; set; }
	public sbyte				Referendum { get; set; }
	public AutoId				Publisher { get; set; }
	public sbyte				Choice { get; set; }

	public override bool		IsValid(McvNet net) => Choice >= (sbyte)SpecialChoice._First;
	public override string		Explanation => $"Referendum={Referendum}, Voter={Publisher}, Choice={Choice}";

	public PerpetualVoting()
	{
	}

	public PerpetualVoting(sbyte proposal, AutoId voter, sbyte choice)
	{
		Referendum = proposal;
		Publisher = voter;
		Choice = choice;
	}

	public override void Read(BinaryReader reader)
	{
		Site		= reader.Read<AutoId>();
		Referendum	= reader.ReadSByte();
		Publisher	= reader.Read<AutoId>();
		Choice		= reader.ReadSByte();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Site);
		writer.Write(Referendum);
		writer.Write(Publisher);
		writer.Write(Choice);
	}

	public override void Execute(FairExecution execution)
	{
		if(!SiteExists(execution, Site, out var s, out Error))
			return;
 
		if(!IsPublisher(execution, s, Publisher, out var x, out Error))
			return;

		if(Referendum >= s.PerpetualSurveys.Length)
		{
 			Error = NotFound;
 			return;
		}

		var r = s.PerpetualSurveys[Referendum];

		if(Choice >= r.Options.Length)
		{
 			Error = NotFound;
 			return;
		}

		var o = r.Options[Choice];
 
 		var old = Array.FindIndex(r.Options, i => i.Yes.Contains(Publisher));

		s = execution.Sites.Affect(s.Id);
		 
		bool won(AutoId[] votes)
		{
 			return votes.Length >= s.Publishers.Length/2 + (s.Publishers.Length & 1);
		}


		s.PerpetualSurveys				= [..s.PerpetualSurveys];
		s.PerpetualSurveys[Referendum]  = r = new PerpetualSurvey {Options	= [..r.Options], LastWin = r.LastWin, Comments = r.Comments};

		if(old != -1)
		{
			var oo = s.PerpetualSurveys[Referendum].Options[old];

			s.PerpetualSurveys[Referendum].Options[old] = new SurveyOption {Operation = oo.Operation, Yes = oo.Yes.Remove(Publisher)};
		}

		s.PerpetualSurveys[Referendum].Options[Choice]	= o = new SurveyOption {Operation = o.Operation, Yes = [..o.Yes, Publisher]};

 		if(won(r.Options[Choice].Yes))
 		{
			o.Operation.Site = s;

			o.Operation.Execute(execution);
			r.LastWin = (sbyte)Choice;
		}

		var a = execution.Authors.Affect(Publisher);
		execution.PayCycleEnergy(a);
	}
}