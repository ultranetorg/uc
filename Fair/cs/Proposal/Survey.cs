using System.Collections.Immutable;
using Uccs;

namespace Uccs.Fair;

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

public class Survey : IBinarySerializable
{
	public AutoId						Id { get; set; }
	public ImmutableList<SurveyOption>	Options { get; set; }
	public sbyte						LastWin { get; set; }
	public AutoId[]						Comments;
	
	public sbyte						FindIndex(ApprovalRequirement policy) => (sbyte)Options.FindIndex(i => i.Operation is StoreApprovalPolicyChange o && o.Approval == policy);

	public Survey()
	{
	}

	public Survey Clone()
	{
		var a = new Survey()
				{	
					Id			= Id,
					LastWin		= LastWin,
					Options		= Options,
					Comments	= Comments
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
		Id				= reader.Read<AutoId>();
		LastWin			= reader.ReadSByte();
		Options			= reader.ReadImmutableList<SurveyOption>();
		Comments		= reader.ReadArray<AutoId>();
	}

	public void Write(Writer writer)
	{
		writer.Write(Id);
		writer.Write(LastWin);
		writer.Write(Options);
		writer.Write(Comments);
	}

	public void Cleanup(Round lastInCommit)
	{
	}
}
