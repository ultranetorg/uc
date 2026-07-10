using Uccs;
using Uccs.Fair;

public class SurveyOption : IBinarySerializable
{
	public StoreOperation		Operation { get; set; }
	public AutoId[]				Yes { get; set; }

	public SurveyOption()
	{
	}

	public SurveyOption(StoreOperation option)
	{
		Operation	= option;
		Yes			= [];
	}

	public void Read(Reader reader)
	{
		Operation = reader.ReadVirtual<Operation>() as StoreOperation;
 		Yes = reader.ReadArray<AutoId>();
	}

	public void Write(Writer writer)
	{
		writer.WriteVirtual(Operation);
 		writer.Write(Yes);
	}
}

public class PerpetualSurvey : IBinarySerializable
{
	public SurveyOption[]		Options { get; set; }
	public sbyte				LastWin { get; set; }
	public AutoId[]				Comments;
	
	public sbyte				FindIndex(ApprovalRequirement policy) => (sbyte)Array.FindIndex(Options, i => i.Operation is StoreApprovalPolicyChange o && o.Approval == policy);

	public PerpetualSurvey()
	{
	}

	public PerpetualSurvey Clone()
	{
		var a = new PerpetualSurvey()
				{	
					LastWin			= LastWin,
					Options			= Options,
					Comments		= Comments
				};

		return a;
	}

	public void ReadMain(Reader reader)
	{
		Read(reader);
	}

	public void WriteMain(Writer writer)
	{
		Write(writer);
	}

	public void Read(Reader reader)
	{
		LastWin			= reader.ReadSByte();
		Options			= reader.ReadArray<SurveyOption>();
		Comments		= reader.ReadArray<AutoId>();
	}

	public void Write(Writer writer)
	{
		writer.Write(LastWin);
		writer.Write(Options);
		writer.Write(Comments);
	}

	public void Cleanup(Round lastInCommit)
	{
	}
}

//public class Survey : IBinarySerializable
//{
//	public SurveyOption		Options { get; set; }
//		
//	public PerpetualSurvey()
//	{
//	}
//
//	public PerpetualSurvey Clone()
//	{
//		var a = new PerpetualSurvey()
//				{	
//					LastWin			= LastWin,
//					Options			= Options,
//					Comments		= Comments
//				};
//
//		return a;
//	}
//
//	public void ReadMain(Reader reader)
//	{
//		Read(reader);
//	}
//
//	public void WriteMain(Writer writer)
//	{
//		Write(writer);
//	}
//
//	public void Read(Reader reader)
//	{
//		LastWin			= reader.ReadSByte();
//		Options			= reader.ReadArray<SurveyOption>();
//		Comments		= reader.ReadArray<AutoId>();
//	}
//
//	public void Write(Writer writer)
//	{
//		writer.Write(LastWin);
//		writer.Write(Options);
//		writer.Write(Comments);
//	}
//
//	public void Cleanup(Round lastInCommit)
//	{
//	}
//}
