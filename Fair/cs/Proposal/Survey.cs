using Uccs;
using Uccs.Fair;

public abstract class SiteOperation : FairOperation
{
	public Site	Site;
}

public class SurveyOption : IBinarySerializable
{
	public SiteOperation		Operation { get; set; }
	public AutoId[]				Yes { get; set; }

	public SurveyOption()
	{
	}

	public SurveyOption(SiteOperation option)
	{
		Operation	= option;
		Yes			= [];
	}

	public void Read(BinaryReader reader)
	{
		Operation = Fair.OContructors[typeof(Operation)][reader.ReadUInt32()].Invoke(null) as SiteOperation;
 		Operation.Read(reader); 
 		Yes = reader.ReadArray<AutoId>();
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(Fair.OCodes[Operation.GetType()]);
		Operation.Write(writer);
 		writer.Write(Yes);
	}
}

public class PerpetualSurvey : IBinarySerializable
{
	public SurveyOption[]		Options { get; set; }
	public sbyte				LastWin { get; set; }
	public AutoId[]				Comments;
	
	public sbyte				FindIndex(ApprovalPolicy policy) => (sbyte)Array.FindIndex(Options, i => i.Operation is SitePolicyChange o && o.Approval == policy);

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

	public void ReadMain(BinaryReader reader)
	{
		Read(reader);
	}

	public void WriteMain(BinaryWriter writer)
	{
		Write(writer);
	}

	public void Read(BinaryReader reader)
	{
		LastWin			= reader.ReadSByte();
		Options			= reader.ReadArray<SurveyOption>();
		Comments		= reader.ReadArray<AutoId>();
	}

	public void Write(BinaryWriter writer)
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
//	public void ReadMain(BinaryReader reader)
//	{
//		Read(reader);
//	}
//
//	public void WriteMain(BinaryWriter writer)
//	{
//		Write(writer);
//	}
//
//	public void Read(BinaryReader reader)
//	{
//		LastWin			= reader.ReadSByte();
//		Options			= reader.ReadArray<SurveyOption>();
//		Comments		= reader.ReadArray<AutoId>();
//	}
//
//	public void Write(BinaryWriter writer)
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
