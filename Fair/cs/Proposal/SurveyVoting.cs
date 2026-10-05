using System.Collections.Immutable;

namespace Uccs.Fair;

public class SurveyVoting : FairOperation
{
	public AutoId				Store { get; set; }
	public AutoId				Survey { get; set; }
	public AutoId				Publisher { get; set; }
	public sbyte				Choice { get; set; }

	public override bool		IsValid(McvNet net) => Choice >= (sbyte)SpecialChoice._First && Store != null && Publisher != null;
	public override string		Explanation => $"{nameof(Survey)}={Survey}, {nameof(Publisher)}={Publisher}, {nameof(Choice)}={Choice}";

	public SurveyVoting()
	{
	}

	public SurveyVoting(AutoId survey, AutoId voter, sbyte choice)
	{
		Survey = survey;
		Publisher = voter;
		Choice = choice;
	}

	public override void Read(Reader reader)
	{
		Store		= reader.Read<AutoId>();
		Survey		= reader.Read<AutoId>();
		Publisher	= reader.Read<AutoId>();
		Choice		= reader.ReadSByte();
	}

	public override void Write(Writer writer)
	{
		writer.Write(Store);
		writer.Write(Survey);
		writer.Write(Publisher);
		writer.Write(Choice);
	}

	public override void Execute(FairExecution execution)
	{
		if(!StoreExists(execution, Store, out var s, out Error))
			return;
 
		if(!IsPublisher(execution, s, Publisher, out var _, out Error))
			return;

		var r = s.Surveys.Find(i => i.Id == Survey);

		if(r == null)
		{
 			Error = NotFound;
 			return;
		}

		if(Choice >= r.Options.Count)
		{
 			Error = OutOfBounds;
 			return;
		}

		s = execution.Stores.Affect(s.Id);

		var o = r.Options[Choice];
 
 		var oprev = r.Options.Find(i => i.Yes.Contains(Publisher));
		 
		bool won(AutoId[] votes)
		{
 			return votes.Length >= s.Publishers.Length/2 + (s.Publishers.Length & 1);
		}

		s.Surveys = s.Surveys.Replace(r, r = new Survey {Id = r.Id, Options = r.Options, LastWin = r.LastWin, Comments = r.Comments});

		if(oprev != null)
		{
			r.Options = r.Options.Replace(oprev, new SurveyOption {Operation = oprev.Operation, Yes = oprev.Yes.Remove(Publisher)});
		}

		r.Options = r.Options.Replace(o, o = new SurveyOption {Operation = o.Operation, Yes = [..o.Yes, Publisher]});

 		if(won(r.Options[Choice].Yes))
 		{
			o.Operation.Store = s;

			o.Operation.Execute(execution);
			r.LastWin = Choice;

			if(o.Operation is StoreModeratorRemoval)
			{
				s.Surveys = s.Surveys.Remove(r);
			}
		}

		var a = execution.Authors.Affect(Publisher);
		execution.PayOperationEnergy(a);
	}
}